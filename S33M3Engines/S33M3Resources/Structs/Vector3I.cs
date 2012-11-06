using System;
using SharpDX;

namespace S33M3Resources.Structs
{
    /// <summary>
    /// Defines a three component structure of System.Int32 type
    /// </summary>
    public struct Vector3I : IComparable<Vector3I>
    {
        public int X;
        public int Y;
        public int Z;

        public Vector3I(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3I(double x, double y, double z)
        {
            X = (int)x;
            Y = (int)y;
            Z = (int)z;
        }

        /// <summary>
        /// Returns length between vectors using sqrt
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static double Distance(Vector3I first, Vector3I second)
        {
            var dx = first.X - second.X;
            var dy = first.Y - second.Y;
            var dz = first.Z - second.Z;

            dx = dx * dx;
            dy = dy * dy;
            dz = dz * dz;

            return Math.Sqrt(dx + dy + dz);
        }

        public static double DistanceSquared(Vector3I first, Vector3I second)
        {
            var dx = first.X - second.X;
            var dy = first.Y - second.Y;
            var dz = first.Z - second.Z;

            return dx * dx + dy * dy + dz * dz;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj != null && obj.GetType() == GetType())
            {
                return CompareTo((Vector3I)obj);
            }
            return -1;
        }

        #endregion

        #region IComparable<ChunkPosition> Members

        public int CompareTo(Vector3I other)
        {
            if (X == other.X)
            {
                if (Y == other.Y)
                {
                    if (Z == other.Z)
                    {
                        return 0;
                    }
                    return Z > other.Z ? 1 : -1;
                }
                return Y > other.Y ? 1 : -1;
            }
            return X > other.X ? 1 : -1;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;
            var other = (Vector3I)obj;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return X + (Y << 10) + (Z << 20);
        }

        public override string ToString()
        {
            return string.Format("[{0:000};{1:000};{2:000}]", X, Y, Z);
        }

        public static implicit operator Vector3(Vector3I pos)
        {
            Vector3 vec;

            vec.X = pos.X;
            vec.Y = pos.Y;
            vec.Z = pos.Z;

            return vec;
        }

        public static explicit operator Vector3I(Vector3 vec)
        {
            return new Vector3I(Fastfloor(vec.X), Fastfloor(vec.Y), Fastfloor(vec.Z));
        }

        public static explicit operator Vector3I(Vector3D vec)
        {
            return new Vector3I(Fastfloor(vec.X), Fastfloor(vec.Y), Fastfloor(vec.Z));
        }

        private static int Fastfloor(double x)
        {
            return x > 0 ? (int)x : (int)x - 1;
        }

        private static int Fastfloor(float x)
        {
            return x > 0 ? (int)x : (int)x - 1;
        }

        public static Vector3I operator *(Vector3I pos, int value)
        {
            Vector3I res;

            res.X = pos.X * value;
            res.Y = pos.Y * value;
            res.Z = pos.Z * value;

            return res;
        }

        public static Vector3I operator /(Vector3I pos, int value)
        {
            Vector3I res;

            res.X = pos.X / value;
            res.Y = pos.Y / value;
            res.Z = pos.Z / value;

            return res;
        }

        public static Vector3I operator +(Vector3I pos, Vector3I value)
        {
            Vector3I res;

            res.X = pos.X + value.X;
            res.Y = pos.Y + value.Y;
            res.Z = pos.Z + value.Z;

            return res;
        }

        public static Vector3I operator +(Vector3I pos, int value)
        {
            Vector3I res;

            res.X = pos.X + value;
            res.Y = pos.Y + value;
            res.Z = pos.Z + value;

            return res;
        }

        public static Vector3I operator -(Vector3I pos, int value)
        {
            Vector3I res;

            res.X = pos.X - value;
            res.Y = pos.Y - value;
            res.Z = pos.Z - value;

            return res;
        }

        public static Vector3I operator -(Vector3I pos, Vector3I other)
        {
            Vector3I res;

            res.X = pos.X - other.X;
            res.Y = pos.Y - other.Y;
            res.Z = pos.Z - other.Z;

            return res;
        }

        public static bool operator ==(Vector3I first, Vector3I second)
        {
            return first.X == second.X && first.Y == second.Y && first.Z == second.Z;
        }

        public static bool operator !=(Vector3I first, Vector3I second)
        {
            return !(first == second);
        }

        #endregion

        /// <summary>
        /// Gets Vector3I with values x = 1, y = 1, z = 1
        /// </summary>
        public static Vector3I One
        {
            get { return new Vector3I(1, 1, 1); }
        }

        /// <summary>
        /// Gets Vector3I with values x = 0, y = 0, z = 0
        /// </summary>
        public static Vector3I Zero
        {
            get { return new Vector3I(0, 0, 0); }
        }

        public static Vector3I Up
        {
            get { return new Vector3I(0, 1, 0); }
        }

        public static Vector3I Down
        {
            get { return new Vector3I(0, -1, 0); }
        }

        public static Vector3I Left
        {
            get { return new Vector3I(-1, 0, 0); }
        }

        public static Vector3I Right
        {
            get { return new Vector3I(1, 0, 0); }
        }

        public static Vector3I Front
        {
            get { return new Vector3I(0, 0, -1); }
        }

        public static Vector3I Back
        {
            get { return new Vector3I(0, 0, 1); }
        }

        public bool IsZero()
        {
            return X == 0 && Y == 0 && Z == 0;
        }

        /// <summary>
        /// Returns a vector containing a smallest components of vectors provided
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static Vector3I Min(Vector3I vec1, Vector3I vec2)
        {
            Vector3I vec;
            
            vec.X = Math.Min(vec1.X, vec2.X);
            vec.Y = Math.Min(vec1.Y, vec2.Y);
            vec.Z = Math.Min(vec1.Z, vec2.Z);

            return vec;
        }

        /// <summary>
        /// Returns a vector containing a largest components of vectors provided
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static Vector3I Max(Vector3I vec1, Vector3I vec2)
        {
            Vector3I vec;

            vec.X = Math.Max(vec1.X, vec2.X);
            vec.Y = Math.Max(vec1.Y, vec2.Y);
            vec.Z = Math.Max(vec1.Z, vec2.Z);

            return vec;
        }
    }
}
