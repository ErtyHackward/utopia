using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using System.Runtime.InteropServices;

namespace S33M3CoreComponents.Maths
{
    public static class MathHelper
    {
        //using System.Runtime.InteropServices;
        [StructLayout(LayoutKind.Explicit)]
        public struct IntsToLong
        {
            [FieldOffset(0)]
            public long LongValue;
            [FieldOffset(0)]
            public int LeftInt32;
            [FieldOffset(4)]
            public int RightInt32;
        }

        // Fields
        public const float E = 2.718282f;
        public const float Log10E = 0.4342945f;
        public const float Log2E = 1.442695f;
        public const float Pi = 3.141593f;
        public const float PiOver2 = 1.570796f;
        public const float PiOver4 = 0.7853982f;
        public const float TwoPi = 6.283185f;
        public const float SqrtOf2 = 1.4142135623730950488f;
        public const float SqrtOf3 = 1.7320508075688772935f;
        public const float SqrtOf5 = 2.2360679774997896964f;

        /// <summary>
        /// Unpack the given short (int16) value to an array of 2 bytes in big endian format.
        /// If the length of the buffer is too smal, it wil be resized.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="convert">the output buffer</param>
        static public byte[] UnpackBigUint16(short value, ref byte[] buffer)
        {

            if (buffer.Length < 2)
            {
                Array.Resize<byte>(ref buffer, 2);
            }
            buffer[0] = (byte)(value >> 8);
            buffer[1] = (byte)(value);
            return buffer;
        }

        /// <summary>
        /// Unpack the given short (int16) to an array of 2 bytes  in little endian format.
        /// If the length of the buffer is too smal, it wil be resized.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="convert">the output buffer</param>
        static public byte[] UnpackLittleUint16(short value, ref byte[] buffer)
        {

            if (buffer.Length < 2)
            {
                Array.Resize<byte>(ref buffer, 2);
            }
            buffer[0] = (byte)(value & 0x00ff);
            buffer[1] = (byte)((value & 0xff00) >> 8);
            return buffer;
        }

        /// <summary>
        /// Unpack the given integer (int32) to an array of 4 bytes  in little endian format.
        /// If the length of the buffer is too smal, it wil be resized.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="convert">the output buffer</param>
        static public byte[] UnpackLittleUint32(int value, ref byte[] buffer)
        {

            if (buffer.Length < 4)
            {
                Array.Resize<byte>(ref buffer, 4);
            }
            buffer[0] = (byte)(value & 0x00ff);
            buffer[1] = (byte)((value & 0xff00) >> 8);
            buffer[2] = (byte)((value & 0x00ff0000) >> 16);
            buffer[3] = (byte)((value & 0xff000000) >> 24);
            return buffer;
        }

        // Methods
        public static float Barycentric(float value1, float value2, float value3, float amount1, float amount2)
        {
            return ((value1 + (amount1 * (value2 - value1))) + (amount2 * (value3 - value1)));
        }

        public static float CatmullRom(float value1, float value2, float value3, float value4, float amount)
        {
            float num = amount * amount;
            float num2 = amount * num;
            return (0.5f * ((((2f * value2) + ((-value1 + value3) * amount)) + (((((2f * value1) - (5f * value2)) + (4f * value3)) - value4) * num)) + ((((-value1 + (3f * value2)) - (3f * value3)) + value4) * num2)));
        }

        public static float Clamp(float value, float min, float max)
        {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;
            return value;
        }

        public static double Clamp(double value, double min, double max)
        {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;
            return value;
        }

        public static float Distance(float value1, float value2)
        {
            return System.Math.Abs((float)(value1 - value2));
        }

        public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
        {
            float num3 = amount;
            float num = num3 * num3;
            float num2 = num3 * num;
            float num7 = ((2f * num2) - (3f * num)) + 1f;
            float num6 = (-2f * num2) + (3f * num);
            float num5 = (num2 - (2f * num)) + num3;
            float num4 = num2 - num;
            return ((((value1 * num7) + (value2 * num6)) + (tangent1 * num5)) + (tangent2 * num4));
        }

