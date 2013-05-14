using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Network;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;

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
