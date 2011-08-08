using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace S33M3Engines.Maths
{
    public static class MBoundingBox
    {
        public static bool Intersects(ref BoundingBox box1, ref BoundingBox box2, bool notStrict = false)
        {
            if (notStrict)
            {
                if ((box1.Maximum.X <= box2.Minimum.X) || (box1.Minimum.X >= box2.Maximum.X))
                {
                    return false;
                }
                if ((box1.Maximum.Y <= box2.Minimum.Y) || (box1.Minimum.Y >= box2.Maximum.Y))
                {
                    return false;
                }
                if ((box1.Maximum.Z <= box2.Minimum.Z) || (box1.Minimum.Z >= box2.Maximum.Z))
                {
                    return false;
                }
            }
            else
            {
                if ((box1.Maximum.X < box2.Minimum.X) || (box1.Minimum.X > box2.Maximum.X))
                {
                    return false;
                }
                if ((box1.Maximum.Y < box2.Minimum.Y) || (box1.Minimum.Y > box2.Maximum.Y))
                {
                    return false;
                }
                if ((box1.Maximum.Z < box2.Minimum.Z) || (box1.Minimum.Z > box2.Maximum.Z))
                {
                    return false;
                }
            }
            return true;
        }


        public static void SupportMapping(ref Vector3 v, ref Vector3 minimum,ref Vector3 maximum,  out Vector3 result)
        {
            result.X = (v.X >= 0f) ? maximum.X : minimum.X;
            result.Y = (v.Y >= 0f) ? maximum.Y : minimum.Y;
            result.Z = (v.Z >= 0f) ? maximum.Z : minimum.Z;
        }


    }
}
