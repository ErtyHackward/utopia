using System;
using System.Collections.Generic;

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

    }
}
