using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Structs
{
    public struct Location3<T> where T : IEquatable<T>
    {
        public T X;
        public T Y;
        public T Z;

        public Location3(T X, T Y, T Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public override string ToString()
        {
            return "X:" + X.ToString() + " Y:" + Y.ToString() + " Z:" + Z.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(ref Location3<T> other)
        {
            return (((this.X.Equals(other.X)) && (this.Y.Equals(other.Y))) &&
                    (this.Z.Equals(other.Z)));
        }

        public static bool operator ==(Location3<T> left, Location3<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Location3<T> left, Location3<T> right)
        {
            return !left.Equals(right);
        }
    }
}
