using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using System.Globalization;

namespace S33M3Engines.Maths
{
[Serializable, StructLayout(LayoutKind.Sequential, Pack=4)]
public struct DVector3 : IEquatable<DVector3>
{
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
    public static DVector3 Zero
    {
        get
        {
            return new DVector3(0f, 0f, 0f);
        }
    }
    public static DVector3 UnitX
    {
        get
        {
            return new DVector3(1f, 0f, 0f);
        }
    }
    public static DVector3 UnitY
    {
        get
        {
            return new DVector3(0f, 1f, 0f);
        }
    }
    public static DVector3 UnitZ
    {
        get
        {
            return new DVector3(0f, 0f, 1f);
        }
    }
    public static int SizeInBytes
    {
        get
        {
            return Marshal.SizeOf(typeof(DVector3));
        }
    }
    public DVector3(double x, double y, double z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public DVector3(float x, float y, float z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public DVector3(Vector2 value, double z)
    {
        this.X = value.X;
        this.Y = value.Y;
        this.Z = z;
    }

    public DVector3(double value)
    {
        this.X = value;
        this.Y = value;
        this.Z = value;
    }

    public double Length()
    {
        double y = this.Y;
        double x = this.X;
        double z = this.Z;
        return (double) Math.Sqrt(((x * x) + (y * y)) + (z * z));
    }

    public double LengthSquared()
    {
        double y = this.Y;
        double x = this.X;
        double z = this.Z;
        return (double) (((x * x) + (y * y)) + (z * z));
    }

    public static void Normalize(ref DVector3 vector, out DVector3 result)
    {
        result = vector;
        result.Normalize();
    }

    public static DVector3 Normalize(DVector3 vector)
    {
        vector.Normalize();
        return vector;
    }

    public void Normalize()
    {
        double length = this.Length();
        if (length != 0f)
        {
            double num = (double) (1.0 / ((double) length));
            this.X *= num;
            this.Y *= num;
            this.Z *= num;
        }
    }

    public Vector3 AsVector3()
    {
        return new Vector3((float)X, (float)Y, (float)Z);
    }

    public static void Add(ref DVector3 left, ref DVector3 right, out DVector3 result)
    {
        DVector3 vector;
        vector.X = left.X + right.X;
        vector.Y = left.Y + right.Y;
        vector.Z = left.Z + right.Z;
        result = vector;
    }

    public static DVector3 Add(DVector3 left, DVector3 right)
    {
        DVector3 vector;
        vector.X = left.X + right.X;
        vector.Y = left.Y + right.Y;
        vector.Z = left.Z + right.Z;
        return vector;
    }

    public static void Subtract(ref DVector3 left, ref DVector3 right, out DVector3 result)
    {
        DVector3 vector;
        vector.X = left.X - right.X;
        vector.Y = left.Y - right.Y;
        vector.Z = left.Z - right.Z;
        result = vector;
    }

    public static DVector3 Subtract(DVector3 left, DVector3 right)
    {
        DVector3 vector;
        vector.X = left.X - right.X;
        vector.Y = left.Y - right.Y;
        vector.Z = left.Z - right.Z;
        return vector;
    }

    public static void Multiply(ref DVector3 value, double scale, out DVector3 result)
    {
        DVector3 vector;
        vector.X = value.X * scale;
        vector.Y = value.Y * scale;
        vector.Z = value.Z * scale;
        result = vector;
    }

    public static DVector3 Multiply(DVector3 value, double scale)
    {
        DVector3 vector;
        vector.X = value.X * scale;
        vector.Y = value.Y * scale;
        vector.Z = value.Z * scale;
        return vector;
    }

    public static void Modulate(ref DVector3 left, ref DVector3 right, out DVector3 result)
    {
        DVector3 vector;
        vector.X = left.X * right.X;
        vector.Y = left.Y * right.Y;
        vector.Z = left.Z * right.Z;
        result = vector;
    }

    public static DVector3 Modulate(DVector3 left, DVector3 right)
    {
        DVector3 vector;
        vector.X = left.X * right.X;
        vector.Y = left.Y * right.Y;
        vector.Z = left.Z * right.Z;
        return vector;
    }

    public static void Divide(ref DVector3 value, double scale, out DVector3 result)
    {
        DVector3 vector;
        vector.X = (double)(((double)value.X) / ((double)scale));
        vector.Y = (double)(((double)value.Y) / ((double)scale));
        vector.Z = (double)(((double)value.Z) / ((double)scale));
        result = vector;
    }

    public static DVector3 Divide(DVector3 value, double scale)
    {
        DVector3 vector;
        vector.X = (double) (((double) value.X) / ((double) scale));
        vector.Y = (double) (((double) value.Y) / ((double) scale));
        vector.Z = (double) (((double) value.Z) / ((double) scale));
        return vector;
    }

    public static void Negate(ref DVector3 value, out DVector3 result)
    {
        DVector3 vector;
        double num3 = -value.X;
        double num2 = -value.Y;
        double num = -value.Z;
        vector.X = num3;
        vector.Y = num2;
        vector.Z = num;
        result = vector;
    }

    public static DVector3 Negate(DVector3 value)
    {
        DVector3 vector;
        double num3 = -value.X;
        double num2 = -value.Y;
        double num = -value.Z;
        vector.X = num3;
        vector.Y = num2;
        vector.Z = num;
        return vector;
    }

    public static void Barycentric(ref DVector3 value1, ref DVector3 value2, ref DVector3 value3, double amount1, double amount2, out DVector3 result)
    {
        DVector3 vector;
        vector.X = (((value2.X - value1.X) * amount1) + value1.X) + ((value3.X - value1.X) * amount2);
        vector.Y = (((value2.Y - value1.Y) * amount1) + value1.Y) + ((value3.Y - value1.Y) * amount2);
        vector.Z = (((value2.Z - value1.Z) * amount1) + value1.Z) + ((value3.Z - value1.Z) * amount2);
        result = vector;
    }

    public static DVector3 Barycentric(DVector3 value1, DVector3 value2, DVector3 value3, double amount1, double amount2)
    {
        DVector3 vector = new DVector3();
        vector.X = (((value2.X - value1.X) * amount1) + value1.X) + ((value3.X - value1.X) * amount2);
        vector.Y = (((value2.Y - value1.Y) * amount1) + value1.Y) + ((value3.Y - value1.Y) * amount2);
        vector.Z = (((value2.Z - value1.Z) * amount1) + value1.Z) + ((value3.Z - value1.Z) * amount2);
        return vector;
    }

    public static void CatmullRom(ref DVector3 value1, ref DVector3 value2, ref DVector3 value3, ref DVector3 value4, double amount, out DVector3 result)
    {
        double num = amount;
        double squared = (double) (num * num);
        double cubed = squared * amount;
        DVector3 r = new DVector3();
        r.X = (double) ((((((((value1.X * 2.0) - (value2.X * 5.0)) + (value3.X * 4.0)) - value4.X) * squared) + (((value3.X - value1.X) * amount) + (value2.X * 2.0))) + (((((value2.X * 3.0) - value1.X) - (value3.X * 3.0)) + value4.X) * cubed)) * 0.5);
        r.Y = (double) ((((((((value1.Y * 2.0) - (value2.Y * 5.0)) + (value3.Y * 4.0)) - value4.Y) * squared) + (((value3.Y - value1.Y) * amount) + (value2.Y * 2.0))) + (((((value2.Y * 3.0) - value1.Y) - (value3.Y * 3.0)) + value4.Y) * cubed)) * 0.5);
        r.Z = (double) ((((((((value1.Z * 2.0) - (value2.Z * 5.0)) + (value3.Z * 4.0)) - value4.Z) * squared) + (((value3.Z - value1.Z) * amount) + (value2.Z * 2.0))) + (((((value2.Z * 3.0) - value1.Z) - (value3.Z * 3.0)) + value4.Z) * cubed)) * 0.5);
        result = r;
    }

    public static DVector3 CatmullRom(DVector3 value1, DVector3 value2, DVector3 value3, DVector3 value4, double amount)
    {
        DVector3 vector = new DVector3();
        double num = amount;
        double squared = (double) (num * num);
        double cubed = squared * amount;
        vector.X = (double) ((((((((value1.X * 2.0) - (value2.X * 5.0)) + (value3.X * 4.0)) - value4.X) * squared) + (((value3.X - value1.X) * amount) + (value2.X * 2.0))) + (((((value2.X * 3.0) - value1.X) - (value3.X * 3.0)) + value4.X) * cubed)) * 0.5);
        vector.Y = (double) ((((((((value1.Y * 2.0) - (value2.Y * 5.0)) + (value3.Y * 4.0)) - value4.Y) * squared) + (((value3.Y - value1.Y) * amount) + (value2.Y * 2.0))) + (((((value2.Y * 3.0) - value1.Y) - (value3.Y * 3.0)) + value4.Y) * cubed)) * 0.5);
        vector.Z = (double) ((((((((value1.Z * 2.0) - (value2.Z * 5.0)) + (value3.Z * 4.0)) - value4.Z) * squared) + (((value3.Z - value1.Z) * amount) + (value2.Z * 2.0))) + (((((value2.Z * 3.0) - value1.Z) - (value3.Z * 3.0)) + value4.Z) * cubed)) * 0.5);
        return vector;
    }

    public static void Clamp(ref DVector3 value, ref DVector3 min, ref DVector3 max, out DVector3 result)
    {
        double num;
        double num2;
        double num3;
        double num4;
        double num5;
        double num6;
        DVector3 vector;
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

    public static DVector3 Clamp(DVector3 value, DVector3 min, DVector3 max)
    {
        double num;
        double num2;
        double num3;
        double num4;
        double num5;
        double num6;
        DVector3 vector;
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

    public static void Hermite(ref DVector3 value1, ref DVector3 tangent1, ref DVector3 value2, ref DVector3 tangent2, double amount, out DVector3 result)
    {
        double num2 = amount;
        double squared = (double) (num2 * num2);
        double cubed = squared * amount;
        double num = squared * 3.0;
        double part1 = (double) (((cubed * 2.0) - num) + 1.0);
        double part2 = (double) ((cubed * -2.0) + num);
        double part3 = (cubed - ((double) (squared * 2.0))) + amount;
        double part4 = cubed - squared;
        result.X = (((value2.X * part2) + (value1.X * part1)) + (tangent1.X * part3)) + (tangent2.X * part4);
        result.Y = (((value2.Y * part2) + (value1.Y * part1)) + (tangent1.Y * part3)) + (tangent2.Y * part4);
        result.Z = (((value2.Z * part2) + (value1.Z * part1)) + (tangent1.Z * part3)) + (tangent2.Z * part4);
    }

    public static DVector3 Hermite(DVector3 value1, DVector3 tangent1, DVector3 value2, DVector3 tangent2, double amount)
    {
        DVector3 vector = new DVector3();
        double num2 = amount;
        double squared = (double) (num2 * num2);
        double cubed = squared * amount;
        double num = squared * 3.0;
        double part1 = (double) (((cubed * 2.0) - num) + 1.0);
        double part2 = (double) ((cubed * -2.0) + num);
        double part3 = (cubed - ((double) (squared * 2.0))) + amount;
        double part4 = cubed - squared;
        vector.X = (((value2.X * part2) + (value1.X * part1)) + (tangent1.X * part3)) + (tangent2.X * part4);
        vector.Y = (((value2.Y * part2) + (value1.Y * part1)) + (tangent1.Y * part3)) + (tangent2.Y * part4);
        vector.Z = (((value2.Z * part2) + (value1.Z * part1)) + (tangent1.Z * part3)) + (tangent2.Z * part4);
        return vector;
    }

    public static void Lerp(ref DVector3 start, ref DVector3 end, double amount, out DVector3 result)
    {
        result.X = ((end.X - start.X) * amount) + start.X;
        result.Y = ((end.Y - start.Y) * amount) + start.Y;
        result.Z = ((end.Z - start.Z) * amount) + start.Z;
    }

    public static DVector3 Lerp(DVector3 start, DVector3 end, double amount)
    {
        DVector3 vector = new DVector3();
        vector.X = ((end.X - start.X) * amount) + start.X;
        vector.Y = ((end.Y - start.Y) * amount) + start.Y;
        vector.Z = ((end.Z - start.Z) * amount) + start.Z;
        return vector;
    }

    public static void SmoothStep(ref DVector3 start, ref DVector3 end, double amount, out DVector3 result)
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
        amount = (double) ((3.0 - (num * 2.0)) * (num3 * num3));
        result.X = ((end.X - start.X) * amount) + start.X;
        result.Y = ((end.Y - start.Y) * amount) + start.Y;
        result.Z = ((end.Z - start.Z) * amount) + start.Z;
    }

    public static DVector3 SmoothStep(DVector3 start, DVector3 end, double amount)
    {
        double num;
        DVector3 vector = new DVector3();
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
        amount = (double) ((3.0 - (num * 2.0)) * (num3 * num3));
        vector.X = ((end.X - start.X) * amount) + start.X;
        vector.Y = ((end.Y - start.Y) * amount) + start.Y;
        vector.Z = ((end.Z - start.Z) * amount) + start.Z;
        return vector;
    }

    public static double Distance(DVector3 value1, DVector3 value2)
    {
        double x = value1.X - value2.X;
        double y = value1.Y - value2.Y;
        double z = value1.Z - value2.Z;
        double num3 = y;
        double num2 = x;
        double num = z;
        return (double) Math.Sqrt(((num2 * num2) + (num3 * num3)) + (num * num));
    }

    public static double DistanceSquared(DVector3 value1, DVector3 value2)
    {
        double x = value1.X - value2.X;
        double y = value1.Y - value2.Y;
        double z = value1.Z - value2.Z;
        double num3 = y;
        double num2 = x;
        double num = z;
        return (double) (((num2 * num2) + (num3 * num3)) + (num * num));
    }

    public static double Dot(DVector3 left, DVector3 right)
    {
        return (((left.Y * right.Y) + (left.X * right.X)) + (left.Z * right.Z));
    }

    public static void Cross(ref DVector3 left, ref DVector3 right, out DVector3 result)
    {
        DVector3 r = new DVector3();
        r.X = (left.Y * right.Z) - (left.Z * right.Y);
        r.Y = (left.Z * right.X) - (left.X * right.Z);
        r.Z = (left.X * right.Y) - (left.Y * right.X);
        result = r;
    }

    public static DVector3 Cross(DVector3 left, DVector3 right)
    {
        DVector3 result = new DVector3();
        result.X = (right.Z * left.Y) - (left.Z * right.Y);
        result.Y = (left.Z * right.X) - (right.Z * left.X);
        result.Z = (right.Y * left.X) - (left.Y * right.X);
        return result;
    }

    public static void Reflect(ref DVector3 vector, ref DVector3 normal, out DVector3 result)
    {
        double dot = ((vector.Y * normal.Y) + (vector.X * normal.X)) + (vector.Z * normal.Z);
        double num = dot * 2.0;
        result.X = vector.X - ((double) (normal.X * num));
        result.Y = vector.Y - ((double) (normal.Y * num));
        result.Z = vector.Z - ((double) (normal.Z * num));
    }

    public static DVector3 Reflect(DVector3 vector, DVector3 normal)
    {
        DVector3 result = new DVector3();
        double dot = ((vector.Y * normal.Y) + (vector.X * normal.X)) + (vector.Z * normal.Z);
        double num = dot * 2.0;
        result.X = vector.X - ((double) (normal.X * num));
        result.Y = vector.Y - ((double) (normal.Y * num));
        result.Z = vector.Z - ((double) (normal.Z * num));
        return result;
    }

    public static Vector4[] Transform(DVector3[] vectors, ref Quaternion rotation)
    {
        if (vectors == null)
        {
            throw new ArgumentNullException("vectors");
        }
        int count = vectors.Length;
        Vector4[] results = new Vector4[count];
        double num13 = rotation.X;
        double x = (double) (num13 + num13);
        double num12 = rotation.Y;
        double y = (double) (num12 + num12);
        double num11 = rotation.Z;
        double z = (double) (num11 + num11);
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
                r.X = (float) (((vectors[i].Y * num9) + (vectors[i].X * num10)) + (vectors[i].Z * num8));
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

   
    public static void Minimize(ref DVector3 value1, ref DVector3 value2, out DVector3 result)
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

    public static DVector3 Minimize(DVector3 value1, DVector3 value2)
    {
        double z;
        double y;
        double x;
        DVector3 vector = new DVector3();
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

    public static void Maximize(ref DVector3 value1, ref DVector3 value2, out DVector3 result)
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

    public static DVector3 Maximize(DVector3 value1, DVector3 value2)
    {
        double z;
        double y;
        double x;
        DVector3 vector = new DVector3();
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

    public static DVector3 operator +(DVector3 left, DVector3 right)
    {
        DVector3 vector;
        vector.X = left.X + right.X;
        vector.Y = left.Y + right.Y;
        vector.Z = left.Z + right.Z;
        return vector;
    }

    public static DVector3 operator +(DVector3 left, Vector3 right)
    {
        DVector3 vector;
        vector.X = left.X + right.X;
        vector.Y = left.Y + right.Y;
        vector.Z = left.Z + right.Z;
        return vector;
    }

    public static DVector3 operator -(DVector3 value)
    {
        DVector3 vector;
        double num3 = -value.X;
        double num2 = -value.Y;
        double num = -value.Z;
        vector.X = num3;
        vector.Y = num2;
        vector.Z = num;
        return vector;
    }

    public static DVector3 operator -(DVector3 left, DVector3 right)
    {
        DVector3 vector;
        vector.X = left.X - right.X;
        vector.Y = left.Y - right.Y;
        vector.Z = left.Z - right.Z;
        return vector;
    }

    public static DVector3 operator -(DVector3 left, Vector3 right)
    {
        DVector3 vector;
        vector.X = left.X - right.X;
        vector.Y = left.Y - right.Y;
        vector.Z = left.Z - right.Z;
        return vector;
    }

    public static DVector3 operator *(double scale, DVector3 vector)
    {
        return (DVector3) (vector * scale);
    }

    public static DVector3 operator *(DVector3 value, double scale)
    {
        DVector3 vector;
        vector.X = value.X * scale;
        vector.Y = value.Y * scale;
        vector.Z = value.Z * scale;
        return vector;
    }

    public static DVector3 operator /(DVector3 value, double scale)
    {
        DVector3 vector;
        vector.X = (double)(((double)value.X) / ((double)scale));
        vector.Y = (double)(((double)value.Y) / ((double)scale));
        vector.Z = (double)(((double)value.Z) / ((double)scale));
        return vector;
    }

    [return: MarshalAs(UnmanagedType.U1)]
    public static bool operator ==(DVector3 left, DVector3 right)
    {
        return Equals(ref left, ref right);
    }

    [return: MarshalAs(UnmanagedType.U1)]
    public static bool operator !=(DVector3 left, DVector3 right)
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
    public static bool Equals(ref DVector3 value1, ref DVector3 value2)
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
    public bool Equals(DVector3 other)
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
        return this.Equals((DVector3) obj);
    }
}
}
