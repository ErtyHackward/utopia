using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3_Resources.Structs
{
    public struct RangeI
    {
        public int Min;
        public int Max;

        public RangeI(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }
}
