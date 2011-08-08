using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

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

        public static Vector2 One
        {
            get
            {
                return _one;
            }
        }
    }
}
