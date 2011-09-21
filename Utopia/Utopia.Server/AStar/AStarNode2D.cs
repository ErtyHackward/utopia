using System;
using System.Collections.Generic;

namespace Utopia.Server.AStar
{
    /// <summary>
    /// A node class for doing pathfinding on a 2-dimensional map
    /// </summary>
    public class AStarNode2D : AStarNode<AStarNode2D>
    {
        #region Properties

        /// <summary>
        /// The X-coordinate of the node
        /// </summary>
        public int X 
        {
            get 
            {
                return FX;
            }
        }
        private int FX;

        /// <summary>
        /// The Y-coordinate of the node
        /// </summary>
        public int Y
        {
            get
            {
                return FY;
            }
        }
        private int FY;

        #endregion
	
        #region Constructors

        /// <summary>
        /// Constructor for a node in a 2-dimensional map
        /// </summary>
        /// <param name="AParent">Parent of the node</param>
        /// <param name="AGoalNode">Goal node</param>
        /// <param name="ACost">Accumulative cost</param>
        /// <param name="AX">X-coordinate</param>
        /// <param name="AY">Y-coordinate</param>
        public AStarNode2D(AStarNode2D AParent, AStarNode2D AGoalNode, double ACost, int AX, int AY)
            : base(AParent, AGoalNode, ACost)
        {
            FX = AX;
            FY = AY;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a successor to a list if it is not impassible or the parent node
        /// </summary>
        /// <param name="ASuccessors">List of successors</param>
        /// <param name="AX">X-coordinate</param>
        /// <param name="AY">Y-coordinate</param>
        private void AddSuccessor(List<AStarNode2D> ASuccessors,int AX,int AY) 
        {
            int CurrentCost = MainClass.GetMap(AX,AY);
            if(CurrentCost == -1) 
            {
                return;
            }
            var newNode = new AStarNode2D(this,GoalNode,Cost + CurrentCost,AX,AY);
            if(newNode.IsSameState(Parent)) 
            {
                return;
            }
            ASuccessors.Add(newNode);
        }

        #endregion

        #region Overidden Methods

        /// <summary>
        /// Determines wheather the current node is the same state as the on passed.
        /// </summary>
        /// <param name="aNode">AStarNode to compare the current node to</param>
        /// <returns>Returns true if they are the same state</returns>
        public override bool IsSameState(AStarNode2D aNode)
        {
            if(aNode == null) 
            {
                return false;
            }
            return ((aNode.X == FX) && (aNode.Y == FY));
        }
		
        /// <summary>
        /// Calculates the estimated cost for the remaining trip to the goal.
        /// </summary>
        public override void Calculate()
        {
            if(GoalNode != null) 
            {
                double xd = FX - GoalNode.X;
                double yd = FY - GoalNode.Y;
                // "Euclidean distance" - Used when search can move at any angle.
                //GoalEstimate = Math.Sqrt((xd*xd) + (yd*yd));
                // "Manhattan Distance" - Used when search can only move vertically and 
                // horizontally.
                //GoalEstimate = Math.Abs(xd) + Math.Abs(yd); 
                // "Diagonal Distance" - Used when the search can move in 8 directions.
                GoalEstimate = Math.Max(Math.Abs(xd),Math.Abs(yd));
            }
            else
            {
                GoalEstimate = 0;
            }
        }

        /// <summary>
        /// Gets all successors nodes from the current node and adds them to the successor list
        /// </summary>
        /// <param name="aSuccessors">List in which the successors will be added</param>
        public override void GetSuccessors(List<AStarNode2D> aSuccessors)
        {
            aSuccessors.Clear();
            AddSuccessor(aSuccessors,FX-1,FY  );
            AddSuccessor(aSuccessors,FX-1,FY-1);
            AddSuccessor(aSuccessors,FX  ,FY-1);
            AddSuccessor(aSuccessors,FX+1,FY-1);
            AddSuccessor(aSuccessors,FX+1,FY  );
            AddSuccessor(aSuccessors,FX+1,FY+1);
            AddSuccessor(aSuccessors,FX  ,FY+1);
            AddSuccessor(aSuccessors,FX-1,FY+1);
        }	

        /// <summary>
        /// Prints information about the current node
        /// </summary>
        public override void PrintNodeInfo()
        {
            Console.WriteLine("X:\t{0}\tY:\t{1}\tCost:\t{2}\tEst:\t{3}\tTotal:\t{4}",FX,FY,Cost,GoalEstimate,TotalCost);
        }

        #endregion
    }
}