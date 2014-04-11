using System;
using System.Linq;
using S33M3Resources.Structs;
using Utopia.Shared.Entities;
using Utopia.Shared.Structs;
using System.Collections.Generic;

namespace Utopia.Shared.Server.Managers
{
    public class EntityGrowingManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variable
        private ServerCore _server;
        private UtopiaTimeSpan _growingLookupSpan = UtopiaTimeSpan.FromMinutes(15);
        #endregion

        #region Public Properties
        #endregion

        public EntityGrowingManager(ServerCore server)
        {
            _server = server;

            _server.Clock.CreateNewTimer(new Clock.GameClockTimer(_growingLookupSpan, server.Clock, GrowingLookup)); 
        }

        #region Public Methods
        #endregion

        #region Private Methods
        private void GrowingLookup(UtopiaTime LookupTime)
        {
            var random = new Random();

            foreach (var chunk in _server.LandscapeManager.GetBufferedChunks())
            {
                var growingEntities = chunk.Entities.OfType<GrowingEntity>().ToList();

                //Check only for entities that can still grow
                foreach (var entity in growingEntities.Where(x => !x.isLastGrowLevel))
                {
                    bool entityUpdated = false;
                    //Init LastGrowUpdate
                    if (entity.LastGrowUpdate.TotalSeconds == 0 && entity.CurrentGrowLevel == 0) entity.LastGrowUpdate = LookupTime;

                    while (entity.LastGrowRefresh < LookupTime && !entity.isLastGrowLevel)
                    {
                        entity.LastGrowRefresh += _growingLookupSpan;
                        if (CheckEntityGrowConstraints(entity, entity.LastGrowRefresh))
                        {
                            //Check for rotting entity =====================================
                            if (entity.CurrentGrowLevel == 0 && entity.RottenChance != 0f)
                            {
                                if (random.NextDouble() < entity.RottenChance)
                                {
                                    chunk.Entities.RemoveById(entity.StaticId);
                                    continue;
                                }
                            }

                            entity.CurrentGrowLevel++;
                            //Make the entity grow to the next level !

                            entityUpdated = true; 
                        }
                    }

                    if (entityUpdated)
                    {
                        chunk.Entities.RemoveById(entity.StaticId);
                        chunk.Entities.Add(entity);
                        entity.LastGrowUpdate = LookupTime;
                    }
                }
            }
        }

        /// <summary>
        /// Fct responsible to check if the entity can grow to the next level, or not.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private bool CheckEntityGrowConstraints(GrowingEntity entity, UtopiaTime time)
        {
            try
            {


                //Check minimum time needed
                if (entity.GrowLevels[entity.CurrentGrowLevel].GrowTime > time - entity.LastGrowUpdate) return false;

                // Check Season constraint
                if (entity.GrowingSeasons.Count > 0 && !entity.GrowingSeasons.Contains(time.Season.Name)) return false;

                //Check Block linked constraint
                if (entity.GrowingBlocks.Count > 0)
                {
                    if (entity.Linked)
                    {
                        var cursor = _server.LandscapeManager.GetCursor(entity.LinkedCube);
                        if (!entity.GrowingBlocks.Contains(cursor.Read())) return false;
                    }
                    else
                    {
                        var cursor = _server.LandscapeManager.GetCursor(entity.Position);
                        if (!entity.GrowingBlocks.Contains(cursor.PeekValue(Vector3I.Down))) return false;
                    }
                }

                // TODO: check light constraint when implemented

                return true;

            }
            catch (Exception e)
            {
                logger.Error("Error while applying Growing logic : {0}", e.Message);
                return false;
            }
        }
        #endregion

    }
}
