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
    public class UnderGround : ITerrainGenerator
    {
        #region Private Variables
        private INoise _mainLandscape;
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public UnderGround(int seed, INoise mainLandscape)
        {
            _seed = seed;
            _mainLandscape = mainLandscape;
        }

        #region Public Methods
        public INoise GetLandFormFct()
        {
            //REM
            //The various parameters value here are scaled for a gradient being feed by 0 to 1 input value.
            //When this gradient is configured to recevied other value range then some parameters needs to be rescaled
            //That's the reason for using this _groundGradientTyped.AdjustY value
            //This way no matter the the Gradient Range, the values impacting it will be rescaled.

            INoise shape1_fractal = new FractalRidgedMulti(new Perlin(_seed), 1, 1.2);

            INoise shape1_base = new Select(0, shape1_fractal, shape1_fractal, 0.75, 0.0);

            INoise shape2_fractal = new FractalRidgedMulti(new Perlin(_seed + 12345), 1, 1.3);

            INoise shape2_base = new Select(0, shape2_fractal, shape2_fractal, 0.75, 0.0);

            Combiner ShapeMult = new Combiner(Combiner.CombinerType.Add);
            ShapeMult.Noises.Add(shape1_base);
            ShapeMult.Noises.Add(shape2_base);
            INoise rescaledShapeMult = new ScaleOffset(ShapeMult, 0.6, 0);
            INoise clamping_base = new Select(0, rescaledShapeMult, ShapeMult, 0.14, 0.0);

            INoise turbX_fractal = new FractalFbm(new Perlin(_seed + 1), 3, 3);
            INoise turbY_fractal = new FractalFbm(new Perlin(_seed + 2), 3, 3);
            INoise turbZ_fractal = new FractalFbm(new Perlin(_seed + 3), 3, 3);

            INoise CaveTurb = new Turbulence(clamping_base, turbX_fractal, turbY_fractal, turbZ_fractal);

            //INoise landscape = new Bias(_mainLandscape, 0.45);
            Combiner underground_Attenuated = new Combiner(Combiner.CombinerType.Multiply);
            underground_Attenuated.Noises.Add(CaveTurb);
            underground_Attenuated.Noises.Add(_mainLandscape);

            return underground_Attenuated;
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
