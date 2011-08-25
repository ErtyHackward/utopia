using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;

namespace Utopia.Worlds.GameClocks
{
    public interface IClock : IUpdatableComponent, IDebugInfo
    {
        Clock.VisualClockTime ClockTime { get; set; }
    }
}
