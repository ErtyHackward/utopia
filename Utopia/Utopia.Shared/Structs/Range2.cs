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
        public IntVector2 Min { get; set; }
        public IntVector2 Max { get; set; }

        /// <summary>
        /// Gets size of the range
        /// </summary>
        public Location2<int> Size
        {
            get {
                Location2<int> rangeSize;
                rangeSize.X = Max.X - Min.X;
                rangeSize.Z = Max.Y - Min.Y;
                return rangeSize;
            }
        }

        /// <summary>
        /// Performs action for each point in this range
        /// </summary>
        /// <param name="action"></param>
        public void Foreach(Action<IntVector2> action)
        {
            for (int x = Min.X; x < Max.X; x++)
            {
                for (int y = Min.Y; y < Max.Y; y++)
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
            for (int x = Min.X; x < Max.X; x++)
            {
                for (int y = Min.Y; y < Max.Y; y++)
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
        public int Count { get { return (Max.X - Min.X)*(Max.Y - Min.Y); } }

        /// <summary>
        /// Indicates if range contains some point
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool Contains(IntVector2 position)
        {
            return Min.X <= position.X && Max.X > position.X && Min.Y <= position.Y && Max.Y > position.Y;
        }

        public IEnumerator<IntVector2> GetEnumerator()
        {
            for (int x = Min.X; x < Max.X; x++)
            {
                for (int y = Min.Y; y < Max.Y; y++)
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
