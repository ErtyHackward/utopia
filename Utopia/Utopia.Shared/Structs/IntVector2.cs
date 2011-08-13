using System;
using SharpDX;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Defines a two component structure of System.Int32 type
    /// </summary>
    public struct IntVector2 : IComparable<IntVector2>
    {
        public int X;
        public int Y;

        public IntVector2(int x, int y)
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
        public static double Distance(IntVector2 first, IntVector2 second)
        {
            var dx = first.X - second.X;
            var dy = first.Y - second.Y;

            dx = dx * dx;
            dy = dy * dy;

            return System.Math.Sqrt(dx + dy);
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj != null && obj.GetType() == GetType())
            {
                return CompareTo((IntVector2)obj);
            }
            return -1;
        }

        #endregion

        #region IComparable<ChunkPosition> Members

        public int CompareTo(IntVector2 other)
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
            var other = (IntVector2)obj;
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

        public static implicit operator Location2<int>(IntVector2 pos)
        {
            Location2<int> vec;

            vec.X = pos.X;
            vec.Z = pos.Y;

            return vec;
        }

        public static implicit operator IntVector2(Location2<int> pos)
        {
            IntVector2 vec;

            vec.X = pos.X;
            vec.Y = pos.Z;

            return vec;
        }

        public static implicit operator Vector2(IntVector2 pos)
        {
            Vector2 vec;

            vec.X = pos.X;
            vec.Y = pos.Y;
            
            return vec;
        }

        public static explicit operator IntVector2(Vector2 vec)
        {
            IntVector2 pos;
            pos.X = (int)vec.X;
            pos.Y = (int)vec.Y;

            return pos;
        }

        public static IntVector2 operator *(IntVector2 pos, int value)
        {
            IntVector2 res;

            res.X = pos.X * value;
            res.Y = pos.Y * value;

            return res;
        }

        public static IntVector2 operator +(IntVector2 pos, IntVector2 value)
        {
            IntVector2 res;

            res.X = pos.X + value.X;
            res.Y = pos.Y + value.Y;

            return res;
        }

        public static IntVector2 operator +(IntVector2 pos, int value)
        {
            IntVector2 res;

            res.X = pos.X + value;
            res.Y = pos.Y + value;

            return res;
        }

        public static IntVector2 operator -(IntVector2 pos, int value)
        {
            IntVector2 res;

            res.X = pos.X - value;
            res.Y = pos.Y - value;

            return res;
        }


        public static bool operator ==(IntVector2 first, IntVector2 second)
        {
            return first.X == second.X && first.Y == second.Y;
        }

        public static bool operator !=(IntVector2 first, IntVector2 second)
        {
            return !(first == second);
        }

        #endregion
    }
}
