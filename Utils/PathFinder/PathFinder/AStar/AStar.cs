using System;
using System.Collections;
using System.Collections.Generic;
using PathFinder.AStar;

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

        public event EventHandler NextNode;

        public event EventHandler<AStarDebugEventArgs<T>> NodeSelected;

		#endregion

		#region Properties

	    /// <summary>
	    /// Holds the solution after pathfinding is done. <see>FindPath()</see>
	    /// </summary>
        public List<T> Solution { get; private set; }

        public IEnumerable<T> EnumerateOpen()
        {
            foreach (var item in _fOpenList)
            {
                yield return item;
            }
        }

        public IEnumerable<T> EnumerateClosed()
        {
            foreach (var item in _fClosedList)
            {
                yield return item;
            }
        }

        #endregion
		
		#region Constructors

		public AStar()
		{
            _fOpenList = new SortedSet<T>();
            _fClosedList = new SortedSet<T>();
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
                if (NextNode != null)
                    NextNode(this, null);

				// Get the node with the lowest TotalCost
				var nodeCurrent = _fOpenList.Min;
                _fOpenList.Remove(nodeCurrent);

                if (NodeSelected != null)
                    NodeSelected(this, new AStarDebugEventArgs<T>() { Node = nodeCurrent });

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
                    if (_fOpenList.Contains(nodeSuccessor))
                    {
                        continue;
                    }
					
					// Test if the currect successor node is on the closed list, if it is and
					// the TotalCost is higher, we will throw away the current successor.
					T nodeClosed = null;
                    if (_fClosedList.Contains(nodeSuccessor))
                    {
                        continue;
                    }


				    // Remove the old successor from the open list
                    //_fOpenList.Remove(nodeSuccessor);

					// Remove the old successor from the closed list
                    //_fClosedList.Remove(nodeSuccessor);
					
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