using System.Collections.Generic;

namespace Utopia.Shared.Server.AStar 
{

    /// <summary>
	/// Class for performing generic A* algorithm (algorithm lies in the generic class)
	/// </summary>
	public sealed class AStar<T> where T : AStarNode<T>
	{
		#region Private Fields

		private T _fStartNode;

        private readonly AStarList<T> _fOpenList = new AStarList<T>();
        private readonly List<T> _fSuccessors = new List<T>();

        private readonly Dictionary<T, T> _fClosedList = new Dictionary<T, T>();

        /// <summary>
        /// Maximum amount of iterations before say "there is no way"
        /// </summary>
        public int IterationsLimit { get; set; }

        private int _iterations;

        #endregion

		#region Properties

	    /// <summary>
	    /// Holds the solution after pathfinding is done. <see>FindPath()</see>
	    /// </summary>
        public List<T> Solution { get; private set; }

        /// <summary>
        /// Gets total amount of iterations performed to find a path
        /// </summary>
        public int Iterations
        {
            get { return _iterations; }
        }

        #endregion
		
		#region Constructors

		public AStar()
		{
            IterationsLimit = 5000;
		}

		#endregion
		
		#region Public Methods

        /// <summary>
        /// Finds the shortest path from the start node to the goal node
        /// </summary>
        /// <param name="aStartNode">Start node</param>
        public void FindPath(T aStartNode)
		{
            Solution = null;
            _fClosedList.Clear();
            _fOpenList.Clear();
            _iterations = 0;
			_fStartNode = aStartNode;

			_fOpenList.Add(_fStartNode);

			while (_fOpenList.Count > 0) 
			{
                if (_iterations++ > IterationsLimit)
                    break;

				// Get the node with the lowest TotalCost
                var nodeCurrent = _fOpenList.Pop();

				// If the node is the goal copy the path to the solution array
                if (nodeCurrent.IsGoal())
                {
                    Solution = new List<T>();
                    while (nodeCurrent != null) 
                    {
                        Solution.Add(nodeCurrent);
						nodeCurrent = nodeCurrent.Parent;
					}
                    Solution.Reverse();
					break;					
				}

				// Get successors to the current node
                _fSuccessors.Clear();
                nodeCurrent.GetSuccessors(_fSuccessors);
				foreach (var nodeSuccessor in _fSuccessors) 
				{
					// Test if the current successor node is on the open list, if it is and
					// the TotalCost is higher, we will throw away the current successor.
					T nodeOpen;
                    if (_fOpenList.TryGetValue(nodeSuccessor, out nodeOpen))
                    {
                        if (nodeOpen.Cost > nodeSuccessor.Cost)
                            _fOpenList[nodeSuccessor] = nodeSuccessor;
                        continue;
                    }
					
					// Test if the currect successor node is on the closed list, if it is and
					// the TotalCost is higher, we will throw away the current successor.
					T nodeClosed;
                    if (_fClosedList.TryGetValue(nodeSuccessor, out nodeClosed))
                    {
                        if (nodeClosed.Cost > nodeSuccessor.Cost)
                            _fClosedList[nodeSuccessor] = nodeSuccessor;
                        continue;
                    }
					
					// Add the current successor to the open list
					_fOpenList.Add(nodeSuccessor);
				}
				// Add the current node to the closed list
				_fClosedList.Add(nodeCurrent, nodeCurrent);
			}
		}
		
		#endregion

        
    }
}