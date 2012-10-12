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
using S33M3CoreComponents.Noise.ResultModifier;

namespace Utopia.Shared.World.Processors.Utopia.LandformFct
{
    public class PlainCtrl : ITerrainGenerator
    {
        #region Private Variables
        private int _seed;
        private int _octaves;
        private double _frequency;
        private double _bias;
        #endregion

        #region Public Properties
        #endregion

        public PlainCtrl(int seed, int octave = 3, double frequency = 2.5, double bias = 0.6)
        {
            _seed = seed;
            _octaves = octave;
            _frequency = frequency;
            _bias = bias;
        }

        #region Public Methods
        public INoise GetLandFormFct()
        {
            INoise AnomaliesZonesFractal = new FractalFbm(new Perlin(_seed), _octaves, _frequency, enuBaseNoiseRange.ZeroToOne);
            INoise AnomaliesZonesFractal_y_scale = new NoiseAccess(AnomaliesZonesFractal, NoiseAccess.enuDimUsage.Noise2D, true);
            INoise AnomaliesZonesFractal_Bias = new Gain(AnomaliesZonesFractal_y_scale, _bias);

            return AnomaliesZonesFractal_Bias;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
