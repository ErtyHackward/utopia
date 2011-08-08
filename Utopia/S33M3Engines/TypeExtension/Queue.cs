using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.TypeExtension
{
    public static class TypeExtension
    {
        public static void EnqueueDistinct<T>(this Queue<T> q, T queueObject)
        {
            //Check if the item is not already in the 
            for (int i = 0; i < q.Count; i++)
            {
                if (q.ElementAt(i).Equals(queueObject)) return;
            }

            q.Enqueue(queueObject);
        }
    }
}
