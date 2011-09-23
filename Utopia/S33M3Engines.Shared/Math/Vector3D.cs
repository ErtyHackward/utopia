using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using SharpDX;

namespace S33M3Engines.Shared.Math
{
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector3D : IEquatable<Vector3D>
    {
        public static Vector3D Up = new Vector3D(0, 1, 0);
        public static Vector3D Down = new Vector3D(0, -1, 0);
        public static Vector3D Right = new Vector3D(1, 0, 0);
        public static Vector3D Left = new Vector3D(-1, 0, 0);

        public double X;
        public double Y;
        public double Z;
        public double this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return this.X;
                }
                if (index == 1)
                {
                    return this.Y;
                }
                if (index != 2)
                {
                    throw new ArgumentOutOfRangeException("index", "Indices for DVector3 run from 0 to 2, inclusive.");
                }
                return this.Z;
            }
            set
            {
                if (index != 0)
                {
                    if (index != 1)
                    {
                        if (index != 2)
                        {
                            throw new ArgumentOutOfRangeException("index", "Indices for DVector3 run from 0 to 2, inclusive.");
                        }
                        this.Z = value;
                    }
                    else
                    {
                        this.Y = value;
                    }
                }
                else
                {
                    this.X = value;
                }
            }
        }
        public static Vector3D Zero
        {
            get
            {
                return new Vector3D(0f, 0f, 0f);
            }
        }
        public static Vector3D UnitX
        {
            get
            {
                return new Vector3D(1f, 0f, 0f);
            }
        }
        public static Vector3D UnitY
        {
            get
            {
                return new Vector3D(0f, 1f, 0f);
            }
        }
        public static Vector3D UnitZ
        {
            get
            {
                return new Vector3D(0f, 0f, 1f);
            }
        }
        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(Vector3D));
            }
        }
        public Vector3D(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3D(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3D(Vector2 value, double z)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = z;
        }

        public Vector3D(double value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
        }

        public Vector3D(Vector3 floatVector3)
        {
            this.X = floatVector3.X;
            this.Y = floatVector3.Y;
            this.Z = floatVector3.Z;
        }


        public double Length()
        {
            double y = this.Y;
            double x = this.X;
            double z = this.Z;
            return (double)System.Math.Sqrt(((x * x) + (y * y)) + (z * z));
        }

        public double LengthAbs()
        {
            double y = System.Math.Abs(this.Y);
            double x = System.Math.Abs(this.X);
            double z = System.Math.Abs(this.Z);
            return (double)System.Math.Sqrt(((x * x) + (y * y)) + (z * z));
        }

        public double LengthSquared()
        {
            double y = this.Y;
            double x = this.X;
            double z = this.Z;
            return (double)(((x * x) + (y * y)) + (z * z));
        }

        public static void Normalize(ref Vector3D vector, out Vector3D result)
        {
            result = vector;
            result.Normalize();
        }

        public static Vector3D Normalize(Vector3D vector)
        {
            vector.Normalize();
            return vector;
        }

        public void Normalize()
        {
            double length = this.Length();
            if (length != 0f)
            {
                double num = (double)(1.0 / ((double)length));
                this.X *= num;
                this.Y *= num;
                this.Z *= num;
            }
        }

        public Vector3 AsVector3()
        {
            return new Vector3((float)X, (float)Y, (float)Z);
        }

        public static void Add(ref Vector3D left, ref Vector3D right, out Vector3D result)
        {
            Vector3D vector;
            vector.X = left.X + right.X;
            vector.Y = left.Y + right.Y;
            vector.Z = left.Z + right.Z;
            result = vector;
        }

        public static Vector3D Add(Vector3D left, Vector3D right)
        {
            Vector3D vector;
            vector.X = left.X + right.X;
            vector.Y = left.Y + right.Y;
            vector.Z = left.Z + right.Z;
            return vector;
        }

        public static void Subtract(ref Vector3D left, ref Vector3D right, out Vector3D result)
        {
            Vector3D vector;
            vector.X = left.X - right.X;
            vector.Y = left.Y - right.Y;
            vector.Z = left.Z - right.Z;
            result = vector;
        }

        public static Vector3D Subtract(Vector3D left, Vector3D right)
        {
            Vector3D vector;
            vector.X = left.X - right.X;
            vector.Y = left.Y - right.Y;
            vector.Z = left.Z - right.Z;
            return vector;
        }

        public static void Multiply(ref Vector3D value, double scale, out Vector3D result)
        {
            Vector3D vector;
            vector.X = value.X * scale;
            vector.Y = value.Y * scale;
            vector.Z = value.Z * scale;
            result = vector;
        }

        public static Vector3D Multiply(Vector3D value, double scale)
        {
            Vector3D vector;
            vector.X = value.X * scale;
            vector.Y = value.Y * scale;
            vector.Z = value.Z * scale;
            return vector;
        }

        public static void Modulate(ref Vector3D left, ref Vector3D right, out Vector3D result)
        {
            Vector3D vector;
            vector.X = left.X * right.X;
            vector.Y = left.Y * right.Y;
            vector.Z = left.Z * right.Z;
            result = vector;
        }

        public static Vector3D Modulate(Vector3D left, Vector3D right)
        {
            Vector3D vector;
            vector.X = left.X * right.X;
            vector.Y = left.Y * right.Y;
            vector.Z = left.Z * right.Z;
            return vector;
        }

        public static void Divide(ref Vector3D value, double scale, out Vector3D result)
        {
            Vector3D vector;
            vector.X = (double)(((double)value.X) / ((double)scale));
            vector.Y = (double)(((double)value.Y) / ((double)scale));
            vector.Z = (double)(((double)value.Z) / ((double)scale));
            result = vector;
        }

        public static Vector3D Divide(Vector3D value, double scale)
        {
            Vector3D vector;
            vector.X = (double)(((double)value.X) / ((double)scale));
            vector.Y = (double)(((double)value.Y) / ((double)scale));
            vector.Z = (double)(((double)value.Z) / ((double)scale));
            return vector;
        }

        public static void Negate(ref Vector3D value, out Vector3D result)
        {
            Vector3D vector;
            double num3 = -value.X;
            double num2 = -value.Y;
            double num = -value.Z;
            vector.X = num3;
            vector.Y = num2;
            vector.Z = num;
            result = vector;
        }

        public static Vector3D Negate(Vector3D value)
        {
            Vector3D vector;
            double num3 = -value.X;
            double num2 = -value.Y;
            double num = -value.Z;
            vector.X = num3;
            vector.Y = num2;
            vector.Z = num;
            return vector;
        }

        public static void Barycentric(ref Vector3D value1, ref Vector3D value2, ref Vector3D value3, double amount1, double amount2, out Vector3D result)
        {
            Vector3D vector;
            vector.X = (((value2.X - value1.X) * amount1) + value1.X) + ((value3.X - value1.X) * amount2);
            vector.Y = (((value2.Y - value1.Y) * amount1) + value1.Y) + ((value3.Y - value1.Y) * amount2);
            vector.Z = (((value2.Z - value1.Z) * amount1) + value1.Z) + ((value3.Z - value1.Z) * amount2);
            result = vector;
        }

        public static Vector3D Barycentric(Vector3D value1, Vector3D value2, Vector3D value3, double amount1, double amount2)
        {
            Vector3D vector = new Vector3D();
            vector.X = (((value2.X - value1.X) * amount1) + value1.X) + ((value3.X - value1.X) * amount2);
            vector.Y = (((value2.Y - value1.Y) * amount1) + value1.Y) + ((value3.Y - value1.Y) * amount2);
            vector.Z = (((value2.Z - value1.Z) * amount1) + value1.Z) + ((value3.Z - value1.Z) * amount2);
            return vector;
        }

        public static void CatmullRom(ref Vector3D value1, ref Vector3D value2, ref Vector3D value3, ref Vector3D value4, double amount, out Vector3D result)
        {
            double num = amount;
            double squared = (double)(num * num);
            double cubed = squared * amount;
            Vector3D r = new Vector3D();
            r.X = (double)((((((((value1.X * 2.0) - (value2.X * 5.0)) + (value3.X * 4.0)) - value4.X) * squared) + (((value3.X - value1.X) * amount) + (value2.X * 2.0))) + (((((value2.X * 3.0) - value1.X) - (value3.X * 3.0)) + value4.X) * cubed)) * 0.5);
            r.Y = (double)((((((((value1.Y * 2.0) - (value2.Y * 5.0)) + (value3.Y * 4.0)) - value4.Y) * squared) + (((value3.Y - value1.Y) * amount) + (value2.Y * 2.0))) + (((((value2.Y * 3.0) - value1.Y) - (value3.Y * 3.0)) + value4.Y) * cubed)) * 0.5);
            r.Z = (double)((((((((value1.Z * 2.0) - (value2.Z * 5.0)) + (value3.Z * 4.0)) - value4.Z) * squared) + (((value3.Z - value1.Z) * amount) + (value2.Z * 2.0))) + (((((value2.Z * 3.0) - value1.Z) - (value3.Z * 3.0)) + value4.Z) * cubed)) * 0.5);
            result = r;
        }

        public static Vector3D CatmullRom(Vector3D value1, Vector3D value2, Vector3D value3, Vector3D value4, double amount)
        {
            Vector3D vector = new Vector3D();
            double num = amount;
            double squared = (double)(num * num);
            double cubed = squared * amount;
            vector.X = (double)((((((((value1.X * 2.0) - (value2.X * 5.0)) + (value3.X * 4.0)) - value4.X) * squared) + (((value3.X - value1.X) * amount) + (value2.X * 2.0))) + (((((value2.X * 3.0) - value1.X) - (value3.X * 3.0)) + value4.X) * cubed)) * 0.5);
            vector.Y = (double)((((((((value1.Y * 2.0) - (value2.Y * 5.0)) + (value3.Y * 4.0)) - value4.Y) * squared) + (((value3.Y - value1.Y) * amount) + (value2.Y * 2.0))) + (((((value2.Y * 3.0) - value1.Y) - (value3.Y * 3.0)) + value4.Y) * cubed)) * 0.5);
            vector.Z = (double)((((((((value1.Z * 2.0) - (value2.Z * 5.0)) + (value3.Z * 4.0)) - value4.Z) * squared) + (((value3.Z - value1.Z) * amount) + (value2.Z * 2.0))) + (((((value2.Z * 3.0) - value1.Z) - (value3.Z * 3.0)) + value4.Z) * cubed)) * 0.5);
            return vector;
        }

        public static void Clamp(ref Vector3D value, ref Vector3D min, ref Vector3D max, out Vector3D result)
        {
            double num;
            double num2;
            double num3;
            double num4;
            double num5;
            double num6;
            Vector3D vector;
            double x = value.X;
            if (x > max.X)
            {
                num3 = max.X;
            }
            else
            {
                num3 = x;
            }
            if (num3 < min.X)
            {
                num6 = min.X;
            }
            else
            {
                num6 = num3;
            }
            double y = value.Y;
            if (y > max.Y)
            {
                num2 = max.Y;
            }
            else
            {
                num2 = y;
            }
            if (num2 < min.Y)
            {
                num5 = min.Y;
            }
            else
            {
                num5 = num2;
            }
            double z = value.Z;
            if (z > max.Z)
            {
                num = max.Z;
            }
            else
            {
                num = z;
            }
            if (num < min.Z)
            {
                num4 = min.Z;
            }
            else
            {
                num4 = num;
            }
            vector.X = num6;
            vector.Y = num5;
            vector.Z = num4;
            result = vector;
        }

        public static Vector3D Clamp(Vector3D value, Vector3D min, Vector3D max)
        {
            double num;
            double num2;
            double num3;
            double num4;
            double num5;
            double num6;
            Vector3D vector;
            double x = value.X;
            if (x > max.X)
            {
                num3 = max.X;
            }
            else
            {
                num3 = x;
            }
            if (num3 < min.X)
            {
                num6 = min.X;
            }
            else
            {
                num6 = num3;
            }
            double y = value.Y;
            if (y > max.Y)
            {
                num2 = max.Y;
            }
            else
            {
                num2 = y;
            }
            if (num2 < min.Y)
            {
                num5 = min.Y;
            }
            else
            {
                num5 = num2;
            }
            double z = value.Z;
            if (z > max.Z)
            {
                num = max.Z;
            }
            else
            {
                num = z;
            }
            if (num < min.Z)
            {
                num4 = min.Z;
            }
            else
            {
                num4 = num;
            }
            vector.X = num6;
            vector.Y = num5;
            vector.Z = num4;
            return vector;
        }

        public static void Hermite(ref Vector3D value1, ref Vector3D tangent1, ref Vector3D value2, ref Vector3D tangent2, double amount, out Vector3D result)
        {
            double num2 = amount;
            double squared = (double)(num2 * num2);
            double cubed = squared * amount;
            double num = squared * 3.0;
            double part1 = (double)(((cubed * 2.0) - num) + 1.0);
            double part2 = (double)((cubed * -2.0) + num);
            double part3 = (cubed - ((double)(squared * 2.0))) + amount;
            double part4 = cubed - squared;
            result.X = (((value2.X * part2) + (value1.X * part1)) + (tangent1.X * part3)) + (tangent2.X * part4);
            result.Y = (((value2.Y * part2) + (value1.Y * part1)) + (tangent1.Y * part3)) + (tangent2.Y * part4);
            result.Z = (((value2.Z * part2) + (value1.Z * part1)) + (tangent1.Z * part3)) + (tangent2.Z * part4);
        }

        public static Vector3D Hermite(Vector3D value1, Vector3D tangent1, Vector3D value2, Vector3D tangent2, double amount)
        {
            Vector3D vector = new Vector3D();
            double num2 = amount;
            double squared = (double)(num2 * num2);
            double cubed = squared * amount;
            double num = squared * 3.0;
            double part1 = (double)(((cubed * 2.0) - num) + 1.0);
            double part2 = (double)((cubed * -2.0) + num);
            double part3 = (cubed - ((double)(squared * 2.0))) + amount;
            double part4 = cubed - squared;
            vector.X = (((value2.X * part2) + (value1.X * part1)) + (tangent1.X * part3)) + (tangent2.X * part4);
            vector.Y = (((value2.Y * part2) + (value1.Y * part1)) + (tangent1.Y * part3)) + (tangent2.Y * part4);
            vector.Z = (((value2.Z * part2) + (value1.Z * part1)) + (tangent1.Z * part3)) + (tangent2.Z * part4);
            return vector;
        }

        public static void Lerp(ref Vector3D start, ref Vector3D end, double amount, out Vector3D result)
        {
            result.X = ((end.X - start.X) * amount) + start.X;
            result.Y = ((end.Y - start.Y) * amount) + start.Y;
            result.Z = ((end.Z - start.Z) * amount) + start.Z;
        }

        public static Vector3D Lerp(Vector3D start, Vector3D end, double amount)
        {
            Vector3D vector = new Vector3D();
            vector.X = ((end.X - start.X) * amount) + start.X;
            vector.Y = ((end.Y - start.Y) * amount) + start.Y;
            vector.Z = ((end.Z - start.Z) * amount) + start.Z;
            return vector;
        }

        public static void SmoothStep(ref Vector3D start, ref Vector3D end, double amount, out Vector3D result)
        {
            double num;
            if (amount > 1f)
            {
                num = 1f;
            }
            else
            {
                double num2;
                if (amount < 0f)
                {
                    num2 = 0f;
                }
                else
                {
                    num2 = amount;
                }
                num = num2;
            }
            double num3 = num;
            amount = (double)((3.0 - (num * 2.0)) * (num3 * num3));
            result.X = ((end.X - start.X) * amount) + start.X;
            result.Y = ((end.Y - start.Y) * amount) + start.Y;
            result.Z = ((end.Z - start.Z) * amount) + start.Z;
        }

        public static Vector3D SmoothStep(Vector3D start, Vector3D end, double amount)
        {
            double num;
            Vector3D vector = new Vector3D();
            if (amount > 1f)
            {
                num = 1f;
            }
            else
            {
                double num2;
                if (amount < 0f)
                {
                    num2 = 0f;
                }
                else
                {
                    num2 = amount;
                }
                num = num2;
            }
            double num3 = num;
            amount = (double)((3.0 - (num * 2.0)) * (num3 * num3));
            vector.X = ((end.X - start.X) * amount) + start.X;
            vector.Y = ((end.Y - start.Y) * amount) + start.Y;
            vector.Z = ((end.Z - start.Z) * amount) + start.Z;
            return vector;
        }

        public static double Distance(Vector3D value1, Vector3D value2)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;
            double num3 = y;
            double num2 = x;
            double num = z;
            return (double)System.Math.Sqrt(((num2 * num2) + (num3 * num3)) + (num * num));
        }

        public static double DistanceSquared(Vector3D value1, Vector3D value2)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;
            double num3 = y;
            double num2 = x;
            double num = z;
            return (double)(((num2 * num2) + (num3 * num3)) + (num * num));
        }

        public static double Dot(Vector3D left, Vector3D right)
        {
            return (((left.Y * right.Y) + (left.X * right.X)) + (left.Z * right.Z));
        }

        public static void Cross(ref Vector3D left, ref Vector3D right, out Vector3D result)
        {
            Vector3D r = new Vector3D();
            r.X = (left.Y * right.Z) - (left.Z * right.Y);
            r.Y = (left.Z * right.X) - (left.X * right.Z);
            r.Z = (left.X * right.Y) - (left.Y * right.X);
            result = r;
        }

        public static Vector3D Cross(Vector3D left, Vector3D right)
        {
            Vector3D result = new Vector3D();
            result.X = (right.Z * left.Y) - (left.Z * right.Y);
            result.Y = (left.Z * right.X) - (right.Z * left.X);
            result.Z = (right.Y * left.X) - (left.Y * right.X);
            return result;
        }

        public static void Reflect(ref Vector3D vector, ref Vector3D normal, out Vector3D result)
        {
            double dot = ((vector.Y * normal.Y) + (vector.X * normal.X)) + (vector.Z * normal.Z);
            double num = dot * 2.0;
            result.X = vector.X - ((double)(normal.X * num));
            result.Y = vector.Y - ((double)(normal.Y * num));
            result.Z = vector.Z - ((double)(normal.Z * num));
        }

        public static Vector3D Reflect(Vector3D vector, Vector3D normal)
        {
            Vector3D result = new Vector3D();
            double dot = ((vector.Y * normal.Y) + (vector.X * normal.X)) + (vector.Z * normal.Z);
            double num = dot * 2.0;
            result.X = vector.X - ((double)(normal.X * num));
            result.Y = vector.Y - ((double)(normal.Y * num));
            result.Z = vector.Z - ((double)(normal.Z * num));
            return result;
        }

        public static Vector4[] Transform(Vector3D[] vectors, ref Quaternion rotation)
        {
            if (vectors == null)
            {
                throw new ArgumentNullException("vectors");
            }
            int count = vectors.Length;
            Vector4[] results = new Vector4[count];
            double num13 = rotation.X;
            double x = (double)(num13 + num13);
            double num12 = rotation.Y;
            double y = (double)(num12 + num12);
            double num11 = rotation.Z;
            double z = (double)(num11 + num11);
            double wx = rotation.W * x;
            double wy = rotation.W * y;
            double wz = rotation.W * z;
            double xx = rotation.X * x;
            double xy = rotation.X * y;
            double xz = rotation.X * z;
            double yy = rotation.Y * y;
            double yz = rotation.Y * z;
            double zz = rotation.Z * z;
            int i = 0;
            if (0 < count)
            {
                double num10 = (1.0 - yy) - zz;
                double num9 = xy - wz;
                double num8 = xz + wy;
                double num = 1.0 - xx;
                double num7 = num - zz;
                double num6 = xy + wz;
                double num5 = yz - wx;
                double num4 = xz - wy;
                double num3 = yz + wx;
                double num2 = num - yy;
                do
                {
                    Vector4 r = new Vector4();
                    r.X = (float)(((vectors[i].Y * num9) + (vectors[i].X * num10)) + (vectors[i].Z * num8));
                    r.Y = (float)(((vectors[i].X * num6) + (vectors[i].Y * num7)) + (vectors[i].Z * num5));
                    r.Z = (float)(((vectors[i].Y * num3) + (vectors[i].X * num4)) + (vectors[i].Z * num2));
                    r.W = 1f;
                    results[i] = r;
                    i++;
                }
                while (i < count);
            }
            return results;
        }

        public static void Transform(ref Vector3 value, ref Quaternion rotation, out Vector4 result)
        {
            double num4 = rotation.X;
            double x = (double)(num4 + num4);
            double num3 = rotation.Y;
            double y = (double)(num3 + num3);
            double num2 = rotation.Z;
            double z = (double)(num2 + num2);
            double wx = rotation.W * x;
            double wy = rotation.W * y;
            double wz = rotation.W * z;
            double xx = rotation.X * x;
            double xy = rotation.X * y;
            double xz = rotation.X * z;
            double yy = rotation.Y * y;
            double yz = rotation.Y * z;
            double zz = rotation.Z * z;
            result.X = (float)((((value.X * ((1.0 - yy) - zz))) + (value.Y * (xy - wz))) + (value.Z * (xz + wy)));
            double num = 1.0 - xx;
            result.Y = (float)(((value.X * (xy + wz)) + ((value.Y * (num - zz)))) + (value.Z * (yz - wx)));
            result.Z = (float)(((value.X * (xz - wy)) + (value.Y * (yz + wx))) + ((value.Z * (num - yy))));
            result.W = 1f;
        }

        public static Vector4 Transform(Vector3 value, Quaternion rotation)
        {
            Vector4 vector = new Vector4();
            double num4 = rotation.X;
            double x = (double)(num4 + num4);
            double num3 = rotation.Y;
            double y = (double)(num3 + num3);
            double num2 = rotation.Z;
            double z = (double)(num2 + num2);
            double wx = rotation.W * x;
            double wy = rotation.W * y;
            double wz = rotation.W * z;
            double xx = rotation.X * x;
            double xy = rotation.X * y;
            double xz = rotation.X * z;
            double yy = rotation.Y * y;
            double yz = rotation.Y * z;
            double zz = rotation.Z * z;
            vector.X = (float)((((value.X * ((1.0 - yy) - zz))) + (value.Y * (xy - wz))) + (value.Z * (xz + wy)));
            double num = 1.0 - xx;
            vector.Y = (float)(((value.X * (xy + wz)) + ((value.Y * (num - zz)))) + (value.Z * (yz - wx)));
            vector.Z = (float)(((value.X * (xz - wy)) + (value.Y * (yz + wx))) + ((value.Z * (num - yy))));
            vector.W = 1f;
            return vector;
        }


        public static void Minimize(ref Vector3D value1, ref Vector3D value2, out Vector3D result)
        {
            double z;
            double y;
            double x;
            if (value1.X < value2.X)
            {
                x = value1.X;
            }
            else
            {
                x = value2.X;
            }
            result.X = x;
            if (value1.Y < value2.Y)
            {
                y = value1.Y;
            }
            else
            {
                y = value2.Y;
            }
            result.Y = y;
            if (value1.Z < value2.Z)
            {
                z = value1.Z;
            }
            else
            {
                z = value2.Z;
            }
            result.Z = z;
        }

        public static Vector3D Minimize(Vector3D value1, Vector3D value2)
        {
            double z;
            double y;
            double x;
            Vector3D vector = new Vector3D();
            if (value1.X < value2.X)
            {
                x = value1.X;
            }
            else
            {
                x = value2.X;
            }
            vector.X = x;
            if (value1.Y < value2.Y)
            {
                y = value1.Y;
            }
            else
            {
                y = value2.Y;
            }
            vector.Y = y;
            if (value1.Z < value2.Z)
            {
                z = value1.Z;
            }
            else
            {
                z = value2.Z;
            }
            vector.Z = z;
            return vector;
        }

        public static void Maximize(ref Vector3D value1, ref Vector3D value2, out Vector3D result)
        {
            double z;
            double y;
            double x;
            if (value1.X > value2.X)
            {
                x = value1.X;
            }
            else
            {
                x = value2.X;
            }
            result.X = x;
            if (value1.Y > value2.Y)
            {
                y = value1.Y;
            }
            else
            {
                y = value2.Y;
            }
            result.Y = y;
            if (value1.Z > value2.Z)
            {
                z = value1.Z;
            }
            else
            {
                z = value2.Z;
            }
            result.Z = z;
        }

        public static Vector3D Maximize(Vector3D value1, Vector3D value2)
        {
            double z;
            double y;
            double x;
            Vector3D vector = new Vector3D();
            if (value1.X > value2.X)
            {
                x = value1.X;
            }
            else
            {
                x = value2.X;
            }
            vector.X = x;
            if (value1.Y > value2.Y)
            {
                y = value1.Y;
            }
            else
            {
                y = value2.Y;
            }
            vector.Y = y;
            if (value1.Z > value2.Z)
            {
                z = value1.Z;
            }
            else
            {
                z = value2.Z;
            }
            vector.Z = z;
            return vector;
        }

        public static Vector3D operator +(Vector3D left, Vector3D right)
        {
            Vector3D vector;
            vector.X = left.X + right.X;
            vector.Y = left.Y + right.Y;
            vector.Z = left.Z + right.Z;
            return vector;
        }

        public static Vector3D operator +(Vector3D left, Vector3 right)
        {
            Vector3D vector;
            vector.X = left.X + right.X;
            vector.Y = left.Y + right.Y;
            vector.Z = left.Z + right.Z;
            return vector;
        }

        public static Vector3D operator -(Vector3D value)
        {
            Vector3D vector;
            double num3 = -value.X;
            double num2 = -value.Y;
            double num = -value.Z;
            vector.X = num3;
            vector.Y = num2;
            vector.Z = num;
            return vector;
        }

        public static Vector3D operator -(Vector3D left, Vector3D right)
        {
            Vector3D vector;
            vector.X = left.X - right.X;
            vector.Y = left.Y - right.Y;
            vector.Z = left.Z - right.Z;
            return vector;
        }

        public static Vector3D operator -(Vector3D left, Vector3 right)
        {
            Vector3D vector;
            vector.X = left.X - right.X;
            vector.Y = left.Y - right.Y;
            vector.Z = left.Z - right.Z;
            return vector;
        }

        public static Vector3D operator *(double scale, Vector3D vector)
        {
            return (Vector3D)(vector * scale);
        }

        public static Vector3D operator *(Vector3D value, double scale)
        {
            Vector3D vector;
            vector.X = value.X * scale;
            vector.Y = value.Y * scale;
            vector.Z = value.Z * scale;
            return vector;
        }

        public static Vector3D operator /(Vector3D value, double scale)
        {
            Vector3D vector;
            vector.X = (double)(((double)value.X) / ((double)scale));
            vector.Y = (double)(((double)value.Y) / ((double)scale));
            vector.Z = (double)(((double)value.Z) / ((double)scale));
            return vector;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static bool operator ==(Vector3D left, Vector3D right)
        {
            return Equals(ref left, ref right);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static bool operator !=(Vector3D left, Vector3D right)
        {
            return !Equals(ref left, ref right);
        }

        public override string ToString()
        {
            object[] args = new object[] { this.X.ToString(CultureInfo.CurrentCulture), this.Y.ToString(CultureInfo.CurrentCulture), this.Z.ToString(CultureInfo.CurrentCulture) };
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", args);
        }

        public override int GetHashCode()
        {
            double x = this.X;
            double y = this.Y;
            double z = this.Z;
            int num = y.GetHashCode() + z.GetHashCode();
            return (x.GetHashCode() + num);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static bool Equals(ref Vector3D value1, ref Vector3D value2)
        {
            bool num;
            if (((value1.X == value2.X) && (value1.Y == value2.Y)) && (value1.Z == value2.Z))
            {
                num = true;
            }
            else
            {
                num = false;
            }
            return num;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool Equals(Vector3D other)
        {
            bool num;
            if (((this.X == other.X) && (this.Y == other.Y)) && (this.Z == other.Z))
            {
                num = true;
            }
            else
            {
                num = false;
            }
            return num;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            return this.Equals((Vector3D)obj);
        }
    }
}
