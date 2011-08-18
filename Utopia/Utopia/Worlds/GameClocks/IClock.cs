using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;

namespace Utopia.Worlds.GameClocks
{
    public interface IClock : IUpdatableComponent
    {
        Clock.VisualClockTime ClockTime { get; set; }
    }
}
