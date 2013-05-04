using System;
using System.Runtime.InteropServices;
using SharpDX;

namespace S33M3Resources.Structs
{
    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct ByteColor
    {
        public static readonly ByteColor Zero = new ByteColor();

        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public ByteColor(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public ByteColor(byte r, byte g, byte b)
            :this(r, g, b, (byte)255)
        {
        }

        public ByteColor(int r, int g, int b, int a)
        {
            this.R = (byte)r;
            this.G = (byte)g;
            this.B = (byte)b;
            this.A = (byte)a;
        }

        public ByteColor(int r, int g, int b)
            :this(r, g, b, 255)
        {
        }

        public ByteColor(SharpDX.Color4 color)
        {
            R = (byte)(color.Red * 255);
            G = (byte)(color.Green * 255);
            B = (byte)(color.Blue * 255);
            A = (byte)(color.Alpha * 255);
        }

        public ByteColor(SharpDX.Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        public static bool operator ==(ByteColor a, ByteColor b)
        {
            return a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        }

        public static bool operator !=(ByteColor a, ByteColor b)
        {
            return !( a == b );
        }


        public static ByteColor operator +(ByteColor a, ByteColor b)
        {
            ByteColor result;

            result.R = (byte) (a.R + b.R);
            result.G = (byte) (a.G + b.G);
            result.B = (byte) (a.B + b.B);
            result.A = (byte) (a.A + b.A);

            return result;
        }

        public static ByteColor Average(params ByteColor[] colors)
        {
            int a = 0;
            int r = 0;
            int g = 0;
            int b = 0;
            int nbrColors = colors.Length;
            for (int i = 0; i < nbrColors; i++)
            {
                a += colors[i].A;
                r += colors[i].R;
                g += colors[i].G;
                b += colors[i].B;
            }

            return new ByteColor((byte) (r/nbrColors), (byte) (g/nbrColors), (byte) (b/nbrColors), (byte) (a/nbrColors));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int value = A + (R << 8) + (G << 16) + (B << 24);
                return value;
            }
        }

        public override String ToString()
        {
            return R.ToString() + ',' + G + ',' + B + ',' + A;
        }

        public static implicit operator ByteColor(Color4 color)
        {
            return new ByteColor(color);
        }

        public static implicit operator ByteColor(Color color)
        {
            return new ByteColor(color);
        }

        /// <summary>
        /// Returns Color3 structure based on the current color, truncates alpha
        /// </summary>
        /// <returns></returns>
        public Color3 ToColor3()
        {
            return new Color3((float)R / 255, (float)G / 255, (float)B / 255);
        }

        /// <summary>
        /// Returns Color4 structure based on the current color
        /// </summary>
        /// <returns></returns>
        public Color4 ToColor4()
        {
            return new Color4((float)R / 255, (float)G / 255, (float)B / 255, (float)A / 255);
        }
    }
}