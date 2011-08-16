using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Shared.Math.Noises;

namespace Utopia.Shared.Math
{
    public static class MathHelper
    {
        public static int Mod(int value, int div)
        {
            int result = value % div;
            return result < 0 ? result + div : result;
        }

        public static int Fastfloor(double x)
        {
            return x > 0 ? (int)x : (int)x - 1;
        }

        public static int Fastfloor(float x)
        {
            return x > 0 ? (int)x : (int)x - 1;
        }

        public static byte FastfloorByte(float x)
        {
            return x > 0 ? (byte)x : (byte)(x - 1);
        }

        public static double Dot(int[] g, ref double x, ref double y, ref double z)
        {
            return g[0] * x + g[1] * y + g[2] * z;
        }

        public static float Dot(int[] g, ref float x, ref float y, ref float z)
        {
            return g[0] * x + g[1] * y + g[2] * z;
        }

        public static double Dot(int[] g, ref double x, ref double y)
        {
            return g[0] * x + g[1] * y;
        }

        public static float Dot(int[] g, ref float x, ref float y)
        {
            return g[0] * x + g[1] * y;
        }

        public static float FullLerp(float MinTargetValue, float MaxTargetValue, double MinAmount, double MaxAmount, double amount, bool withClamp = false)
        {
            float result = MinTargetValue + (float)((MaxTargetValue - MinTargetValue) / (MaxAmount - MinAmount) * (amount - MinAmount));
            return withClamp ? MathHelper.Clamp(result, MinTargetValue, MaxTargetValue) : result;
        }

        public static float FullLerp(float MinTargetValue, float MaxTargetValue, Utopia.Shared.Math.Noises.NoiseResult Amounts, bool withClamp = false)
        {
            return FullLerp(MinTargetValue, MaxTargetValue, Amounts.MinValue, Amounts.MaxValue, Amounts.Value, withClamp);
        }

        public static float Clamp(float value, float min, float max)
        {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;
            return value;
        }

        //Count the number of bit inside the number
        public static int Bitcount(int n)
        {
            int count = 0;
            while (n != 0)
            {
                count++;
                n &= (n - 1);
            }
            return count;
        }
    }
}
