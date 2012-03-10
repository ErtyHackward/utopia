using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main.Interfaces;
using S33M3_DXEngine.Debug.Interfaces;

namespace Utopia.Worlds.GameClocks
{
    public interface IClock : IUpdatableComponent, IGameComponent, IDebugInfo
    {
        Clock.VisualClockTime ClockTime { get; set; }
    }
}
