using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Resources.Structs
{
    /// <summary>
    /// Will help doing float comparison with more reliability than normal Float
    /// You must use the smallest as possible Float value, this class only help for after coma problem
    /// Like when you do 1-0.9 that give 0.099999999... instead of 0.1.
    /// </summary>
    public struct FloatAsInt : IComparable, IComparable<FloatAsInt>, IEquatable<FloatAsInt>
    {
        // To work with a maximum precision of 4 numbers after the coma.
        public static int FloatPrecision = 10000;

        public int InternalValue;

        public FloatAsInt(float value)
        {
            InternalValue = Convert.ToInt32((value * FloatPrecision));
        }

        public FloatAsInt(int value)
        {
            InternalValue = value;
        }

        public override string ToString()
        {
            return (InternalValue / (float)FloatPrecision).ToString();
        }

        //Float to loatAsInt
        public static implicit operator FloatAsInt(float value)
        {
            return new FloatAsInt(value);
        }

        //Double to FloatAsInt
        public static implicit operator FloatAsInt(double value)
        {
            return new FloatAsInt((float)value);
        }

        //FloatAsInt to float conversion
        public static implicit operator float(FloatAsInt value)
        {
            return value.InternalValue / (float)FloatPrecision;
        }

        //FloatAsInt to Double conversion
        public static implicit operator double(FloatAsInt value)
        {
            return value.InternalValue / (double)FloatPrecision;
        }

        public int CompareTo(FloatAsInt other)
        {
            return InternalValue.CompareTo(other.InternalValue);
        }

        public override bool Equals(object obj)
        {
            return InternalValue == ((FloatAsInt)obj).InternalValue;
        }

        public bool Equals(FloatAsInt other)
        {
            return InternalValue == other.InternalValue;
        }

        public int CompareTo(object obj)
        {
            return InternalValue.CompareTo(((FloatAsInt)obj).InternalValue);
        }

        public static bool operator ==(FloatAsInt val1, FloatAsInt val2)
        {
            return val1.InternalValue == val2.InternalValue;
        }

        public static bool operator !=(FloatAsInt val1, FloatAsInt val2)
        {
            return val1.InternalValue != val2.InternalValue;
        }

        public static bool operator ==(FloatAsInt val1, float val2)
        {
            return val1.InternalValue == (int)(val2 * FloatPrecision);
        }

        public static bool operator ==(FloatAsInt val1, double val2)
        {
            return val1.InternalValue == (int)(val2 * FloatPrecision);
        }

        public static bool operator !=(FloatAsInt val1, float val2)
        {
            return val1.InternalValue != (int)(val2 * FloatPrecision);
        }

        public static bool operator !=(FloatAsInt val1, double val2)
        {
            return val1.InternalValue != (int)(val2 * FloatPrecision);
        }

        public static FloatAsInt operator +(FloatAsInt val1, FloatAsInt val2)
        {
            return new FloatAsInt(val1.InternalValue + val2.InternalValue);
        }

        public static FloatAsInt operator -(FloatAsInt val1, FloatAsInt val2)
        {
            return new FloatAsInt(val1.InternalValue - val2.InternalValue);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //public static FloatAsInt operator +(FloatAsInt val1, float val2)
        //{
        //    return new FloatAsInt(val1.InternalValue + (int)(val2 * FloatPrecision));
        //}

        //public static FloatAsInt operator -(FloatAsInt val1, float val2)
        //{
        //    return new FloatAsInt(val1.InternalValue - (int)(val2 * FloatPrecision));
        //}

        //public static FloatAsInt operator +(FloatAsInt val1, double val2)
        //{
        //    return new FloatAsInt(val1.InternalValue + (int)(val2 * FloatPrecision));
        //}

        //public static FloatAsInt operator -(FloatAsInt val1, double val2)
        //{
        //    return new FloatAsInt(val1.InternalValue - (int)(val2 * FloatPrecision));
        //}
    }
}