        public static float Lerp(float MinValue, float MaxValue, float amount)
        {
            return (MinValue + ((MaxValue - MinValue) * amount));
        }

        public static double Lerp(double MinValue, double MaxValue, double amount)
        {
            return (MinValue + ((MaxValue - MinValue) * amount));
        }

        /// <summary>
        /// Performs cubic interpolation between two values bound between two other values.
        ///
        /// The amount value should range from 0.0 to 1.0.  If the amount value is
        /// 0.0, this function returns n1.  If the amount value is 1.0, this
        /// function returns n2.
        /// </summary>
        /// <param name="n0">The value before the first value</param>
        /// <param name="n1">The first value</param>
        /// <param name="n2">The second value</param>
        /// <param name="n3">The value after the second value</param>
        /// <param name="a">the amount to interpolate between the two values</param>
        /// <returns>The interpolated value.</returns>
        public static float Cerp(float n0, float n1, float n2, float n3, float a)
        {
            float p = (n3 - n2) - (n0 - n1);
            float q = (n0 - n1) - p;
            float r = n2 - n0;
            float s = n1;
            return p * a * a * a + q * a * a + r * a + s;
        }

        /// <summary>
        /// Maps a value onto a cubic S-curve.
        /// a should range from 0.0 to 1.0.
        /// The derivitive of a cubic S-curve is zero at a = 0.0 and a = 1.0
        /// </summary>
        /// <param name="a">The value to map onto a cubic S-curve</param>
        /// <returns>The mapped value</returns>
        public static float SCurve3(float a)
        {
            return (a * a * (3.0f - 2.0f * a));
        }

        /// <summary>
        /// Maps a value onto a cubic S-curve.
        /// a should range from 0.0 to 1.0.
        /// The derivitive of a cubic S-curve is zero at a = 0.0 and a = 1.0
        /// </summary>
        /// <param name="a">The value to map onto a cubic S-curve</param>
        /// <returns>The mapped value</returns>
        public static double SCurve3(double a)
        {
            return (a * a * (3.0 - 2.0 * a));
        }

        /// <summary>
        /// Maps a value onto a quintic S-curve.
        /// a should range from 0.0 to 1.0.
        /// The first derivitive of a quintic S-curve is zero at a = 0.0 and a = 1.0
        /// The second derivitive of a quintic S-curve is zero at a = 0.0 and a = 1.0
        /// </summary>
        /// <param name="a">The value to map onto a quintic S-curve</param>
        /// <returns>The mapped value</returns>
        public static float SCurve5(float a)
        {
            return a * a * a * (a * (a * 6.0f - 15.0f) + 10.0f);

            /* original libnoise code
            double a3 = a * a * a;
            double a4 = a3 * a;
            double a5 = a4 * a;
            return (6.0 * a5) - (15.0 * a4) + (10.0 * a3);
            */
        }

        /// <summary>
        /// Maps a value onto a quintic S-curve.
        /// a should range from 0.0 to 1.0.
        /// The first derivitive of a quintic S-curve is zero at a = 0.0 and a = 1.0
        /// The second derivitive of a quintic S-curve is zero at a = 0.0 and a = 1.0
        /// </summary>
        /// <param name="a">The value to map onto a quintic S-curve</param>
        /// <returns>The mapped value</returns>
        public static double SCurve5(double a)
        {
            return a * a * a * (a * (a * 6.0 - 15.0) + 10.0);

            /* original libnoise code
            double a3 = a * a * a;
            double a4 = a3 * a;
            double a5 = a4 * a;
            return (6.0 * a5) - (15.0 * a4) + (10.0 * a3);
            */
        }

        public static double FullLerp(double MinTargetValue, double MaxTargetValue, double MinAmount, double MaxAmount, double amount, bool withClamp = false)
        {
            double result = MinTargetValue + (float)((MaxTargetValue - MinTargetValue) / (MaxAmount - MinAmount) * (amount - MinAmount));
            return withClamp ? MathHelper.Clamp(result, MinTargetValue, MaxTargetValue) : result;
        }

