﻿using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using Utopia.Shared.Chunks;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Server.Structs;
using Utopia.Shared.Services;
using Utopia.Shared.Structs;
using Utopia.Shared.ClassExt;

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

        private TimeSpan _chunkUpdateCycle = TimeSpan.FromDays(1); // A minimum of one day must be passed before a chunk can be do a spawn refresh again !
        private int _maxChunkRefreshPerCycle = 20; //Maximum of 20 chunk update per cycle

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
                _server.Clock.ClockTimers.Add(new Clock.GameClockTimer(0, 0, 0, 15, server.Clock, UtopiaSpawningLookup));    
            }
        }
        
        /// <summary>
        /// Method responsible to do chunk spawn logic
        /// </summary>
        private void UtopiaSpawningLookup(DateTime gametime)
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

                    bool isDayTime = gametime.TimeOfDay > TimeSpan.FromHours(8) && gametime.TimeOfDay < TimeSpan.FromHours(20);
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
                    if (_entitySpawningControler.TryGetSpawnLocation(spawnableEntity, chunk, chunkCursor, _fastRandom, out entityLocation))
                    {
                        var entity = _server.EntityFactory.CreateFromBluePrint(spawnableEntity.BluePrintId);
                        var staticEntity = entity as IStaticEntity;

                        //Check the maximum amount of static entities;
                        int MaxEntityAmount;
                        chunk.BlockData.ChunkMetaData.InitialSpawnableEntitiesAmount.TryGetValue(spawnableEntity.BluePrintId, out MaxEntityAmount);
                        if (MaxEntityAmount == 0) 
                            continue;

                        if (chunk.Entities.Where(e => e.BluePrintId == spawnableEntity.BluePrintId).CountAtLeast(MaxEntityAmount))
                            continue;

                        if (staticEntity != null)
                        {
                            staticEntity.Position = entityLocation;

                            var cursor = _server.LandscapeManager.GetCursor(entityLocation);
                            logger.Debug("Spawning new static entity : {0} at location {1}", staticEntity.Name, entityLocation);
                            cursor.AddEntity(staticEntity);
                        }

                        var charEntity = entity as CharacterEntity;
                        if (charEntity != null)
                        {
                            var radius = spawnableEntity.DynamicEntitySpawnRadius;
                            if (radius < 8) 
                                continue; //A minimum radius of 8 blocks distance is needed (= 1 chunk), otherwhile it could create new everytime.
                            if (_server.AreaManager.EnumerateAround(entityLocation, radius).CountAtLeast(spawnableEntity.MaxEntityAmount))
                                continue;
                            

                            charEntity.Position = entityLocation;
                            logger.Debug("Spawning new dynamic entity : {0} at location {1}", charEntity.Name, entityLocation);
                            _server.EntityManager.AddNpc(charEntity);
                        }
                    }
                }

                chunk.LastSpawningRefresh = gametime;
            }

            if (_chunks4Processing.Count < _maxChunkRefreshPerCycle) _chunks4Processing.Clear();
            else _chunks4Processing.RemoveRange(0, _maxChunkRefreshPerCycle);
        }

    }
}
