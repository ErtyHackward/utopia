using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks;

namespace Utopia.Shared.Server.Managers
{
    public class EntitySpawningManager
    {
        private readonly ServerCore _server;
        private readonly IEntitySpawningControler _entitySpawningControler;

        public EntitySpawningManager(ServerCore server, IEntitySpawningControler entitySpawningControler)
        {
            _server = server;
            _entitySpawningControler = entitySpawningControler;
            
            //Register a thread timer based on "In game" time value (Like 1 hours ingame passed), this will start a "look up" through the various
            //Server chunk currently handled, and will do an entity spawning refresh on each chunk.
        }

        /// <summary>
        /// This method should be called every "in game hour".
        /// </summary>
        private void SpawningLookup()
        {

        }

    }
}
