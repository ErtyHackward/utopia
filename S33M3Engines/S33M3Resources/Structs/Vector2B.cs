using System;

namespace S33M3Resources.Structs
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct Vector2B
    {
        public byte X;
        public byte Y;

        public Vector2B(byte x, byte y)
        {
            this.X = x;
            this.Y = y;
        }

        public Vector2B(uint x, uint y)
        {
            this.X = (byte)x;
            this.Y = (byte)y;
        }

        public Vector2B(int x, int y)
        {
            this.X = (byte)x;
            this.Y = (byte)y;
        }

        public Vector2B(byte value)
        {
            this.X = this.Y = value;
        }

        #region Operators
        public static bool operator ==(Vector2B left, Vector2B right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2B left, Vector2B right)
        {
            return !left.Equals(right);
        }

        public static Vector2B operator +(Vector2B a, Vector2B b)
        {
            Vector2B result;

            result.X = (byte)(a.X + b.X);
            result.Y = (byte)(a.Y + b.Y);

            return result;
        }

        public static Vector2B operator -(Vector2B a, Vector2B b)
        {
            Vector2B result;

            result.X = (byte)(a.X - b.X);
            result.Y = (byte)(a.Y - b.Y);

            return result;
        }

        public static Vector2B operator *(Vector2B a, Vector2B b)
        {
            Vector2B result;

            result.X = (byte)(a.X * b.X);
            result.Y = (byte)(a.Y * b.Y);

            return result;
        }

        public static Vector2B operator /(Vector2B a, Vector2B b)
        {
            Vector2B result;

            result.X = (byte)(a.X / b.X);
            result.Y = (byte)(a.Y / b.Y);

            return result;
        }
        #endregion

        public bool Equals(Vector2B other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)));
        }

        public override bool Equals(object obj)
        {
            bool flag = false;

            if (obj is Vector2B)
            {
                flag = this.Equals((Vector2B)obj);
            }

            return flag;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int value = X + (Y << 8);
                return value;
            }
        }

        public override string ToString()
        {
            return String.Format("{{X:{0} Y:{1}}", X, Y);
        }
    }
}
