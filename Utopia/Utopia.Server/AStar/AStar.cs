using System;
using System.Collections;
using System.Collections.Generic;

namespace Utopia.Server.AStar {
    /// <summary>
	/// Class for performing A* pathfinding
	/// </summary>
	public sealed class AStar<T> where T : AStarNode<T>
	{
		#region Private Fields

        private SortedSet<T> items;
		private T _fStartNode;
		private T _fGoalNode;
		private readonly SortedSet<T> _fOpenList;
        private readonly SortedSet<T> _fClosedList;
        private readonly List<T> _fSuccessors;

		#endregion

		#region Properties

	    /// <summary>
	    /// Holds the solution after pathfinding is done. <see>FindPath()</see>
	    /// </summary>
        public List<T> Solution { get; private set; }

	    #endregion
		
		#region Constructors

		public AStar(IComparer<T> comparer)
		{
            _fOpenList = new SortedSet<T>(comparer);
            _fClosedList = new SortedSet<T>(comparer);
			_fSuccessors = new List<T>();
            Solution = new List<T>();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Prints all the nodes in a list
		/// </summary>
		/// <param name="aNodeList">List to print</param>
		private void PrintNodeList(object aNodeList)
		{
			Console.WriteLine("Node list:");
			foreach(AStarNode<T> n in (aNodeList as IEnumerable)) 
			{
				n.PrintNodeInfo();
			}
			Console.WriteLine("=====");
		}

		#endregion
		
		#region Public Methods

		/// <summary>
		/// Finds the shortest path from the start node to the goal node
		/// </summary>
		/// <param name="aStartNode">Start node</param>
		/// <param name="aGoalNode">Goal node</param>
		public void FindPath(T aStartNode,T aGoalNode)
		{
			_fStartNode = aStartNode;
			_fGoalNode = aGoalNode;

			_fOpenList.Add(_fStartNode);
			while(_fOpenList.Count > 0) 
			{
				// Get the node with the lowest TotalCost
				var nodeCurrent = _fOpenList.Min;

				// If the node is the goal copy the path to the solution array
				if(nodeCurrent.IsGoal()) {
					while(nodeCurrent != null) {
                        Solution.Insert(0, nodeCurrent);
						nodeCurrent = nodeCurrent.Parent;
					}
					break;					
				}

				// Get successors to the current node
				nodeCurrent.GetSuccessors(_fSuccessors);
				foreach(var nodeSuccessor in _fSuccessors) 
				{
					// Test if the currect successor node is on the open list, if it is and
					// the TotalCost is higher, we will throw away the current successor.
					T nodeOpen = null;
					if(_fOpenList.Contains(nodeSuccessor))
                        nodeOpen = nodeSuccessor; //nodeOpen = (T)_fOpenList[_fOpenList.IndexOf(nodeSuccessor)];
					if((nodeOpen != null) && (nodeSuccessor.TotalCost > nodeOpen.TotalCost)) 
						continue;
					
					// Test if the currect successor node is on the closed list, if it is and
					// the TotalCost is higher, we will throw away the current successor.
					T nodeClosed = null;
					if(_fClosedList.Contains(nodeSuccessor))
                        nodeClosed = nodeSuccessor;
					if((nodeClosed != null) && (nodeSuccessor.TotalCost > nodeClosed.TotalCost)) 
						continue;
					
					// Remove the old successor from the open list
					_fOpenList.Remove(nodeOpen);

					// Remove the old successor from the closed list
					_fClosedList.Remove(nodeClosed);
					
					// Add the current successor to the open list
					_fOpenList.Add(nodeSuccessor);
				}
				// Add the current node to the closed list
				_fClosedList.Add(nodeCurrent);
			}
		}
		
		#endregion
	}
}