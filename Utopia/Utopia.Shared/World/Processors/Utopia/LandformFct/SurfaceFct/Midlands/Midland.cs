﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.ResultModifier;
using S33M3CoreComponents.Noise.DomainModifier;
using S33M3CoreComponents.Noise.Various;

namespace Utopia.Shared.World.Processors.Utopia.LandformFct
{
    public class Midland : ITerrainGenerator
    {
        #region Private Variables
        private INoise _groundGradient;
        private Gradient _groundGradientTyped;
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public Midland(int seed, Gradient groundGradient)
            : this(seed, groundGradient, groundGradient)
        {
        }

        public Midland(int seed, Cache<Gradient> groundGradient)
            : this(seed, groundGradient, groundGradient.Source)
        {
        }

        private Midland(int seed, INoise groundGradient, Gradient groundGradientTyped)
        {
            _groundGradient = groundGradient;
            _groundGradientTyped = groundGradientTyped;
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()
        {
            //REM
            //The various parameters value here are scaled for a gradient being feed by 0 to 1 input value.
            //When this gradient is configured to recevied other value range then some parameters needs to be rescaled
            //That's the reason for using this _groundGradientTyped.AdjustY value
            //This way no matter the the Gradient Range, the values impacting it will be rescaled.

            //Create the Lowland base fractal with range from 0 to 1 values
            INoise midLand_shape_fractal = new FractalRidgedMulti(new Simplex(_seed), 4, 0.7, enuBaseNoiseRange.ZeroToOne);
            //Rescale + offset the output result ==> Wil modify the Scope of output range value
            INoise midLand_scale = new ScaleOffset(midLand_shape_fractal, 0.2 * _groundGradientTyped.AdjustY, 0 * _groundGradientTyped.AdjustY);
            //Remove Y value from impacting the result (Fixed to 0), the value output range will not be changed, but the influence of the Y will be removed

            //Force the Fractal to be used as 2D Noise, I don't need to 3th dimension
            INoise midLand_y_scale = new NoiseAccess(midLand_scale, NoiseAccess.enuDimUsage.Noise2D, true);

            //Offset the ground_gradient ( = create turbulance) to the Y scale of the gradient. input value 
            INoise _groundGradient_biased = new Bias(_groundGradient, 0.60);
            INoise midLand_terrain = new Turbulence(_groundGradient_biased, 0, midLand_y_scale);

            return midLand_terrain;
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
