using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Utopia.Shared.Structs
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct ByteVector4
    {
        public byte X;
        public byte Y;
        public byte Z;
        public byte W;

        public ByteVector4(int x, int y, int z, int w)
        {
            this.X = (byte)x;
            this.Y = (byte)y;
            this.Z = (byte)z;
            this.W = (byte)w;
        }

        public ByteVector4(byte x, byte y, byte z, byte w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public ByteVector4(byte x, byte y, byte z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = 1;
        }

        public ByteVector4(uint x, uint y, uint z)
        {
            this.X = (byte)x;
            this.Y = (byte)y;
            this.Z = (byte)z;
            this.W = 1;
        }

        public ByteVector4(int x, int y, int z)
        {
            this.X = (byte)x;
            this.Y = (byte)y;
            this.Z = (byte)z;
            this.W = 1;
        }

        public ByteVector4(byte value)
        {
            this.X = this.Y = this.Z = value;
            this.W = 1;
        }

        #region Operators
        public static bool operator ==(ByteVector4 left, ByteVector4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ByteVector4 left, ByteVector4 right)
        {
            return !left.Equals(right);
        }

        public static ByteVector4 operator +(ByteVector4 a, ByteVector4 b)
        {
            ByteVector4 result;

            result.X = (byte)(a.X + b.X);
            result.Y = (byte)(a.Y + b.Y);
            result.Z = (byte)(a.Z + b.Z);
            result.W = (byte)(a.W + b.W);

            return result;
        }

        public static ByteVector4 operator -(ByteVector4 a, ByteVector4 b)
        {
            ByteVector4 result;

            result.X = (byte)(a.X - b.X);
            result.Y = (byte)(a.Y - b.Y);
            result.Z = (byte)(a.Z - b.Z);
            result.W = (byte)(a.W - b.W);

            return result;
        }

        public static ByteVector4 operator *(ByteVector4 a, ByteVector4 b)
        {
            ByteVector4 result;

            result.X = (byte)(a.X * b.X);
            result.Y = (byte)(a.Y * b.Y);
            result.Z = (byte)(a.Z * b.Z);
            result.W = (byte)(a.W * b.W);

            return result;
        }

        public static ByteVector4 operator /(ByteVector4 a, ByteVector4 b)
        {
            ByteVector4 result;

            result.X = (byte)(a.X / b.X);
            result.Y = (byte)(a.Y / b.Y);
            result.Z = (byte)(a.Z / b.Z);
            result.W = (byte)(a.W / b.W);

            return result;
        }
        #endregion

        public bool Equals(ByteVector4 other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)) &&
                    (this.Z == other.Z) && (this.W == other.W));
        }

        public override bool Equals(object obj)
        {
            bool flag = false;

            if (obj is ByteVector4)
            {
                flag = this.Equals((ByteVector4)obj);
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
