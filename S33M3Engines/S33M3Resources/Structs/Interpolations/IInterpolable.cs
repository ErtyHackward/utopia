using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Resources.Structs.Interpolations
{
    public interface IInterpolable
    {
        void BackUpValue();
        void Interpolate(float interpolationLd, double interpolationHd);
    }
}
