using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3_DXEngine.Main.Interfaces
{
    public interface IUpdatable : IDisposable
    {
        /// <summary>
        /// Performs game logic update. Always called with fixed time step
        /// </summary>
        /// <param name="timeSpend">Gives an amount of seconds passed since last call [runtime constant]</param>
        void Update(GameTime timeSpend);

        /// <summary>
        /// Performs gamecomponents interpolation. This method called right before each Draw call.
        /// Only visual interpolation should be done here.
        /// </summary>
        /// <param name="interpolationHd">Value in range [0;1] that indicates factor of interpolation between Update calls</param>
        /// <param name="interpolationLd">Value in range [0;1] that indicates factor of interpolation between Update calls</param>
        /// <param name="elapsedTime">The Elapsed time between 2 call to the interpolation method</param>
        void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime);
    }
}
