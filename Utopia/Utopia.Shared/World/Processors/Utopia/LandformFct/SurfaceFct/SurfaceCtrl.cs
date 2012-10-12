using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Fractal;
using Ninject.Activation.Caching;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.Various;
using S33M3CoreComponents.Noise.DomainModifier;

namespace Utopia.Shared.World.Processors.Utopia.LandformFct
{
    public class SurfaceCtrl : ITerrainGenerator
    {
        #region Private Variables
        private int _seed;
        private int _octaves;
        private double _frequency;
        private double _bias;
        #endregion

        #region Public Properties
        #endregion

        public SurfaceCtrl(int seed, int octave = 2, double frequency = 1.5, double bias = 0.6)
        {
            _seed = seed;
            _octaves = octave;
            _frequency = frequency;
            _bias = bias;
        }

        #region Public Methods
        public INoise GetLandFormFct()
        {
            INoise SurfaceBiomeFractal = new FractalHybridMulti(new Perlin(_seed), _octaves, _frequency, enuBaseNoiseRange.ZeroToOne);
            INoise SurfaceBiomeFractal_as2DNoise = new NoiseAccess(SurfaceBiomeFractal, NoiseAccess.enuDimUsage.Noise2D, true);

            return SurfaceBiomeFractal_as2DNoise;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
