using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Utopia.Shared.Structs
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct ByteColor
    {
        public byte R;
        public byte G;
        public byte B;
        public byte SunLight;

        public ByteColor(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.SunLight = a;
        }

        public ByteColor(Color c)
        {
            this.R = c.R;
            this.G = c.G;
            this.B = c.B;
            this.SunLight = c.A;
        }

        public ByteColor(SharpDX.Color4 color)
        {
            R = (byte)(color.Red * 255);
            G = (byte)(color.Green * 255);
            B = (byte)(color.Blue * 255);
            SunLight = (byte)(color.Alpha * 255);
        }

        public static ByteColor operator +(ByteColor a, ByteColor b)
        {
            ByteColor result;

            result.R = (byte) (a.R + b.R);
            result.G = (byte) (a.G + b.G);
            result.B = (byte) (a.B + b.B);
            result.SunLight = (byte) (a.SunLight + b.SunLight);

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
                a += colors[i].SunLight;
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
                int value = SunLight + (R << 8) + (G << 16) + (B << 24);
                return value;
            }
        }

        public override String ToString()
        {
            return R.ToString() + ',' + G + ',' + B + ',' + SunLight;
        }
    }
}