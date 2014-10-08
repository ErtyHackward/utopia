using System;
using System.Collections.Generic;
using ProtoBuf;
using System.Linq;
using S33M3Resources.Structs;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Represents two component range
    /// </summary>
    [ProtoContract]
    public struct Range2I : IEnumerable<Vector2I>
    {
        private Vector2I _position;
        private Vector2I _size;

        /// <summary>
        /// Creates new Range2
        /// </summary>
        /// <param name="position">Range top left position</param>
        /// <param name="size">Range size</param>
        public Range2I(Vector2I position, Vector2I size)
        {
            _position = position;
            _size = size;
        }
        
        /// <summary>
        /// Gets or sets top left position of the range
        /// </summary>
        [ProtoMember(1)]
        public Vector2I Position
        {
            get { return _position; }
            set { _position = value; }
        }
        
        /// <summary>
        /// Gets or sets size of this range
        /// </summary>
        [ProtoMember(2)]
        public Vector2I Size
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Performs action for each point in this range
        /// </summary>
        /// <param name="action"></param>
        public void Foreach(Action<Vector2I> action)
        {
            for (int x = _position.X; x < _position.X + _size.X; x++)
            {
                for (int y = _position.Y; y < _position.Y + _size.Y; y++)
                {
                    Vector2I loc;
                    loc.X = x;
                    loc.Y = y;
                    action(loc);
                }
            }
        }

        /// <summary>
        /// Performs action for each point in this range
        /// </summary>
        /// <param name="action"></param>
        public void Foreach(Action<Vector2I,int> action)
        {
            int index = 0;
            for (int x = _position.X; x < _position.X + _size.X; x++)
            {
                for (int y = _position.Y; y < _position.Y + _size.Y; y++)
                {
                    Vector2I loc;
                    loc.X = x;
                    loc.Y = y;
                    action(loc, index++);
                }
            }
        }

        /// <summary>
        /// Gets total items count
        /// </summary>
        public int Count { get { return _size.X * _size.Y; } }

        /// <summary>
        /// Indicates if range contains some point
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool Contains(Vector2I position)
        {
            return _position.X <= position.X && _position.X + _size.X > position.X && _position.Y <= position.Y && _position.Y + _size.Y > position.Y;
        }

        /// <summary>
        /// Enumerates all points in range excluding other range
        /// </summary>
        /// <param name="range"></param>
        /// <param name="exclude"></param>
        /// <returns></returns>
        public static IEnumerable<Vector2I> AllExclude(Range2I range, Range2I exclude)
        {
            if (range == exclude) return null;
            return range.Where(pos => !exclude.Contains(pos));
        }

        /// <summary>
        /// Enumerates all points in range excluding range specified
        /// </summary>
        /// <param name="exclude"></param>
        /// <returns></returns>
        public IEnumerable<Vector2I> AllExclude(Range2I exclude)
        {
            return AllExclude(this, exclude);
        }


        public IEnumerator<Vector2I> GetEnumerator()
        {
            for (int x = _position.X; x < _position.X + _size.X; x++)
            {
                for (int y = _position.Y; y < _position.Y + _size.Y; y++)
                {
                    Vector2I loc;
                    loc.X = x;
                    loc.Y = y;
                    yield return loc;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static bool operator ==(Range2I one, Range2I two)
        {
            return one.Position == two.Position && one.Size == two.Size;
        }

        public static bool operator !=(Range2I one, Range2I two)
        {
            return !(one == two);
        }

        public bool Equals(Range2I other)
        {
            return other.Position.Equals(Position) && other.Size.Equals(Size);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Range2I)) return false;
            return Equals((Range2I)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^ Size.GetHashCode();
            }
        }

    }
}
