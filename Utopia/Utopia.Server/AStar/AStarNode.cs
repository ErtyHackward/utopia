using System;
using System.Collections.Generic;

namespace Utopia.Server.AStar
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
            set
            {
                _fGoalEstimate = value;
            }
            get 
            {
                Calculate();
                return(_fGoalEstimate);
            }
        }
        private double _fGoalEstimate;

        /// <summary>
        /// The cost plus the estimated cost to the goal from here.
        /// </summary>
        public double TotalCost
        {
            get 
            {
                return(Cost + GoalEstimate);
            }
        }

        /// <summary>
        /// The goal node.
        /// </summary>
        public T GoalNode 
        {
            set 
            {
                _fGoalNode = value;
                Calculate();
            }
            get
            {
                return _fGoalNode;
            }
        }
        private T _fGoalNode;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="aParent">The node's parent</param>
        /// <param name="aGoalNode">The goal node</param>
        /// <param name="aCost">The accumulative cost until now</param>
        public AStarNode(T aParent,T aGoalNode,double aCost)
        {
            Parent = aParent;
            Cost = aCost;
            GoalNode = aGoalNode;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Determines wheather the current node is the goal.
        /// </summary>
        /// <returns>Returns true if current node is the goal</returns>
        public bool IsGoal()
        {
            return IsSameState(_fGoalNode);
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Determines wheather the current node is the same state as the on passed.
        /// </summary>
        /// <param name="aNode">AStarNode to compare the current node to</param>
        /// <returns>Returns true if they are the same state</returns>
        public virtual bool IsSameState(T aNode)
        {
            return false;
        }

        /// <summary>
        /// Calculates the estimated cost for the remaining trip to the goal.
        /// </summary>
        public virtual void Calculate()
        {
            _fGoalEstimate = 0.0f;
        }

        /// <summary>
        /// Gets all successors nodes from the current node and adds them to the successor list
        /// </summary>
        /// <param name="aSuccessors">List in which the successors will be added</param>
        public abstract void GetSuccessors(List<T> aSuccessors);
		

        /// <summary>
        /// Prints information about the current node
        /// </summary>
        public virtual void PrintNodeInfo()
        {
        }

        #endregion

        #region Overridden Methods

        public override bool Equals(object obj)
        {
            return IsSameState((T)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return(-TotalCost.CompareTo(((AStarNode<T>)obj).TotalCost));
        }

        #endregion
    }
}