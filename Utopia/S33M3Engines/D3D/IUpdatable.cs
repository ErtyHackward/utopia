using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.D3D
{
    public interface IUpdatable : IDisposable
    {
        /// <summary>
        /// Performs game logic update. Always called with fixed time step
        /// </summary>
        /// <param name="timeSpent">Gives an amount of seconds passed since last call [runtime constant]</param>
        void Update(ref GameTime timeSpent);

        /// <summary>
        /// Performs gamecomponents interpolation. This method called right before each Draw call.
        /// Only visual interpolation should be done here.
        /// </summary>
        /// <param name="interpolationHd">Value in range [0;1] that indicates factor of interpolation between Update calls</param>
        /// <param name="interpolationLd">Value in range [0;1] that indicates factor of interpolation between Update calls</param>
        /// <param name="timePassed">Time passed since last method call [milliseconds]</param>
        void Interpolation(ref double interpolationHd, ref float interpolationLd, ref long timePassed);
    }
}
