using System;
using System.Collections.Generic;

namespace Utopia.Shared.Server.AStar
{
    /// <summary>
    /// Base class for pathfinding nodes, it holds no actual information about the map. 
    /// An inherited class must be constructed from this class and all virtual methods must be 
    /// implemented. Note, that calling base() in the overridden methods is not needed.
    /// </summary>
    public abstract class AStarNode<T> : IComparable
    {
        #region Properties

        /// <summary>
        /// The parent of the node.
        /// </summary>
        public T Parent { get; set; }

        /// <summary>
        /// The accumulative cost of the path until now.
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        /// The estimated cost to the goal from here.
        /// </summary>
        public double GoalEstimate 
        {
            get 
            {
                if(double.IsNaN(_fGoalEstimate))
                    _fGoalEstimate = Estimate();
                return _fGoalEstimate;
            }
        }
        private double _fGoalEstimate = double.NaN;

        /// <summary>
        /// The cost plus the estimated cost to the goal from here.
        /// </summary>
        public double TotalCost
        {
            get 
            {
                return (Cost + GoalEstimate);
            }
        }

        /// <summary>
        /// The goal node.
        /// </summary>
        public T GoalNode { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="aParent">The node's parent</param>
        /// <param name="aGoalNode">The goal node</param>
        /// <param name="aCost">The accumulative cost until now</param>
        protected AStarNode(T aParent, T aGoalNode, double aCost)
        {
            Parent = aParent;
            Cost = aCost;
            GoalNode = aGoalNode;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines whether the current node is the goal.
        /// </summary>
        /// <returns>Returns true if current node is the goal</returns>
        public virtual bool IsGoal()
        {
            return IsSameState(GoalNode);
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Determines wheather the current node is the same state as the on passed.
        /// </summary>
        /// <param name="aNode">AStarNode to compare the current node to</param>
        /// <returns>Returns true if they are the same state</returns>
        public abstract bool IsSameState(T aNode);
        
        /// <summary>
        /// Calculates the estimated cost for the remaining trip to the goal.
        /// </summary>
        public abstract double Estimate();

        /// <summary>
        /// Gets all successors nodes from the current node and adds them to the successor list
        /// </summary>
        /// <param name="aSuccessors">List in which the successors will be added</param>
        public abstract void GetSuccessors(List<T> aSuccessors);
		
        #endregion

        #region Overridden Methods

        public override bool Equals(object obj)
        {
            return IsSameState((T)obj);
        }

        #endregion

        #region IComparable Members

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            var otherNode = (AStarNode<T>)obj;

            if (IsSameState((T)obj))
                return 0;

            return otherNode.TotalCost.CompareTo(TotalCost);
        }

        #endregion
    }
}