using System;
using System.Collections.Generic;
using System.Linq;
using S33M3Resources.Structs;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Represents a range in 3d space by position and size points of Vector3I type
    /// </summary>
    public struct Range3I : IEnumerable<Vector3I>
    {
        /// <summary>
        /// Minimum point
        /// </summary>
        public Vector3I Position;

        /// <summary>
        /// Size of the range
        /// </summary>
        public Vector3I Size;

        public Vector3I Max
        {
            get { return Position + Size; }
        }

        public Range3I(Vector3I position, Vector3I size)
        {
            Position = position;
            Size = size;
        }

        /// <summary>
        /// Indicates if the point inside this range
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(Vector3I point)
        {
            return Position.X <= point.X && Position.X + Size.X > point.X &&
                   Position.Y <= point.Y && Position.Y + Size.Y > point.Y &&
                   Position.Z <= point.Z && Position.Z + Size.Z > point.Z;
        }

        /// <summary>
        /// Enumerates all points in range excluding other range
        /// </summary>
        /// <param name="range"></param>
        /// <param name="exclude"></param>
        /// <returns></returns>
        public static IEnumerable<Vector3I> AllExclude(Range3I range, Range3I exclude)
        {
            if (range == exclude) return null;
            return range.Where(pos => !exclude.Contains(pos));
        }

        /// <summary>
        /// Enumerates all points in range excluding range specified
        /// </summary>
        /// <param name="exclude"></param>
        /// <returns></returns>
        public IEnumerable<Vector3I> AllExclude(Range3I exclude)
        {
            return AllExclude(this, exclude);
        }

        public IEnumerator<Vector3I> GetEnumerator()
        {
            for (var x = Position.X; x < Position.X + Size.X; x++)
            {
                for (var y = Position.Y; y < Position.Y + Size.Y; y++)
                {
                    for (var z = Position.Z; z < Position.Z + Size.Z; z++)
                    {
                        Vector3I pos;

                        pos.x = x;
                        pos.y = y;
                        pos.z = z;

                        yield return pos;
                    }
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static bool operator ==(Range3I one, Range3I two)
        {
            return one.Position == two.Position && one.Size == two.Size;
        }

        public static bool operator !=(Range3I one, Range3I two)
        {
            return !(one == two);
        }

        public bool Equals(Range3I other)
        {
            return other.Position.Equals(Position) && other.Size.Equals(Size);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Range3I)) return false;
            return Equals((Range3I)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^ Size.GetHashCode();
            }
        }

        /// <summary>
        /// Creates a range from any two Vector3I objects
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static Range3I FromTwoVectors(Vector3I one, Vector3I two)
        {
            var min = Vector3I.Min(one, two);
            var max = Vector3I.Max(one, two);

            return new Range3I(min, max - min + Vector3I.One);
        }
    }
}