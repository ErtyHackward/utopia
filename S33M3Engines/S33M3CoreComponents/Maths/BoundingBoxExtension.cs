using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace S33M3CoreComponents.Maths
{
    public static class BoundingBoxExtension
    {
        /// <summary>
        /// Provide an intersect test with tolerance accounting
        /// </summary>
        /// <param name="bbox"></param>
        /// <param name="box1"></param>
        /// <param name="box2"></param>
        /// <returns></returns>
        public static bool Intersects(this BoundingBox box1, ref BoundingBox box2, float tolerance)
        {
            if ((box1.Minimum.X + tolerance) > box2.Maximum.X || (box2.Minimum.X + tolerance) > box1.Maximum.X)
                return false;

            if ((box1.Minimum.Y + tolerance) > box2.Maximum.Y || (box2.Minimum.Y + tolerance) > box1.Maximum.Y)
                return false;

            if ((box1.Minimum.Z + tolerance) > box2.Maximum.Z || (box2.Minimum.Z + tolerance) > box1.Maximum.Z)
                return false;

            return true;
        }

        public static bool Contains(this BoundingBox box, ref Vector3 point, float tolerance)
        {
            if (box.Minimum.X - tolerance <= point.X && box.Maximum.X + tolerance >= point.X &&
                box.Minimum.Y - tolerance <= point.Y && box.Maximum.Y + tolerance >= point.Y &&
                box.Minimum.Z - tolerance <= point.Z && box.Maximum.Z + tolerance >= point.Z)
            {
                return true;
            }

            return false;
        }
    }
}
