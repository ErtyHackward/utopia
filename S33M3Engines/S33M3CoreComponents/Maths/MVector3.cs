using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using SharpDX;

namespace S33M3CoreComponents.Maths
{
    public class MVector3
    {
        public static Vector3 Up = new Vector3(0, 1, 0);
        public static Vector3 Down = new Vector3(0, -1, 0);
        public static Vector3 Right = new Vector3(1, 0, 0);
        public static Vector3 Left = new Vector3(-1, 0, 0);

        private static readonly Vector3 _forward = new Vector3(0, 0, -1);
        public static Vector3 Forward
        {
            get
            {
                return _forward;
            }
        }

        private static readonly Vector3 _backward = new Vector3(0, 0, 1);
        public static Vector3 Backward
        {
            get
            {
                return _backward;
            }
        }

        public static void Dot(ref Vector3 vector1, ref Vector3 vector2, out float result)
        {
            result = ((vector1.X * vector2.X) + (vector1.Y * vector2.Y)) + (vector1.Z * vector2.Z);
        }

        public static float Distance(Vector3 value1, Vector3 value2)
        {
            float num3 = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num = value1.Z - value2.Z;
            float num4 = ((num3 * num3) + (num2 * num2)) + (num * num);
            return (float)Math.Sqrt((double)num4);
        }

        public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result)
        {
            float num3 = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num = value1.Z - value2.Z;
            float num4 = ((num3 * num3) + (num2 * num2)) + (num * num);
            result = (float)Math.Sqrt((double)num4);
        }

        public static float Distance(Vector3I value1, Vector3D value2)
        {
            float num3 = value1.X - (float)value2.X;
            float num2 = value1.Y - (float)value2.Y;
            float num = value1.Z - (float)value2.Z;
            float num4 = ((num3 * num3) + (num2 * num2)) + (num * num);
            return (float)Math.Sqrt((double)num4);
        }

        public static double DistanceSquared(Vector3D value1, Vector3D value2)
        {
            double num3 = value1.X - value2.X;
            double num2 = value1.Y - value2.Y;
            double num = value1.Z - value2.Z;
            return (((num3 * num3) + (num2 * num2)) + (num * num));
        }

        public static void DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result)
        {
            float num3 = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num = value1.Z - value2.Z;
            result = ((num3 * num3) + (num2 * num2)) + (num * num);
        }

        public static Vector3 Substract(ref Vector3 vectorValue, ref Vector3I locationValue)
        {
            return new Vector3(vectorValue.X - locationValue.X, vectorValue.Y - locationValue.Y, vectorValue.Z - locationValue.Z);
        }

        public static Vector3 Substract(Vector3 vectorValue, Vector3I locationValue)
        {
            return new Vector3(vectorValue.X - locationValue.X, vectorValue.Y - locationValue.Y, vectorValue.Z - locationValue.Z);
        }

        public static Vector3 Add(Vector3 vectorValue, Vector3I locationValue)
        {
            return new Vector3(vectorValue.X + locationValue.X, vectorValue.Y + locationValue.Y, vectorValue.Z + locationValue.Z);
        }

        public static void Divide(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
        }

        public static void Transform(Vector3[] sourceArray, ref Matrix matrix, Vector3[] destinationArray)
        {
            for (int i = 0; i < sourceArray.Length; i++)
            {
                float x = sourceArray[i].X;
                float y = sourceArray[i].Y;
                float z = sourceArray[i].Z;
                destinationArray[i].X = (((x * matrix.M11) + (y * matrix.M21)) + (z * matrix.M31)) + matrix.M41;
                destinationArray[i].Y = (((x * matrix.M12) + (y * matrix.M22)) + (z * matrix.M32)) + matrix.M42;
                destinationArray[i].Z = (((x * matrix.M13) + (y * matrix.M23)) + (z * matrix.M33)) + matrix.M43;
            }
        }

        public static void Transform(ref Vector3 position, ref Matrix matrix, out Vector3 result)
        {
            float num3 = (((position.X * matrix.M11) + (position.Y * matrix.M21)) + (position.Z * matrix.M31)) + matrix.M41;
            float num2 = (((position.X * matrix.M12) + (position.Y * matrix.M22)) + (position.Z * matrix.M32)) + matrix.M42;
            float num = (((position.X * matrix.M13) + (position.Y * matrix.M23)) + (position.Z * matrix.M33)) + matrix.M43;
            result.X = num3;
            result.Y = num2;
            result.Z = num;
        }

        public static void Round(ref Vector3 data, int decimals)
        {
            data.X = (float)Math.Round(data.X, decimals);
            data.Y = (float)Math.Round(data.Y, decimals);
            data.Z = (float)Math.Round(data.Z, decimals);
        }

        public static void Round(ref Vector3D data, int decimals)
        {
            data.X = Math.Round(data.X, decimals);
            data.Y = Math.Round(data.Y, decimals);
            data.Z = Math.Round(data.Z, decimals);
        }
    }
}
