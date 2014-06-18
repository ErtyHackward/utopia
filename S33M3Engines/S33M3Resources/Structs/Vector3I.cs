using System;
using System.ComponentModel;
using System.Globalization;
using SharpDX;

namespace S33M3Resources.Structs
{
    /// <summary>
    /// Defines a three component structure of System.Int32 type
    /// </summary>
    [TypeConverter(typeof(Vector3ITypeConverter))]
    public struct Vector3I : IComparable<Vector3I>
    {
        public int x;
        public int y;
        public int z;

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public int Z
        {
            get { return z; }
            set { z = value; }
        }

        public Vector3I(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3I(double x, double y, double z)
        {
            this.x = (int)x;
            this.y = (int)y;
            this.z = (int)z;
        }

        public Vector3I(int p)
        {
            x = p;
            y = p;
            z = p;
        }

        /// <summary>
        /// Returns length between vectors using sqrt
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static double Distance(Vector3I first, Vector3I second)
        {
            var dx = first.x - second.x;
            var dy = first.y - second.y;
            var dz = first.z - second.z;

            dx = dx * dx;
            dy = dy * dy;
            dz = dz * dz;

            return Math.Sqrt(dx + dy + dz);
        }

        public static double DistanceSquared(Vector3I first, Vector3I second)
        {
            var dx = first.x - second.x;
            var dy = first.y - second.y;
            var dz = first.z - second.z;

            return dx * dx + dy * dy + dz * dz;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj != null && obj.GetType() == GetType())
            {
                return CompareTo((Vector3I)obj);
            }
            return -1;
        }

        #endregion

        #region IComparable<ChunkPosition> Members

        public int CompareTo(Vector3I other)
        {
            if (x == other.x)
            {
                if (y == other.y)
                {
                    if (z == other.z)
                    {
                        return 0;
                    }
                    return z > other.z ? 1 : -1;
                }
                return y > other.y ? 1 : -1;
            }
            return x > other.x ? 1 : -1;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;
            var other = (Vector3I)obj;
            return x == other.x && y == other.y && z == other.z;
        }

        public override int GetHashCode()
        {
            return x + (y << 10) + (z << 20);
        }

        public override string ToString()
        {
            return string.Format("[{0:000}; {1:000}; {2:000}]", x, y, z);
        }

        public static implicit operator Vector3(Vector3I pos)
        {
            Vector3 vec;

            vec.X = pos.x;
            vec.Y = pos.y;
            vec.Z = pos.z;

            return vec;
        }

        public static explicit operator Vector3I(Vector3 vec)
        {
            return new Vector3I(Math.Floor(vec.X), Math.Floor(vec.Y), Math.Floor(vec.Z));
        }

        public static implicit operator Vector3D(Vector3I pos)
        {
            Vector3D vec;

            vec.X = pos.x;
            vec.Y = pos.y;
            vec.Z = pos.z;

            return vec;
        }

        public static explicit operator Vector3I(Vector3D vec)
        {
            return new Vector3I(Math.Floor(vec.X), Math.Floor(vec.Y), Math.Floor(vec.Z));
        }

        public static Vector3I operator *(Vector3I pos, int value)
        {
            Vector3I res;

            res.x = pos.x * value;
            res.y = pos.y * value;
            res.z = pos.z * value;

            return res;
        }

        public static Vector3I operator /(Vector3I pos, int value)
        {
            Vector3I res;

            res.x = pos.x / value;
            res.y = pos.y / value;
            res.z = pos.z / value;

            return res;
        }

        public static Vector3I operator +(Vector3I pos, Vector3I value)
        {
            Vector3I res;

            res.x = pos.x + value.x;
            res.y = pos.y + value.y;
            res.z = pos.z + value.z;

            return res;
        }

        public static Vector3I operator +(Vector3I pos, int value)
        {
            Vector3I res;

            res.x = pos.x + value;
            res.y = pos.y + value;
            res.z = pos.z + value;

            return res;
        }

        public static Vector3I operator -(Vector3I pos, int value)
        {
            Vector3I res;

            res.x = pos.x - value;
            res.y = pos.y - value;
            res.z = pos.z - value;

            return res;
        }

        public static Vector3I operator -(Vector3I pos, Vector3I other)
        {
            Vector3I res;

            res.x = pos.x - other.x;
            res.y = pos.y - other.y;
            res.z = pos.z - other.z;

            return res;
        }

        public static bool operator ==(Vector3I first, Vector3I second)
        {
            return first.x == second.x && first.y == second.y && first.z == second.z;
        }

        public static bool operator !=(Vector3I first, Vector3I second)
        {
            return !(first == second);
        }

        #endregion

        /// <summary>
        /// Gets Vector3I with values x = 1, y = 1, z = 1
        /// </summary>
        public static Vector3I One
        {
            get { return new Vector3I(1, 1, 1); }
        }

        /// <summary>
        /// Gets Vector3I with values x = 0, y = 0, z = 0
        /// </summary>
        public static Vector3I Zero
        {
            get { return new Vector3I(0, 0, 0); }
        }

        public static Vector3I Up
        {
            get { return new Vector3I(0, 1, 0); }
        }

        public static Vector3I Down
        {
            get { return new Vector3I(0, -1, 0); }
        }

        public static Vector3I Left
        {
            get { return new Vector3I(-1, 0, 0); }
        }

        public static Vector3I Right
        {
            get { return new Vector3I(1, 0, 0); }
        }

        public static Vector3I Front
        {
            get { return new Vector3I(0, 0, -1); }
        }

        public static Vector3I Back
        {
            get { return new Vector3I(0, 0, 1); }
        }

        public int Volume
        {
            get { return x * y * z; }
        }

        public bool IsZero()
        {
            return x == 0 && y == 0 && z == 0;
        }

        /// <summary>
        /// Returns a vector containing a smallest components of vectors provided
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static Vector3I Min(Vector3I vec1, Vector3I vec2)
        {
            Vector3I vec;
            
            vec.x = Math.Min(vec1.x, vec2.x);
            vec.y = Math.Min(vec1.y, vec2.y);
            vec.z = Math.Min(vec1.z, vec2.z);

            return vec;
        }

        /// <summary>
        /// Returns a vector containing a largest components of vectors provided
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static Vector3I Max(Vector3I vec1, Vector3I vec2)
        {
            Vector3I vec;

            vec.x = Math.Max(vec1.x, vec2.x);
            vec.y = Math.Max(vec1.y, vec2.y);
            vec.z = Math.Max(vec1.z, vec2.z);

            return vec;
        }

        //Property Grid editing Purpose
        internal class Vector3ITypeConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {   //This method is used to shown information in the PropertyGrid.
                if (destinationType == typeof(string))
                {
                    return ((Vector3I)value).ToString();
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(Vector3I), attributes).Sort(new string[] { "X", "Y", "Z" });
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
            {
                return new Vector3I((int)propertyValues["X"], (int)propertyValues["Y"], (int)propertyValues["Z"]);
            }

        }
    }
}
