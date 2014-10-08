using System;
using System.Collections.Generic;
using S33M3Resources.Structs;

namespace Utopia.Shared.ClassExt
{
    public static class RandomExtensions
    {
        /// <summary>
        /// Returns random element from the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="r"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Next<T>(this Random r, IList<T> list)
        {
            return list[r.Next(0, list.Count)];
        }

        /// <summary>
        /// Returns random element from the list, except the one provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="r"></param>
        /// <param name="list"></param>
        /// <param name="item">item to except</param>
        /// <returns></returns>
        public static T NextExcept<T>(this Random r, IList<T> list, T item)
        {
            if (list.Count < 2)
                throw new InvalidOperationException("Too few items in the list");

            while (true)
            {
                var take = list[r.Next(0, list.Count)];

                if (!take.Equals(item))
                    return take;
            }
        }

        public static Vector2I NextVector2IOnRadius(this Random rand, float radius)
        {
            var angle = rand.NextDouble() * Math.PI * 2;
            var x = radius * Math.Cos(angle);
            var y = radius * Math.Sin(angle);
            return new Vector2I((int)x, (int)y);
        }

        public static Vector2I NextVector2IInRadius(this Random rand, float radius)
        {
            var angle = rand.NextDouble() * Math.PI * 2;
            radius = (float)(rand.NextDouble() * radius);
            var x = radius * Math.Cos(angle);
            var y = radius * Math.Sin(angle);
            return new Vector2I((int)x, (int)y);
        }

    }
}
