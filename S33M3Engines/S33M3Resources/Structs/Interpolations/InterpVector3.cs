using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace S33M3Resources.Structs.Interpolations
{
    public class InterpVector3 : InterpBase<Vector3>
    {
        public InterpVector3(Vector3 initialValue)
            :base(initialValue)
        {
        }

        public override void Interpolate(float interpolationLd, double interpolationHd)
        {
            Vector3.Lerp(ref PrevValue, ref Value, interpolationLd, out InterpolatedValue);
        }
    }
}
