using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3_Resources.Structs
{
    public struct Range
    {
        public float Min;
        public float Max;

        public Range(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
