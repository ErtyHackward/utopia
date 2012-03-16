using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Resources.Structs
{
    public class NetworkValue<T> where T : struct
    {
        public T Value;
        public T Interpolated;

        public T DeltaValue;
        public double Distance;
    }
}
