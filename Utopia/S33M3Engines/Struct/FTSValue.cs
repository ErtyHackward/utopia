using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.Struct
{
    public class FTSValue<T>
    {
        public T Value;
        public T ValuePrev;
        public T ValueInterp;

        public void BackUpValue()
        {
            ValuePrev = Value;
        }
    }
}
