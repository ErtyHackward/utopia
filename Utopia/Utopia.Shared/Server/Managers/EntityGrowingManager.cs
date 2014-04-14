using System;
using System.Linq;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Server.Structs;
using Utopia.Shared.Services;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Managers
{
    public class EntityGrowingManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private ServerCore _server;
        
        public EntityGrowingManager(ServerCore server)
        {
            _server = server;

            if (_server != null)
                _server.Clock.CreateNewTimer(new Clock.GameClockTimer(UtopiaTimeSpan.FromMinutes(15), server.Clock, GrowingLookup));
        }

        private void GrowingLookup(UtopiaTime gametime)
        {
            var random = new Random();

            foreach (var chunk in _server.LandscapeManager.GetBufferedChunks())
            {
                var growingEntities = chunk.Entities.OfType<GrowingEntity>().ToList();

                foreach (var entity in growingEntities)
                {
                    var now = _server.Clock.Now;
                    
                    // fix time cheat
                    if (now > entity.LastGrowUpdate)
                        EntityGrowCheck(_server.Clock.Now, entity, chunk, random);
                    
                    entity.LastGrowUpdate = now;
                }
            }
        }

        public void EntityGrowCheck(UtopiaTime now, GrowingEntity entity, ServerChunk chunk, Random random)
        {
            if (entity.LastGrowUpdate.IsZero)
                return;

            var checkTimeSpan = now - entity.LastGrowUpdate;

            bool updated;
            bool rotten;

            // grow time left at the current season
            var tillTheEndOfSeason = UtopiaTimeSpan.FromSeasons(1d - entity.LastGrowUpdate.TotalSeasons % 1d);

            if (checkTimeSpan <= tillTheEndOfSeason)
            {
                // small grow update
                updated = GrowSeasonLogic(entity, now.Season, checkTimeSpan, random, chunk, out rotten);

                if (rotten)
                    return;
            }
            else
            {
                // grow at the end of the first season
                updated = GrowSeasonLogic(entity, now.Season, tillTheEndOfSeason, random, chunk, out rotten);

                if (rotten)
                    return;

                // align time to the beginning of next season
                checkTimeSpan -= tillTheEndOfSeason;

                while (checkTimeSpan.TotalSeconds > 0)
                {
                    var seasonCheckSpan = checkTimeSpan.TotalSeasons > 0 ? UtopiaTimeSpan.FromSeasons(1) : checkTimeSpan;

                    updated = GrowSeasonLogic(entity, (now - checkTimeSpan).Season, seasonCheckSpan, random, chunk, out rotten) || updated;

                    if (rotten)
                        return;

                    checkTimeSpan -= seasonCheckSpan;
                }
            }

            if (updated)
            {
                if (entity is PlantGrowingEntity)
                {
                    if (chunk != null)
                    {
                        chunk.Entities.RemoveById(entity.StaticId);
                        chunk.Entities.Add(entity);
                    }
                }
                var tree = entity as TreeGrowingEntity;
                if (tree != null)
                {
                    var treeBlueprint = _server.EntityFactory.Config.TreeBluePrints[tree.TreeTypeId];

                    if (tree.CurrentGrowTime > treeBlueprint.GrowTime)
                    {
                        // the tree is ready

                        // create tree blocks
                        var rootOffset = -tree.VoxelModel.States[0].PartsStates[0].Translation;
                        var cursor = _server.LandscapeManager.GetCursor(tree.Position);
                        var frame = tree.VoxelModel.Frames[0];
                        var range = new Range3I(new Vector3I(), frame.BlockData.ChunkSize);
                        
                        foreach (var position in range)
                        {
                            var value = frame.BlockData.GetBlock(position);
                            if (value == 0)
                                continue;
                            var blockType = value == 1 ? treeBlueprint.TrunkBlock : treeBlueprint.FoliageBlock;
                            var worldPos = (Vector3I)(tree.Position + rootOffset);
                            cursor.GlobalPosition = worldPos;
                            if (cursor.Read() == WorldConfiguration.CubeId.Air)
                            {
                                cursor.Write(blockType);
                            }
                        }

                        // create tree soul
                        var soul = _server.EntityFactory.CreateEntity<TreeSoul>();
                        soul.Position = tree.Position;
                        soul.TreeRndSeed = tree.TreeRndSeed;
                        soul.TreeBlueprintIndex = tree.TreeTypeId;

                        chunk.Entities.Add(soul);

                        // remove the growing tree
                        chunk.Entities.RemoveById(tree.StaticId);
                    }
                    else
                    {
                        // just make the model bigger
                        tree.Scale = (float)tree.CurrentGrowTime.TotalSeconds / treeBlueprint.GrowTime.TotalSeconds;
                        chunk.Entities.RemoveById(tree.StaticId);
                        chunk.Entities.Add(tree);
                    }
                }
            }
        }

        /// <summary>
        /// Grow at a season
        /// </summary>
        private bool GrowSeasonLogic(GrowingEntity entity, Season season, UtopiaTimeSpan passedTime, Random random, ServerChunk chunk, out bool rotten)
        {
            bool updated = false;
            rotten = false;

            // constranits check
            if (entity.GrowingSeasons.Count > 0 && !entity.GrowingSeasons.Contains(season.Name))
                return false;

            if (entity.GrowingBlocks.Count > 0 && _server != null)
            {
                if (entity.Linked)
                {
                    var cursor = _server.LandscapeManager.GetCursor(entity.LinkedCube);
                    if (!entity.GrowingBlocks.Contains(cursor.Read()))
                        return false;
                }
                else
                {
                    var cursor = _server.LandscapeManager.GetCursor(entity.Position);
                    if (!entity.GrowingBlocks.Contains(cursor.PeekValue(Vector3I.Down)))
                        return false;
                }
            }

            // TODO: check light constraint when implemented

            entity.CurrentGrowTime += passedTime;

            var plant = entity as PlantGrowingEntity;

            if (plant != null)
            {
                // update entity to the actual state 
                while (!plant.IsLastGrowLevel)
                {
                    var currentLevel = plant.CurrentGrowLevel;

                    if (plant.CurrentGrowTime < currentLevel.GrowTime)
                        break;

                    if (plant.CurrentGrowLevelIndex == 0 && plant.RottenChance != 0f)
                    {
                        if (random.NextDouble() < plant.RottenChance)
                        {
                            if (chunk != null)
                                chunk.Entities.RemoveById(plant.StaticId);
                            rotten = true;
                            return true;
                        }
                    }

                    plant.CurrentGrowTime -= currentLevel.GrowTime;
                    plant.CurrentGrowLevelIndex++;
                    updated = true;
                }
            }

            var tree = entity as TreeGrowingEntity;

            if (tree != null)
            {
                updated = true;
            }

            return updated;
        }
    }
}
