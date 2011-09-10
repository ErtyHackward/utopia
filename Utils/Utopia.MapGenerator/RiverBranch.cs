using System.Collections.Generic;

namespace Utopia.MapGenerator
{
    public class RiverBranch
    {
        public Edge Edge { get; set; }

        public bool Final
        {
            get { return Branches.Count == 0; }
        }

        public RiverBranch ParentBranch { get; set; }

        public List<RiverBranch> Branches = new List<RiverBranch>();
    }
}