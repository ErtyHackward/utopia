using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Shared.Structs;
using S33M3Engines.Shared.Math;

namespace S33M3Engines.Maths
{
    public static class MVector2
    {
        private static readonly Vector2 _one = new Vector2(1, 1);

        public static void Transform(ref Vector2 position, ref Matrix matrix, out Vector2 result)
        {
            float num2 = ((position.X * matrix.M11) + (position.Y * matrix.M21)) + matrix.M41;
            float num = ((position.X * matrix.M12) + (position.Y * matrix.M22)) + matrix.M42;
            result.X = num2;
            result.Y = num;
        }

        /// <summary>
        /// Special Distance comparison removing the Y testing
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static float Distance(Vector2I value1, Vector3D value2)
        {
            float num3 = value1.X - (float)value2.X;
            float num = value1.Y - (float)value2.Z;
            float num4 = ((num3 * num3) + (num * num));
            return (float)Math.Sqrt((double)num4);
        }

        public static Vector2 One
        {
            get
            {
                return _one;
            }
        }
    }
}
