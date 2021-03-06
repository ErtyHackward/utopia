﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.ResultModifier;
using S33M3CoreComponents.Noise.Various;

namespace Utopia.Shared.World.Processors.Utopia.ClimateFct
{
    public class Temperature
    {
        #region Private Variables
        private int _seed;
        private int _octave;
        private double _freq;
        #endregion

        #region Public Properties
        #endregion

        public Temperature(int seed, int octave = 2, double frequency = 1)
        {
            _octave = octave;
            _freq = frequency;
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()            
        {
            //Create the Lowland base fractal with range from 0 to 1 values
            INoise Temperature_fractal = new FractalFbm(new Simplex(_seed), _octave, _freq, enuBaseNoiseRange.ZeroToOne);
            INoise ClampedValue = new Clamp(Temperature_fractal, 0, 1);
            INoise Temperature_fractal_biased = new Gain(ClampedValue, 0.71);
            INoise Temperature_fractal_Offset = new ScaleOffset(Temperature_fractal_biased, 1, 0.1);
            INoise ClampedValue2 = new Clamp(Temperature_fractal_Offset, 0, 1);

            return ClampedValue2;

            //INoise TempOffset = new ScaleOffset(new Voronoi2(_seed, _freq, 0.6, new ScaleOffset(new FractalFbm(new Perlin(54321), 3, 2), 0.4, 0.0)), 1.0, 1.0); // + 1
            //INoise TempScale = new ScaleOffset(TempOffset, 0.5, 0.0); // / 2
            //INoise ClampedValue = new Clamp(TempScale, 0, 1);
            //return ClampedValue;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
