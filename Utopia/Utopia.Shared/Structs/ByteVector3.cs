using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Utopia.Shared.Structs
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct ByteVector3
    {
        public byte X;
        public byte Y;
        public byte Z;

        public ByteVector3(int x, int y, int z)
        {
            this.X = (byte)x;
            this.Y = (byte)y;
            this.Z = (byte)z;
        }

        public ByteVector3(byte x, byte y, byte z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public ByteVector3(uint x, uint y, uint z)
        {
            this.X = (byte)x;
            this.Y = (byte)y;
            this.Z = (byte)z;
        }

        public ByteVector3(byte value)
        {
            this.X = this.Y = this.Z = value;
        }

        #region Operators
        public static bool operator ==(ByteVector3 left, ByteVector3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ByteVector3 left, ByteVector3 right)
        {
            return !left.Equals(right);
        }

        public static ByteVector3 operator +(ByteVector3 a, ByteVector3 b)
        {
            ByteVector3 result;

            result.X = (byte)(a.X + b.X);
            result.Y = (byte)(a.Y + b.Y);
            result.Z = (byte)(a.Z + b.Z);

            return result;
        }

        public static ByteVector3 operator -(ByteVector3 a, ByteVector3 b)
        {
            ByteVector3 result;

            result.X = (byte)(a.X - b.X);
            result.Y = (byte)(a.Y - b.Y);
            result.Z = (byte)(a.Z - b.Z);

            return result;
        }

        public static ByteVector3 operator *(ByteVector3 a, ByteVector3 b)
        {
            ByteVector3 result;

            result.X = (byte)(a.X * b.X);
            result.Y = (byte)(a.Y * b.Y);
            result.Z = (byte)(a.Z * b.Z);

            return result;
        }

        public static ByteVector3 operator /(ByteVector3 a, ByteVector3 b)
        {
            ByteVector3 result;

            result.X = (byte)(a.X / b.X);
            result.Y = (byte)(a.Y / b.Y);
            result.Z = (byte)(a.Z / b.Z);

            return result;
        }
        #endregion

        public bool Equals(ByteVector3 other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)) &&
                    (this.Z == other.Z));
        }

        public override bool Equals(object obj)
        {
            bool flag = false;

            if (obj is ByteVector3)
            {
                flag = this.Equals((ByteVector3)obj);
            }

            return flag;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return this.X.GetHashCode() + this.Y.GetHashCode() +
                        this.Z.GetHashCode();
            }
        }

        public override string ToString()
        {
            return String.Format("{{X:{0} Y:{1} Z:{2}}}", X, Y, Z);
        }
    }
}
