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
using S33M3CoreComponents.Noise.NoiseResultCombiner;
using S33M3CoreComponents.Noise.ResultCombiner;

namespace Utopia.Shared.World.Processors.Utopia.LandformFct
{
    public class UnderGround : ITerrainGenerator
    {
        #region Private Variables
        private INoise _groundGradient;
        private Gradient _groundGradientTyped;
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public UnderGround(int seed, INoise groundGradient)
            : this(seed, groundGradient, null)
        {
        }

        public UnderGround(int seed, Cache<Gradient> groundGradient)
            : this(seed, groundGradient, groundGradient.Source)
        {
        }

        private UnderGround(int seed, INoise groundGradient, Gradient groundGradientTyped)
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
            INoise underground_fractal1 = new FractalRidgedMulti(new Simplex(_seed), 1, 2);
            INoise underground_fractal2 = new FractalRidgedMulti(new Simplex(_seed + 12395), 1, 2);

            INoise inverted_fractal1 = new Invert(underground_fractal1);
            INoise inverted_fractal2 = new Invert(underground_fractal2);

            Combiner fractal = new Combiner(Combiner.CombinerType.Multiply);
            fractal.Noises.Add(inverted_fractal1);
            fractal.Noises.Add(inverted_fractal2);

            INoise gradientBias = new Bias(_groundGradient, 0.25);

            Combiner underground_Attenuated = new Combiner(Combiner.CombinerType.Multiply);
            underground_Attenuated.Noises.Add(fractal);
            underground_Attenuated.Noises.Add(gradientBias);

            INoise underground_select = new Select(0, 1, underground_Attenuated, -0.4);

            INoise fract_perturb = new FractalFbm(new Simplex(_seed + 12), 6, 3);

            INoise scaled_perturb = new ScaleOffset(fract_perturb, 0.2, 0);

            INoise perturbed_fractal = new Turbulence(underground_select, scaled_perturb, 0);


            return perturbed_fractal;
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
