using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3_Resources.Structs
{
    public struct RangeD
    {
        public double Min;
        public double Max;

        public RangeD(double min, double max)
        {
            Min = min;
            Max = max;
        }
    }
}
