using System;
using System.Linq;
using System.Threading;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Server.Managers
{
    public class GlobalStateManager : IDisposable, IGlobalStateManager
    {
        private readonly ServerCore _server;

        private readonly Timer _saveTimer;
        
        public GlobalState GlobalState { get; set; }

        public GlobalStateManager(ServerCore server)
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
