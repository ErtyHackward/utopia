using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.ResultModifier;

namespace Utopia.Shared.World.Processors.Utopia.ClimateFct
{
    public class Temperature
    {
        #region Private Variables
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public Temperature(int seed)
        {
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()            
        {
            //Create the Lowland base fractal with range from 0 to 1 values
            INoise Temperature_fractal = new FractalFbm(new Simplex(_seed), 2, 1.4, enuBaseNoiseRange.ZeroToOne);
            INoise ClampedValue = new Clamp(Temperature_fractal, 0, 1);
            INoise Temperature_fractal_Offset = new ScaleOffset(ClampedValue, 1, 0.1);
            INoise Temperature_fractal_biased = new Gain(Temperature_fractal_Offset, 0.71);
            INoise ClampedValue2 = new Clamp(Temperature_fractal_biased, 0, 1);

            return ClampedValue2;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
