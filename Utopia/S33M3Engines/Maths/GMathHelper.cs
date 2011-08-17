using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.Struct;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;

namespace S33M3Engines.Maths
{
    public static class GMathHelper
    {
        public static void CenterOnFocus(ref Matrix WorldMatrix, ref Matrix WorldFocusedMatrix, ref IWorldFocus WorldFocus)
        {
            WorldFocusedMatrix.M41 = WorldMatrix.M41 - (float)WorldFocus.FocusPoint.ActualValue.X;
            WorldFocusedMatrix.M42 = WorldMatrix.M42 - (float)WorldFocus.FocusPoint.ActualValue.Y;
            WorldFocusedMatrix.M43 = WorldMatrix.M43 - (float)WorldFocus.FocusPoint.ActualValue.Z;
        }
    }
}
