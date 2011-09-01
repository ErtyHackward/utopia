using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct;
using S33M3Engines.Maths;
using SharpDX;

namespace S33M3Engines.WorldFocus
{
    public interface IWorldFocus
    {
        FTSValue<DVector3> FocusPoint { get; }
        FTSValue<Matrix> FocusPointMatrix { get; }
    }
}
