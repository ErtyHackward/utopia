using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3_Resources.Structs
{
    public struct RangeB
    {
        public byte Min;
        public byte Max;

        public RangeB(byte min, byte max)
        {
            Min = min;
            Max = max;
        }
    }
}
