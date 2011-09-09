using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace S33M3Engines.Maths
{
    public static class MQuaternion
    {
        public static double getPitch(ref Quaternion quaternion)
        {
            return Math.Atan2(2 * (quaternion.Y * quaternion.Z + quaternion.W * quaternion.X), quaternion.W * quaternion.W - quaternion.X * quaternion.X - quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
        }

        public static double getYaw(ref Quaternion quaternion)
        {
            return Math.Asin(-2 * (quaternion.X * quaternion.Z - quaternion.W * quaternion.Y));
        }

        public static double getRoll(ref Quaternion quaternion)
        {
            return Math.Atan2(2 * (quaternion.X * quaternion.Y + quaternion.W * quaternion.Z), quaternion.W * quaternion.W + quaternion.X * quaternion.X - quaternion.Y * quaternion.Y - quaternion.Z * quaternion.Z);
        }
    }
}
