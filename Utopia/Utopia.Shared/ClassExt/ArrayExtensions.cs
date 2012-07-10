using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.ClassExt
{
    public static class ArrayExtensions
    {
        public static T[] RemoveItemAt<T>(this T[] array, int index)
        {
            return null;
        }
    }

    public static class ArrayHelper
    {
        public static void RemoveAt<T>(ref T[] array, int index)
        {
            // resize the array
            var newArray = new T[array.Length - 1];
            Array.Copy(array, newArray, index);
            if (index != array.Length - 1)
                Array.Copy(array, index + 1, newArray, index, array.Length - 1 - index);
            array = newArray;
        }
    }
}
