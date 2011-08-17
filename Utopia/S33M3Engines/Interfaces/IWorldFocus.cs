using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Maths;
using S33M3Engines.Struct;

namespace S33M3Engines.Interfaces
{
    public interface IWorldFocus
    {
        FTSValue<DVector3> FocusPoint { get; }
    }
}
