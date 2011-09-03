using System;
using System.Collections.Generic;
using SharpDX;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Represents two component range
    /// </summary>
    public struct Range2 : IEnumerable<IntVector2>
    {
        private IntVector2 _position;
        private IntVector2 _size;

        /// <summary>
        /// Creates new Range2
        /// </summary>
        /// <param name="position">Range top left position</param>
        /// <param name="size">Range size</param>
        public Range2(IntVector2 position, IntVector2 size)
        {
            _position = position;
            _size = size;
        }
        
        /// <summary>
        /// Gets or sets top left position of the range
        /// </summary>
        public IntVector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }
        
        /// <summary>
        /// Gets or sets size of this range
        /// </summary>
        public IntVector2 Size
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Performs action for each point in this range
        /// </summary>
        /// <param name="action"></param>
        public void Foreach(Action<IntVector2> action)
        {
            for (int x = _position.X; x < _position.X + _size.X; x++)
            {
                for (int y = _position.Y; y < _position.Y + _size.Y; y++)
                {
                    IntVector2 loc;
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
        public void Foreach(Action<IntVector2,int> action)
        {
            int index = 0;
            for (int x = _position.X; x < _position.X + _size.X; x++)
            {
                for (int y = _position.Y; y < _position.Y + _size.Y; y++)
                {
                    IntVector2 loc;
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
        public bool Contains(IntVector2 position)
        {
            return _position.X <= position.X && _position.X + _size.X > position.X && _position.Y <= position.Y && _position.Y + _size.Y > position.Y;
        }

        public IEnumerator<IntVector2> GetEnumerator()
        {
            for (int x = _position.X; x < _position.X + _size.X; x++)
            {
                for (int y = _position.Y; y < _position.Y + _size.Y; y++)
                {
                    IntVector2 loc;
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


    }
}
