using System;

namespace S33M3Resources.Structs
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct Vector4B
    {
        public byte X;
        public byte Y;
        public byte Z;
        public byte W;

        public Vector4B(int x, int y, int z, int w)
        {
            this.X = (byte)x;
            this.Y = (byte)y;
            this.Z = (byte)z;
            this.W = (byte)w;
        }

        public Vector4B(byte x, byte y, byte z, byte w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public Vector4B(byte x, byte y, byte z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = 1;
        }

        public Vector4B(uint x, uint y, uint z)
        {
            this.X = (byte)x;
            this.Y = (byte)y;
            this.Z = (byte)z;
            this.W = 1;
        }

        public Vector4B(int x, int y, int z)
        {
            this.X = (byte)x;
            this.Y = (byte)y;
            this.Z = (byte)z;
            this.W = 1;
        }

        public Vector4B(byte value)
        {
            this.X = this.Y = this.Z = value;
            this.W = 1;
        }

        #region Operators
        public static bool operator ==(Vector4B left, Vector4B right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4B left, Vector4B right)
        {
            return !left.Equals(right);
        }

        public static Vector4B operator +(Vector4B a, Vector4B b)
        {
            Vector4B result;

            result.X = (byte)(a.X + b.X);
            result.Y = (byte)(a.Y + b.Y);
            result.Z = (byte)(a.Z + b.Z);
            result.W = (byte)(a.W + b.W);

            return result;
        }

        public static Vector4B operator -(Vector4B a, Vector4B b)
        {
            Vector4B result;

            result.X = (byte)(a.X - b.X);
            result.Y = (byte)(a.Y - b.Y);
            result.Z = (byte)(a.Z - b.Z);
            result.W = (byte)(a.W - b.W);

            return result;
        }

        public static Vector4B operator *(Vector4B a, Vector4B b)
        {
            Vector4B result;

            result.X = (byte)(a.X * b.X);
            result.Y = (byte)(a.Y * b.Y);
            result.Z = (byte)(a.Z * b.Z);
            result.W = (byte)(a.W * b.W);

            return result;
        }

        public static Vector4B operator /(Vector4B a, Vector4B b)
        {
            Vector4B result;

            result.X = (byte)(a.X / b.X);
            result.Y = (byte)(a.Y / b.Y);
            result.Z = (byte)(a.Z / b.Z);
            result.W = (byte)(a.W / b.W);

            return result;
        }
        #endregion

        public bool Equals(Vector4B other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)) &&
                    (this.Z == other.Z) && (this.W == other.W));
        }

        public override bool Equals(object obj)
        {
            bool flag = false;

            if (obj is Vector4B)
            {
                flag = this.Equals((Vector4B)obj);
            }

            return flag;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int value = X + (Y << 8) + (Z << 16) + (W << 24);
                return value;
            }
        }

        public override string ToString()
        {
            return String.Format("{{X:{0} Y:{1} Z:{2} W:{3}}}", X, Y, Z, W);
        }
    }
}
