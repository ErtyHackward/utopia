using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct;
using SharpDX;
using S33M3Engines.Maths;

namespace S33M3Engines.D3D
{
    public interface IWorldFocus
    {
        FTSValue<DVector3> FocusPoint { get; }
    }
}
