using System;
using System.Collections.Generic;

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
