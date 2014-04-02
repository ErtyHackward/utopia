using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;

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

            _server.Clock.ClockTimers.Add(new Clock.GameClockTimer(0, 0, 1, 0, server.Clock, SpawningLookup));
        }

        /// <summary>
        /// This method should be called every "in game hour".
        /// </summary>
        private void SpawningLookup()
        {

        }

    }
}
