using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace S33M3_CoreComponents.Maths
{
    public static class MBoundingSphere
    {
        public static void SupportMapping(ref Vector3 v, ref float radius, ref Vector3 center, out Vector3 result)
        {
            float num2 = v.Length();
            float num = radius / num2;
            result.X = center.X + (v.X * num);
            result.Y = center.Y + (v.Y * num);
            result.Z = center.Z + (v.Z * num);
        }
    }
}
