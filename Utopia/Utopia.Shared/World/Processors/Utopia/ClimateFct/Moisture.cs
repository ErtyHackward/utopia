using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise.ResultModifier;
using S33M3CoreComponents.Noise.Various;
using S33M3CoreComponents.Noise.DomainModifier;
using S33M3CoreComponents.Noise.Generator;

namespace Utopia.Shared.World.Processors.Utopia.ClimateFct
{
    public class Moisture: ITerrainGenerator
    {
        #region Private Variables
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public Moisture(int seed)
        {
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()            
        {
            //Create the Lowland base fractal with range from 0 to 1 values
            INoise Moisture_fractal = new FractalFbm(new Simplex(_seed), 2, 0.8, enuBaseNoiseRange.ZeroToOne);
            INoise Moisture_fractal_biased = new Gain(Moisture_fractal, 0.6);

            return Moisture_fractal_biased;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
