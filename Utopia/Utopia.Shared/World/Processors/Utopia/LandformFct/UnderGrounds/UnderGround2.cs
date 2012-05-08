using System;
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
    public class UnderGround2 : ITerrainGenerator
    {
        #region Private Variables
        private INoise _groundGradient;
        private Gradient _groundGradientTyped;
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public UnderGround2(int seed, Gradient groundGradient)
            : this(seed, groundGradient, groundGradient)
        {
        }

        public UnderGround2(int seed, Cache<Gradient> groundGradient)
            : this(seed, groundGradient, groundGradient.Source)
        {
        }

        private UnderGround2(int seed, INoise groundGradient, Gradient groundGradientTyped)
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
            INoise shape1_fractal = new FractalRidgedMulti(new Perlin(_seed), 1, 2);

            INoise shape1_base = new Select(0, 1, shape1_fractal, 0.7, 0.0);

            INoise shape2_fractal = new FractalRidgedMulti(new Perlin(_seed + 12345), 1, 2);

            INoise shape2_base = new Select(0, 1, shape2_fractal, 0.7, 0.0);

            Combiner ShapeMult = new Combiner(Combiner.CombinerType.Multiply);
            ShapeMult.Noises.Add(shape1_base);
            ShapeMult.Noises.Add(shape2_base);

            INoise turbX_fractal = new FractalFbm(new Perlin(_seed + 1), 3, 3);
            INoise turbY_fractal = new FractalFbm(new Perlin(_seed + 2), 3, 3);
            INoise turbZ_fractal = new FractalFbm(new Perlin(_seed + 3), 3, 3);

            INoise CaveTurb = new Turbulence(ShapeMult, turbX_fractal, turbY_fractal, turbZ_fractal);

            INoise invertedCave = new ScaleOffset(CaveTurb, -1, 1);

            return invertedCave;
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
