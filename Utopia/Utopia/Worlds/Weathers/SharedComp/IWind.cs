using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using SharpDX;

namespace Utopia.Worlds.Weathers.SharedComp
{
    public interface IWind : IGameComponent,IUpdateableComponent
    {
        Vector3 WindFlow { get; set; }
        Vector3 FlatWindFlow { get; set; }
    }
}
