using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.D3D
{
    public interface IUpdatable : IDisposable
    {
        void Update(ref GameTime timeSpent);
        void Interpolation(ref double interpolationHd, ref float interpolationLd);
    }
}
