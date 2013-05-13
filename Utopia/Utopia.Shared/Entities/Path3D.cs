using System.Collections.Generic;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Defines a path between two 3d points
    /// </summary>
    public class Path3D
    {
        /// <summary>
        /// Start point of the path
        /// </summary>
        public Vector3I Start { get; set; }

        /// <summary>
        /// Goal point of the path
        /// </summary>
        public Vector3I Goal { get; set; }

        #region Debug

        /// <summary>
        /// Gets time that was taken to create this path (ms)
        /// </summary>
        public double PathFindTime { get; set; }

        /// <summary>
        /// Gets amount of iterations used to calculate the path
        /// </summary>
        public int IterationsPerformed { get; set; }

        #endregion

        /// <summary>
        /// List of points from start to goal or null if path 
        /// </summary>
        public List<Vector3I> Points { get; set; }

        /// <summary>
        /// Indicates if path exists
        /// </summary>
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
