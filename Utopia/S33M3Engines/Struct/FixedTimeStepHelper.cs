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

        public T ActualValue
        {
            get
            {
                if (D3DEngine.FIXED_TIMESTEP_ENABLED)
                {
                    return ValueInterp;
                }
                else
                {
                    return Value;
                }
            }
        }

        public void BackUpValue()
        {
            ValuePrev = Value;
        }
    }
}
