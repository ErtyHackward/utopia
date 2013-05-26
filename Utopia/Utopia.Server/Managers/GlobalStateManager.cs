using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Server.Managers
{
    public class GlobalStateManager : IGlobalStateManager, IDisposable
    {
        private readonly Server _server;

        private readonly Timer _saveTimer;
        
        public GlobalState GlobalState { get; set; }

        public GlobalStateManager(Server server)
        {
            _server = server;

            GlobalState = _server.EntityStorage.LoadState();

            if (GlobalState == null)
            {
                GlobalState = new GlobalState();
            }

            _saveTimer = new Timer(o => Save(), null, 60 * 1000, 60 * 1000);
        }

        private void Save()
        {
            _server.EntityStorage.SaveState(GlobalState);
        }
        
        public Faction GetFaction(uint factionId)
        {
            return GlobalState.Factions.FirstOrDefault(f => f.FactionId == factionId);
        }

        public void Dispose()
        {
            _saveTimer.Dispose();
            Save();
        }
    }
}
