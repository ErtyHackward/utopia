using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Resources.Structs.Interpolations
{
    public abstract class InterpBase<T> : IInterpolable
    {
        public T Value;
        protected T PrevValue;
        public T InterpolatedValue;

        public void BackUpValue()
        {
            PrevValue = Value;
        }

        public InterpBase(T initValue)
        {
            Value = initValue;
            PrevValue = initValue;
            InterpolatedValue = initValue;
        }

        public abstract void Interpolate(float interpolationLd, double interpolationHd);
    }
}
