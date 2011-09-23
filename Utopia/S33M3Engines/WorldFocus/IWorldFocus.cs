using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct;
using S33M3Engines.Maths;
using SharpDX;
using S33M3Engines.Shared.Math;

namespace S33M3Engines.WorldFocus
{
    public interface IWorldFocus
    {
        FTSValue<Vector3D> FocusPoint { get; }
        FTSValue<Matrix> FocusPointMatrix { get; }
    }
}
