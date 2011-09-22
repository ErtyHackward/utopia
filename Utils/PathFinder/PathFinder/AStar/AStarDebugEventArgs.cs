using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathFinder.AStar
{
    public class AStarDebugEventArgs<T> : EventArgs
    {
        public T Node { get; set; }
    }
}
