using System.Collections.Generic;
using Utopia.Shared.Structs;

namespace Utopia.Server.AStar
{
    /// <summary>
    /// Defines a path between two 3d points
    /// </summary>
    public class Path3D
    {
        /// <summary>
        /// Start point of the path
        /// </summary>
        public Location3<int> Start { get; set; }

        /// <summary>
        /// Goal point of the path
        /// </summary>
        public Location3<int> Goal { get; set; }

        /// <summary>
        /// List of points from start to goal or null if path 
        /// </summary>
        public List<Location3<int>> Points { get; set; }

        public bool Exists
        {
            get { return Points != null; }
        }

        public override int GetHashCode()
        {
            return Start.X + (Start.Y << 5) + (Start.Z << 10) + (Goal.X << 15) + (Goal.Y << 20) + (Goal.Z << 25);
        }

        public override bool Equals(object obj)
        {
            if(obj == null || obj.GetType() != GetType())
                return false;

            var other = (Path3D)obj;

            return Start == other.Start && Goal == other.Goal;
        }
    }
}
