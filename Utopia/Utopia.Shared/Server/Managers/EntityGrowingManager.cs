using System;
using System.Linq;
using System.Windows.Forms.VisualStyles;
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

        #region Private variable
        private ServerCore _server;
        #endregion

        #region Public Properties
        #endregion

        public EntityGrowingManager(ServerCore server)
        {
            _server = server;

            _server.Clock.CreateNewTimer(new Clock.GameClockTimer(UtopiaTimeSpan.FromMinutes(15), server.Clock, GrowingLookup)); 
        }

        #region Public Methods
        #endregion

        #region Private Methods
        private void GrowingLookup(UtopiaTime gametime)
        {
            var random = new Random();

            foreach (var chunk in _server.LandscapeManager.GetBufferedChunks())
            {
                var growingEntities = chunk.Entities.OfType<GrowingEntity>().ToList();

                foreach (var entity in growingEntities)
                {
                    var now = _server.Clock.Now;
                    var totalPassed = now - entity.LastGrowUpdate;

                    bool updated;
                    bool rotten;

                    var tillTheEndOfSeason = UtopiaTimeSpan.FromSeasons(1d - entity.LastGrowUpdate.TotalSeasons % 1d);

                    if (totalPassed <= tillTheEndOfSeason)
                    {
                        updated = GrowSeasonLogic(entity, now.Season, totalPassed, random, chunk, out rotten);
                    }
                    else
                    {
                        // grow at the end of the first season
                        updated = GrowSeasonLogic(entity, now.Season, tillTheEndOfSeason, random, chunk, out rotten);
                        
                        // grow at the middle seasons
                        for (int i = 0; i <= totalPassed.Seasons; i++)
                        {
                            now = entity.LastGrowUpdate + tillTheEndOfSeason + UtopiaTimeSpan.FromSeasons(i);
                            updated = GrowSeasonLogic(entity, now.Season, UtopiaTimeSpan.FromSeasons(1) , random, chunk, out rotten) || updated;
                            if (rotten)
                                break;
                        }
                        
                        if (rotten)
                            continue;

                        // grow at the beginning of the last season
                        var lastSeason = UtopiaTimeSpan.FromSeasons(_server.Clock.Now.TotalSeasons % 1d);
                        updated = GrowSeasonLogic(entity, now.Season, lastSeason, random, chunk, out rotten) || updated;

                        if (rotten)
                            continue;
                    }
                    
                    if (updated)
                    {
                        if (entity is PlantGrowingEntity)
                        {
                            chunk.Entities.RemoveById(entity.StaticId);
                            chunk.Entities.Add(entity);
                        }
                        if (entity is TreeGrowingEntity)
                        {
                            // TODO: tree grow logic
                        }
                    }

                    entity.LastGrowUpdate = now;
                }
            }
        }

        private bool GrowSeasonLogic(GrowingEntity entity, Season season, UtopiaTimeSpan passedTime, Random random, ServerChunk chunk, out bool rotten)
        {
            bool updated = false;
            rotten = false;

            // constranits check
            if (entity.GrowingSeasons.Count > 0 && !entity.GrowingSeasons.Contains(season.Name))
                return false;

            if (entity.GrowingBlocks.Count > 0)
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
            while (entity.CurrentGrowLevel < entity.GrowLevels.Count - 1)
            {
                var currentLevel = entity.GrowLevels[entity.CurrentGrowLevel];

                if (entity.CurrentGrowTime < currentLevel.GrowTime)
                    break;

                if (entity.CurrentGrowLevel == 0 && entity.RottenChance != 0f)
                {
                    if (random.NextDouble() < entity.RottenChance)
                    {
                        chunk.Entities.RemoveById(entity.StaticId);
                        rotten = true;
                        return true;
                    }
                }

                entity.CurrentGrowTime -= currentLevel.GrowTime;
                entity.CurrentGrowLevel++;
                updated = true;
            }


            return updated;
        }

        #endregion

    }
}
