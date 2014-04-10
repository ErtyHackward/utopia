using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            _server.Clock.CreateNewTimer(new Clock.GameClockTimer(UtopiaTimeSpan.FromMinutes(5), server.Clock, GrowingLookup)); 
        }

        #region Public Methods
        #endregion

        #region Private Methods
        private void GrowingLookup(UtopiaTime gametime)
        {
            //When a chunk, is loaded back from Database (Was generated and modified, then goes into a "Sleep" mode) this method should be call on all
            //growing entities of this chunk multiple times to "simulate" growing that should have been applied on the entities of this chunk while it was sleeping.


            //Get all entities that are growing entities that are not "matured" = can still grow

            //Filters out entities where
            // - the minimum time for the next growing state is not reached
            // - the entity growing is limited by seasons
            // - the entity growing is limited by "light" presence for the entity
            // - the entity growing is limited by a link to specific blocks
            
            // If passed all those tests, the entity can "grow" to the next level
            // - If entity grow level = 0, then test its rotten chances => If rotten, then the entity will be removed
            
            // Update the entity to make it goes to the next growing state, broadcast a client message to signal the change of growing level for the entity.
        }
        #endregion

    }
}
