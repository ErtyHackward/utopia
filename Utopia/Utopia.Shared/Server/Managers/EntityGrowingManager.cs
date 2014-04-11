using System;
using System.Linq;
using S33M3Resources.Structs;
using Utopia.Shared.Entities;
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
            var now = _server.Clock.Now;

            var random = new Random();

            foreach (var chunk in _server.LandscapeManager.GetBufferedChunks())
            {
                var growingEntities = chunk.Entities.OfType<GrowingEntity>().ToList();

                foreach (var entity in growingEntities)
                {
                    var passedTime = now - entity.LastLevelUpdate;
                    var updated = false;

                    // constranits check
                    if (entity.GrowingSeasons.Count > 0 && !entity.GrowingSeasons.Contains(now.Season.Name))
                        continue;

                    if (entity.GrowingBlocks.Count > 0)
                    {
                        var cursor = _server.LandscapeManager.GetCursor(entity.Position);
                        if (!entity.GrowingBlocks.Contains(cursor.PeekValue(Vector3I.Down)))
                            continue;
                    }

                    // TODO: check light constraint when implemented


                    // update entity to the actual state 
                    while (entity.CurrentGrowLevel < entity.GrowLevels.Count - 1)
                    {
                        var currentLevel = entity.GrowLevels[entity.CurrentGrowLevel];
                        passedTime -= currentLevel.GrowTime;

                        if (passedTime.TotalSeconds < 0)
                            break;

                        if (entity.CurrentGrowLevel == 0 && entity.RottenChance != 0f)
                        {
                            if (random.NextDouble() < entity.RottenChance)
                            {
                                chunk.Entities.RemoveById(entity.StaticId);
                                continue;
                            }
                        }
                        
                        entity.CurrentGrowLevel++;
                        updated = true;
                    }

                    if (updated)
                    {
                        entity.LastLevelUpdate = now;
                        chunk.Entities.RemoveById(entity.StaticId);
                        chunk.Entities.Add(entity);
                    }

                }
            }
        }
        #endregion

    }
}
