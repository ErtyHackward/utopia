﻿using System;
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
            INoise Moisture_fractal = new FractalFbm(new Simplex(_seed), 2, 1, enuBaseNoiseRange.ZeroToOne);
            INoise ClampedValue = new Clamp(Moisture_fractal, 0, 1);
            INoise Moisture_fractal_biased = new Gain(ClampedValue, 0.6);
            INoise Moisture_fractal_Offset = new ScaleOffset(Moisture_fractal_biased, 1, 0.1);
            INoise ClampedValue2 = new Clamp(Moisture_fractal_Offset, 0, 1);

            return ClampedValue2;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
