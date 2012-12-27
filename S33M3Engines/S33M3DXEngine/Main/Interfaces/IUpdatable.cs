using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3DXEngine.Main.Interfaces
{
    public interface IUpdatable : IDisposable
    {
        /// <summary>
        /// Performs game logic update. Always called with fixed time step
        /// </summary>
        /// <param name="timeSpend">Gives an amount of seconds passed since last call [runtime constant]</param>
        void FTSUpdate(GameTime timeSpend);

        /// <summary>
        /// Variable Time Step update, is call once before each draw.
        /// </summary>
        /// <param name="interpolationHd">Value in range [0;1] that indicates factor of interpolation to create interpolation for FTSUpdate</param>
        /// <param name="interpolationLd">Value in range [0;1] that indicates factor of interpolation to create interpolation for FTSUpdate</param>
        /// <param name="elapsedTime">The Elapsed time between 2 call to the interpolation method</param>
        void VTSUpdate(double interpolationHd, float interpolationLd, long elapsedTime);
    }
}