        public static float FullLerp(float MinTargetValue, float MaxTargetValue, double MinAmount, double MaxAmount, double amount, bool withClamp = false)
        {
            float result = MinTargetValue + (float)((MaxTargetValue - MinTargetValue) / (MaxAmount - MinAmount) * (amount - MinAmount));
            return withClamp ? MathHelper.Clamp(result, MinTargetValue, MaxTargetValue) : result;
        }

        public static float FullLerp(float MinTargetValue, float MaxTargetValue, S33M3CoreComponents.Maths.Noises.NoiseResult Amounts, bool withClamp = false)
        {
            return FullLerp(MinTargetValue, MaxTargetValue, Amounts.MinValue, Amounts.MaxValue, Amounts.Value, withClamp);
        }

        public static float Max(float value1, float value2)
        {
            return System.Math.Max(value1, value2);
        }

        public static float Min(float value1, float value2)
        {
            return System.Math.Min(value1, value2);
        }

        public static float SmoothStep(float value1, float value2, float amount)
        {
            float num = Clamp(amount, 0f, 1f);
            return Lerp(value1, value2, (num * num) * (3f - (2f * num)));
        }

        public static float ToDegrees(float radians)
        {
            return (radians * 57.29578f);
        }

        public static float ToRadians(float degrees)
        {
            return (degrees * 0.01745329f);
        }

        public static double ToDegrees(double radians)
        {
            return (radians * 57.29578);
        }

        public static double ToRadians(double degrees)
        {
            return (degrees * 0.01745329);
        }

        public static float WrapAngle(float angle)
        {
            angle = (float)System.Math.IEEERemainder((double)angle, 6.2831854820251465);
            if (angle <= -3.141593f)
            {
                angle += 6.283185f;
                return angle;
            }
            if (angle > 3.141593f)
            {
                angle -= 6.283185f;
            }
            return angle;
        }

        public static int Floor(double x)
        {
            //return x % 1 >= 0 ? (int)x : (int)x - 1;
            return (int)Math.Floor(x);
        }

        public static int Floor(float x)
        {
            //return x % 1 >= 0 ? (int)x : (int)x - 1;
            return (int)Math.Floor(x);
        }

        public static byte FloorByte(float x)
        {
            //return x % 1 >= 0 ? (byte)x : (byte)(x - 1);
            return (byte)Math.Floor(x);
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

        public static double Dot(int[] g, double x, double y, double z, double w)
        {
            return g[0] * x + g[1] * y + g[2] * z + g[3] * w;
        }

        public static double Dot(int[] g, double x, double y, double z)
        {
            return g[0] * x + g[1] * y + g[2] * z;
        }

        public static double Dot(int[] g, double x, double y)
        {
            return g[0] * x + g[1] * y;
        }

        public static int Mod(int value, int div)
        {
            int result = value % div;
            return result < 0 ? result + div : result;
        }

        /// <summary>
        /// Will give back the Least common multiple from a list of numbers
        /// </summary>
        /// <param name="Numbers"></param>
        /// <returns></returns>
        public static int LCM(List<int> Numbers)
        {
            if (Numbers == null) return 0;

            int lcmResult = -1;
            int workingNumber;

            foreach (var number in Numbers)
            {
                if (lcmResult == -1)
                {
                    lcmResult = number;
                    continue;
                }

                workingNumber = number;

                lcmResult = LCMComputing(lcmResult, workingNumber);

            }
            return lcmResult == -1 ? 0 : lcmResult;
        }

        private static int GCD(int a, int b)
        {
            // Make a >= b.
            a = Math.Abs(a);
            b = Math.Abs(b);
            if (a < b)
            {
                int tmp = a;
                a = b;
                b = tmp;
            }

            // Pull out remainders.
            for (; ; )
            {
                int remainder = a % b;
                if (remainder == 0) return b;
                a = b;
                b = remainder;
            };
        }

        // Return the least common multiple
        // (LCM) of two numbers.
        private static int LCMComputing(int a, int b)
        {
            return a * b / GCD(a, b);
        }

    }
}
