using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3DXEngine.Main.Interfaces;

namespace Utopia.Worlds.Weathers.SharedComp
{
    public interface IWind : IGameComponent, IUpdatableComponent
    {
        Vector3 WindFlow { get; set; }
        Vector3 FlatWindFlowNormalizedWithNoise { get; set; }
        double KeyFrameAnimation { get; }
    }
}
