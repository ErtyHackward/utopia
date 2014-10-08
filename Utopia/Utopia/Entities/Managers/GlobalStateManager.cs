using System;
using Utopia.Network;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;

namespace Utopia.Entities.Managers
{
    public class GlobalStateManager : IGlobalStateManager
    {
        public GlobalStateManager(ServerComponent serverComponent)
        {
            if (serverComponent == null) throw new ArgumentNullException("serverComponent");

            GlobalState = serverComponent.GameInformations.GlobalState;
        }

        public GlobalState GlobalState { get; private set; }
    }
}
