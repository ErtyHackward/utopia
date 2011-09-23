using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathFinder.AStar;

namespace Utopia.Server.AStar {
    /// <summary>
	/// Class for performing A* pathfinding
	/// </summary>
	public sealed class AStar<T> where T : AStarNode<T>
	{
		#region Private Fields

		private T _fStartNode;
        private readonly AStarList<T> _fOpenList = new AStarList<T>();
        //private readonly AStarList<T> _fClosedList;
        private readonly List<T> _fSuccessors = new List<T>();

        private readonly Dictionary<T, T> _fClosedList = new Dictionary<T,T>();
        private readonly Dictionary<T, T> _fOpenSet = new Dictionary<T, T>();

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
                yield return item.Key;
            }
        }

        #endregion
		
		#region Constructors

		public AStar()
		{
            Solution = new List<T>();           
		}

		#endregion
		
		#region Public Methods

		/// <summary>
		/// Finds the shortest path from the start node to the goal node
		/// </summary>
		/// <param name="aStartNode">Start node</param>
		public void FindPath(T aStartNode)
		{
            Solution.Clear();
            _fClosedList.Clear();
            _fOpenList.Clear();
            _fOpenSet.Clear();

			_fStartNode = aStartNode;

			_fOpenList.Add(_fStartNode);

			while(_fOpenList.Count > 0) 
			{
                if (NextNode != null)
                    NextNode(this, null);

				// Get the node with the lowest TotalCost
                var nodeCurrent = _fOpenList.Pop();
                _fOpenSet.Remove(nodeCurrent);

                if (NodeSelected != null)
                    NodeSelected(this, new AStarDebugEventArgs<T> { Node = nodeCurrent });

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
					T nodeOpen;
                    if (_fOpenSet.TryGetValue(nodeSuccessor, out nodeOpen))
                    {
                        if (nodeOpen.Cost > nodeSuccessor.Cost)
                            _fOpenSet[nodeSuccessor] = nodeSuccessor;
                        continue;
                    }
					
					// Test if the currect successor node is on the closed list, if it is and
					// the TotalCost is higher, we will throw away the current successor.
					T nodeClosed;
                    if (_fClosedList.TryGetValue(nodeSuccessor, out nodeClosed)) //, n => n.IsSameState(nodeSuccessor)))
                    {
                        if (nodeClosed.Cost > nodeSuccessor.Cost)
                            _fOpenSet[nodeSuccessor] = nodeSuccessor;
                        continue;
                    }
					
					// Add the current successor to the open list
					_fOpenList.Add(nodeSuccessor);
                    _fOpenSet.Add(nodeSuccessor, nodeSuccessor);
				}
				// Add the current node to the closed list
				_fClosedList.Add(nodeCurrent, nodeCurrent);
			}
		}
		
		#endregion
	}
}