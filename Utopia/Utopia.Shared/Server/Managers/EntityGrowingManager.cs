using System;
using System.Linq;
using S33M3Resources.Structs;
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
                if (entity is TreeGrowingEntity)
                {
                    // TODO: tree grow logic
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

            // update entity to the actual state 
            while (!entity.IsLastGrowLevel)
            {
                var currentLevel = entity.CurrentGrowLevel;

                if (entity.CurrentGrowTime < currentLevel.GrowTime)
                    break;

                if (entity.CurrentGrowLevelIndex == 0 && entity.RottenChance != 0f)
                {
                    if (random.NextDouble() < entity.RottenChance)
                    {
                        if (chunk != null)
                            chunk.Entities.RemoveById(entity.StaticId);
                        rotten = true;
                        return true;
                    }
                }

                entity.CurrentGrowTime -= currentLevel.GrowTime;
                entity.CurrentGrowLevelIndex++;
                updated = true;
            }


            return updated;
        }
    }
}
