using System.Collections.Generic;

namespace Utopia.Shared.World.PlanGenerator
{
    public class CornerHeightComparer : IComparer<Corner>
    {
        public int Compare(Corner x, Corner y)
        {
            return -1 * x.Elevation.CompareTo(y.Elevation);
        }
    }
}