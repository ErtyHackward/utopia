using System;

namespace Utopia.Shared.Server.AStar
{
    public abstract class AStarNodeFunc<T> : IComparable 
    {
        public T Parent;

        /// <summary>
        /// Cost from the beggining
        /// </summary>
        public double Cost;

        /// <summary>
        /// Cost from the beginning plus goal estimate
        /// </summary>
        public double TotalCost;

        /// <summary>
        /// Determines wheather the current node is the same state as the on passed.
        /// </summary>
        /// <param name="aNode">AStarNode to compare the current node to</param>
        /// <returns>Returns true if they are the same state</returns>
        public abstract bool IsSameState(T aNode);

        public override bool Equals(object obj)
        {
            return IsSameState((T)obj);
        }

        public int CompareTo(object obj)
        {
            var otherNode = (AStarNodeFunc<T>)obj;

            if (IsSameState((T)obj))
                return 0;

            return otherNode.TotalCost.CompareTo(TotalCost);
        }
    }
}