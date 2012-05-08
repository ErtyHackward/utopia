using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Various;
using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise.ResultModifier;
using S33M3CoreComponents.Noise.DomainModifier;
using S33M3CoreComponents.Noise.ResultCombiner;

namespace Utopia.Shared.World.Processors.Utopia.LandformFct
{
    public class Montain : ITerrainGenerator 
    {
        #region Private Variables
        private INoise _groundGradient;
        private Gradient _groundGradientTyped;
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public Montain(int seed, Gradient groundGradient)
            :this (seed, groundGradient, groundGradient)
        {
        }

        public Montain(int seed, Cache<Gradient> groundGradient)
            :this(seed, groundGradient, groundGradient.Source)
        {
        }

        private Montain(int seed, INoise groundGradient, Gradient groundGradientTyped)
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
            INoise plain_shape_fractal = new FractalFbm(new Simplex(_seed), 4, 3.5, enuBaseNoiseRange.ZeroToOne);
            //Rescale + offset the output result ==> Wil modify the Scope of output range value
            INoise adjustedGradient = new ScaleOffset(_groundGradient, 1.2, 0);

            Combiner noiseCombiner = new Combiner(Combiner.CombinerType.Add);
            noiseCombiner.Noises.Add(plain_shape_fractal);
            noiseCombiner.Noises.Add(adjustedGradient);

            INoise rescaledCombinedNoise = new ScaleOffset(noiseCombiner, 0.5, 0);

            return rescaledCombinedNoise;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
