using System;
using SharpDX;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Defines a two component structure of System.Int32 type
    /// </summary>
    public struct Vector2I : IComparable<Vector2I>
    {
        public int X;
        public int Y;

        public Vector2I(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Returns length between vectors using sqrt
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static double Distance(Vector2I first, Vector2I second)
        {
            var dx = first.X - second.X;
            var dy = first.Y - second.Y;

            dx = dx * dx;
            dy = dy * dy;

            return Math.Sqrt(dx + dy);
        }
        
        public static double DistanceSquared(Vector2I first, Vector2I second)
        {
            var dx = first.X - second.X;
            var dy = first.Y - second.Y;

            return dx * dx + dy * dy;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj != null && obj.GetType() == GetType())
            {
                return CompareTo((Vector2I)obj);
            }
            return -1;
        }

        #endregion

        #region IComparable<ChunkPosition> Members

        public int CompareTo(Vector2I other)
        {
            if (X == other.X)
            {
                if (Y == other.Y)
                {
                    return 0;
                }
                return Y > other.Y ? 1 : -1;
            }
            return X > other.X ? 1 : -1;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;
            var other = (Vector2I)obj;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return X + Y * 65536;
        }

        public override string ToString()
        {
            return string.Format("X = {0},Y = {1}", X, Y);
        }

        /// <summary>
        /// Compute a the ID with Block Unit
        /// </summary>
        /// <returns></returns>
        public long GetID()
        {
            return (((Int64)(X * Utopia.Shared.Chunks.AbstractChunk.ChunkSize.X) << 32) + (Y * Utopia.Shared.Chunks.AbstractChunk.ChunkSize.Z));
        }

        public static implicit operator Location2<int>(Vector2I pos)
        {
            Location2<int> vec;

            vec.X = pos.X;
            vec.Z = pos.Y;

            return vec;
        }

        public static implicit operator Vector2I(Location2<int> pos)
        {
            Vector2I vec;

            vec.X = pos.X;
            vec.Y = pos.Z;

            return vec;
        }

        public static implicit operator Vector2(Vector2I pos)
        {
            Vector2 vec;

            vec.X = pos.X;
            vec.Y = pos.Y;
            
            return vec;
        }

        public static explicit operator Vector2I(Vector2 vec)
        {
            Vector2I pos;
            pos.X = (int)vec.X;
            pos.Y = (int)vec.Y;

            return pos;
        }

        public static Vector2I operator *(Vector2I pos, int value)
        {
            Vector2I res;

            res.X = pos.X * value;
            res.Y = pos.Y * value;

            return res;
        }

        public static Vector2I operator +(Vector2I pos, Vector2I value)
        {
            Vector2I res;

            res.X = pos.X + value.X;
            res.Y = pos.Y + value.Y;

            return res;
        }

        public static Vector2I operator +(Vector2I pos, int value)
        {
            Vector2I res;

            res.X = pos.X + value;
            res.Y = pos.Y + value;

            return res;
        }

        public static Vector2I operator -(Vector2I pos, int value)
        {
            Vector2I res;

            res.X = pos.X - value;
            res.Y = pos.Y - value;

            return res;
        }


        public static bool operator ==(Vector2I first, Vector2I second)
        {
            return first.X == second.X && first.Y == second.Y;
        }

        public static bool operator !=(Vector2I first, Vector2I second)
        {
            return !(first == second);
        }

        #endregion

        /// <summary>
        /// Gets IntVector2 with values x = 1, y = 1
        /// </summary>
        public static Vector2I One
        {
            get { return new Vector2I(1, 1); }
        }

        public bool IsZero()
        {
            return X == 0 && Y == 0;
        }
    }
}
