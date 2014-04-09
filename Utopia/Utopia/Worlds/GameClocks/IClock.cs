using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main.Interfaces;
using S33M3DXEngine.Debug.Interfaces;

namespace Utopia.Worlds.GameClocks
{
    public interface IClock : IUpdatableComponent, IGameComponent, IDebugInfo
    {
        VisualClockTime ClockTime { get; set; }        
    }
}
