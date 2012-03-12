using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;
using SharpDX;

namespace S33M3_CoreComponents.WorldFocus.Interfaces
{
    public interface IWorldFocus
    {
        FTSValue<Vector3D> FocusPoint { get; }
        FTSValue<Matrix> FocusPointMatrix { get; }
    }
}
