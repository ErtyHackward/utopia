﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main.Interfaces;
using S33M3DXEngine.Debug.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Worlds.GameClocks
{
    public interface IClock : IUpdatableComponent, IGameComponent, IDebugInfo
    {
        VisualClockTime ClockTime { get; set; }
        UtopiaTime Now { get; }
        bool FrozenTime { get; set; }
    }
}
