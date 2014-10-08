using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.LandscapeEntities;
using Utopia.Shared.LandscapeEntities.Trees;
using Utopia.Shared.Server.Structs;
using Utopia.Shared.Services;
using Utopia.Shared.Structs;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.Server.Managers
{
    public class EntitySpawningManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ServerCore _server;
        private readonly ServerLandscapeManager _landscapeManager;
        private readonly IEntitySpawningControler _entitySpawningControler;
        private readonly UtopiaWorldConfiguration _configuration;
        private readonly FastRandom _fastRandom;
        private readonly List<ServerChunk> _chunks4Processing = new List<ServerChunk>();
        private TreeLSystem _treeLSystem = new TreeLSystem();

        private UtopiaTimeSpan _chunkUpdateCycle = UtopiaTimeSpan.FromDays(1); // A minimum of one day must be passed before a chunk can be do a spawn refresh again !
        private int _maxChunkRefreshPerCycle = 50; //Maximum of 50 chunk update per cycle

        public bool DisableNPCSpawn { get; set; }

        public EntitySpawningManager(ServerCore server, IEntitySpawningControler entitySpawningControler)
        {
            _server = server;
            _entitySpawningControler = entitySpawningControler;
            _configuration = _server.WorldParameters.Configuration as UtopiaWorldConfiguration;
            _landscapeManager = server.LandscapeManager;
            _fastRandom = new FastRandom();

            //This spawn logic can only be down on UtopiaWorldConfiguration and associated processor.
            if (_configuration != null)
            {
                _server.Clock.CreateNewTimer(new Clock.GameClockTimer(UtopiaTimeSpan.FromMinutes(30), server.Clock, UtopiaSpawningLookup));    
            }
        }
        
        /// <summary>
        /// Method responsible to do chunk spawn logic
        /// </summary>
        private void UtopiaSpawningLookup(UtopiaTime gametime)
        {
            //Logic to "Randomize" and limit the number of chunk to update per cycle ======================================================

            //Get the chunks that are under server management and are candidate for a refresh !
            var serverChunksToUpdate = _landscapeManager.GetBufferedChunks().Where(x => (gametime - x.LastSpawningRefresh) > _chunkUpdateCycle).ToList();

            //Get the chunk not in the processing list => New chunk to process
            var newChunks = serverChunksToUpdate.Where(x => _chunks4Processing.Contains(x) == false).ToList();
            if (newChunks.Count > 0)
            {
                _chunks4Processing.AddRange(newChunks);
                _chunks4Processing.Shuffle(); //Shuffle all chunks that must be updated.
            }
            
            //Get the chunk not handled anymore by the server and remove them from the processing list
            _chunks4Processing.RemoveAll(c => serverChunksToUpdate.Contains(c) == false);

            //Process _maxChunkRefreshPerCycle at maximum
            foreach (var chunk in _chunks4Processing.Take(_maxChunkRefreshPerCycle))
            {
                SpawnBiomeEntities(gametime, chunk);
                TreeSoulUpdate(chunk);
                chunk.LastSpawningRefresh = gametime;
            }

            if (_chunks4Processing.Count < _maxChunkRefreshPerCycle) 
                _chunks4Processing.Clear();
            else 
                _chunks4Processing.RemoveRange(0, _maxChunkRefreshPerCycle);
        }

        private void TreeSoulUpdate(ServerChunk chunk)
        {
            foreach (var soul in chunk.Entities.OfType<TreeSoul>().ToList())
            {
                TreeIntegrity(chunk, soul);
                if (!soul.IsDamaged)
                {
                    SpawnSeeds(chunk, soul);
                    SpawnTreeEntities(chunk, soul);
                }
            }
        }

        private void SpawnTreeEntities(ServerChunk chunk, TreeSoul soul)
        {
            var config = _server.EntityFactory.Config;
            var treeBp = config.TreeBluePrintsDico[soul.TreeTypeId];

            if (_server.Clock.Now - soul.LastItemsRegeneration < treeBp.ItemsRegenerationTime)
                return;

            List<BlockWithPosition> treeBlocks = null;

            soul.LastItemsRegeneration = _server.Clock.Now;

            foreach (var item in treeBp.StaticItems)
            {
                // check quantity limit 
                var maxItems = _fastRandom.Next(item.Quantity.Min, item.Quantity.Max + 1);
                if (_server.LandscapeManager.AroundEntities(soul.Position, item.SpawningRange).Count(e => e.BluePrintId == item.ItemblueprintId) >= maxItems)
                    continue;

                var point = _fastRandom.NextVector2IInRadius(item.SpawningRange) + new Vector2I((int)soul.Position.X, (int)soul.Position.Z);

                var groundSpawning = item.SpawningType == SpawningType.Ground || (item.SpawningType == SpawningType.Both && _fastRandom.NextDouble() < 0.5f);

                if (groundSpawning)
                {
                    var cursor = _server.LandscapeManager.GetCursor(new Vector3I(point.X, AbstractChunk.ChunkSize.Y - 1, point.Y));

                    while (true)
                    {
                        var block = cursor.Read();

                        if (block != WorldConfiguration.CubeId.Air && block != treeBp.FoliageBlock)
                            break;

                        cursor.Move(Vector3I.Down);
                    }
                    cursor.Move(Vector3I.Up);

                    if (cursor.Read() == WorldConfiguration.CubeId.Air)
                    {
                        if (Vector3I.Distance(cursor.GlobalPosition, (Vector3I)soul.Position) <= item.SpawningRange)
                        {
                            var entity = (IStaticEntity)_server.EntityFactory.CreateFromBluePrint(item.ItemblueprintId);
                            entity.Position = cursor.GlobalPosition;

                            var blockLinkedItem = entity as IBlockLinkedEntity;
                            if (blockLinkedItem != null)
                            {
                                blockLinkedItem.LinkedCube = cursor.GlobalPosition + Vector3I.Down;
                            }

                            cursor.AddEntity(entity);
                        }
                    }
                    
                }
                else
                {
                    // lazy initialization of tree 
                    treeBlocks = treeBlocks ?? _treeLSystem.Generate(soul.TreeRndSeed, (Vector3I)soul.Position, treeBp);

                    // find the lowest Y in our point
                    var minY = treeBlocks.Where(b => b.WorldPosition.X == point.X && b.WorldPosition.Z == point.Y).Select(b => b.WorldPosition.Y).DefaultIfEmpty().Min();

                    
                    if (minY == 0 || minY <= soul.Position.Y)
                        // the point is outside of tree
                        continue; 

                    var cursor = _server.LandscapeManager.GetCursor(new Vector3I(point.X, minY - 1, point.Y));
                    if (cursor.Read() == WorldConfiguration.CubeId.Air)
                    {
                        var entity = (IStaticEntity)_server.EntityFactory.CreateFromBluePrint(item.ItemblueprintId);
                        entity.Position = cursor.GlobalPosition;

                        var blockLinkedItem = entity as IBlockLinkedEntity;
                        if (blockLinkedItem != null)
                        {
                            blockLinkedItem.LinkedCube = cursor.GlobalPosition + Vector3I.Down;
                        }

                        cursor.AddEntity(entity);
                    }
                }

            }

        }

        private void TreeIntegrity(ServerChunk chunk, TreeSoul soul)
        {
            if (!soul.IsDamaged)
                return;

            // the tree will regenerate or die after one day
            if ((_server.Clock.Now - soul.LastUpdate) < UtopiaTimeSpan.FromDays(1))
                return;

            var config = _server.EntityFactory.Config;
            var treeBp = config.TreeBluePrintsDico[soul.TreeTypeId];
            var treeBlocks = _treeLSystem.Generate(soul.TreeRndSeed, new Vector3I(), treeBp);

            var cursor = _server.LandscapeManager.GetCursor(soul.Position);

            if (soul.IsDying)
            {
                // remove the tree
                using (cursor.TransactionScope())
                {
                    foreach (var blockWithPosition in treeBlocks)
                    {
                        cursor.GlobalPosition = (Vector3I)soul.Position + blockWithPosition.WorldPosition;

                        if (cursor.Read() == blockWithPosition.BlockId)
                        {
                            cursor.Write(WorldConfiguration.CubeId.Air);
                        }
                    }
                }
                chunk.Entities.Remove(soul);
            }
            else
            {
                // restore the tree
                using (cursor.TransactionScope())
                {
                    foreach (var blockWithPosition in treeBlocks)
                    {
                        cursor.GlobalPosition = (Vector3I)soul.Position + blockWithPosition.WorldPosition;

                        if (cursor.Read() == WorldConfiguration.CubeId.Air)
                        {
                            cursor.Write(blockWithPosition.BlockId);
                        }
                    }
                }

                soul.IsDamaged = false;
            }
        }

        private void SpawnSeeds(ServerChunk chunk, TreeSoul soul)
        {
            // TODO: check other constraints (season, maxseeds etc)

            var seeds = chunk.Entities.OfType<TreeGrowingEntity>().Count(e => e.TreeTypeId == soul.TreeTypeId);
            if (seeds == 0)
            {
                var pos = new Vector2I(_fastRandom.Next(chunk.BlockData.ChunkSize.X),
                    _fastRandom.Next(chunk.BlockData.ChunkSize.Z));

                var metaData = chunk.BlockData.GetColumnInfo(pos);

                var cursor =
                    _server.LandscapeManager.GetCursor(BlockHelper.ConvertToGlobal(chunk.Position,
                        new Vector3I(pos.X, metaData.MaxGroundHeight + 1, pos.Y)));

                while (cursor.GlobalPosition.Y > 0)
                {
                    var val = cursor.PeekValue(Vector3I.Down);

                    if (val != WorldConfiguration.CubeId.Air)
                        break;

                    cursor.Move(Vector3I.Down);
                }

                if (cursor.Read() != WorldConfiguration.CubeId.Air)
                {
                    return;
                }

                if (cursor.GlobalPosition.Y == 0)
                    return;

                var config = _server.EntityFactory.Config;
                var treeBp = config.TreeBluePrintsDico[soul.TreeTypeId];
                var treeSeed = _server.EntityFactory.CreateEntity<TreeGrowingEntity>();
                treeSeed.TreeTypeId = soul.TreeTypeId;
                treeSeed.TreeRndSeed = _fastRandom.Next();
                treeSeed.ModelName = treeBp.SeedModel;
                treeSeed.Name = "Seed of " + treeBp.Name;
                treeSeed.IsPickable = true;
                treeSeed.IsPlayerCollidable = true;
                treeSeed.CollisionType = Entity.EntityCollisionType.Model;
                treeSeed.MountPoint = BlockFace.Top;
                treeSeed.Position = cursor.GlobalPosition + new Vector3D(0.5, 0, 0.5);
                treeSeed.LinkedCube = cursor.GlobalPosition - Vector3I.Up;
                treeSeed.BlockFaceCentered = true;
                treeSeed.GrowingSeasons = config.TreeBluePrintsDico[soul.TreeTypeId].GrowingSeasons;
                treeSeed.GrowingBlocks = config.TreeBluePrintsDico[soul.TreeTypeId].GrowingBlocks;
                cursor.AddEntity(treeSeed);
            }
        }

        private void SpawnBiomeEntities(UtopiaTime gametime, ServerChunk chunk)
        {
            var chunkBiome = _configuration.ProcessorParam.Biomes[chunk.BlockData.ChunkMetaData.ChunkMasterBiomeType];

            foreach (var spawnableEntity in chunkBiome.SpawnableEntities)
            {
                bool isStaticEntity = _configuration.BluePrints[spawnableEntity.BluePrintId] is IStaticEntity;

                //Remark : isChunkGenerationSpawning is set to true for static entities, and false for dynamic entities, maybe worth renaming the properties
                //The aim of it is to avoid dynamic entity creation at chunk generation time (Pure chunk).
                //Apply creation constaint :
                //1) if static entity with is isWildChunkNeeded and chunk is not wild => Do nothing
                if (spawnableEntity.IsWildChunkNeeded && chunk.BlockData.ChunkMetaData.IsWild == false)
                    continue;

                if (chunk.PureGenerated && isStaticEntity)
                    continue;

                var isDayTime = gametime.TimeOfDay > UtopiaTimeSpan.FromHours(8) &&
                                gametime.TimeOfDay < UtopiaTimeSpan.FromHours(20);
                // check daytime constraints
                if (!spawnableEntity.SpawningDayTime.HasFlag(ChunkSpawningDayTime.Day) &&
                    isDayTime)
                    continue;

                if (!spawnableEntity.SpawningDayTime.HasFlag(ChunkSpawningDayTime.Night) &&
                    !isDayTime)
                    continue;

                // check season constraint
                var weatherService = _server.Services.GetService<WeatherService>();

                if (weatherService != null && weatherService.CurrentSeason != null &&
                    spawnableEntity.SpawningSeasons.Count > 0 &&
                    !spawnableEntity.SpawningSeasons.Contains(weatherService.CurrentSeason.Name))
                {
                    continue;
                }

                ByteChunkCursor chunkCursor = new ByteChunkCursor(chunk.BlockData.GetBlocksBytes(), chunk.BlockData.ColumnsInfo);
                // ==> Maybe worth to automaticaly create this specialize cursor at server chunk creation ? Multithreading problem ?
                //It is only use for reading chunk block data in fast way
                Vector3D entityLocation;
                if (_entitySpawningControler.TryGetSpawnLocation(spawnableEntity, chunk, chunkCursor, _fastRandom,
                    out entityLocation))
                {
                    var entity = _server.EntityFactory.CreateFromBluePrint(spawnableEntity.BluePrintId);
                    var staticEntity = entity as IStaticEntity;

                    if (staticEntity != null)
                    {
                        //Check the maximum amount of static entities;
                        int maxEntityAmount;
                        chunk.BlockData.ChunkMetaData.InitialSpawnableEntitiesAmount.TryGetValue(spawnableEntity.BluePrintId,
                            out maxEntityAmount);
                        if (maxEntityAmount == 0)
                            continue;
                        if (chunk.Entities.Where(e => e.BluePrintId == spawnableEntity.BluePrintId)
                            .CountAtLeast(maxEntityAmount))
                            continue;

                        staticEntity.Position = entityLocation;

                        var cursor = _server.LandscapeManager.GetCursor(entityLocation);
                        logger.Debug("Spawning new static entity : {0} at location {1}", staticEntity.Name, entityLocation);

                        var blockLinkedItem = staticEntity as IBlockLinkedEntity;
                        if (blockLinkedItem != null)
                        {
                            blockLinkedItem.LinkedCube = (Vector3I)entityLocation + Vector3I.Down;
                        }

                        cursor.AddEntity(staticEntity);
                    }

                    var charEntity = entity as CharacterEntity;
                    if (charEntity != null)
                    {
                        if (DisableNPCSpawn)
                            continue;

                        var radius = Math.Max(8, spawnableEntity.DynamicEntitySpawnRadius);
                        if (
                            _server.AreaManager.EnumerateAround(entityLocation, radius)
                                .CountAtLeast(spawnableEntity.MaxEntityAmount))
                            continue;

                        charEntity.Position = entityLocation;
                        logger.Debug("Spawning new dynamic entity : {0} at location {1}", charEntity.Name, entityLocation);
                        _server.EntityManager.AddNpc(charEntity);
                    }
                }
            }
        }
    }
}
