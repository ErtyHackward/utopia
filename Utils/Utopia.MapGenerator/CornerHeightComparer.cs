using System.Collections.Generic;

namespace Utopia.MapGenerator
{
    public class CornerHeightComparer : IComparer<Corner>
    {
        public int Compare(Corner x, Corner y)
        {
            return -1 * x.Elevation.CompareTo(y.Elevation);
        }
    }
}