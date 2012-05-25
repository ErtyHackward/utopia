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
        #endregion

        #region Public Properties
        #endregion

        public PlainCtrl(int seed)
        {
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()
        {
            INoise AnomaliesZonesFractal = new FractalFbm(new Perlin(_seed), 3, 2.5, enuBaseNoiseRange.ZeroToOne);
            INoise AnomaliesZonesFractal_y_scale = new NoiseAccess(AnomaliesZonesFractal, NoiseAccess.enuDimUsage.Noise2D, true);
            INoise AnomaliesZonesFractal_Bias = new Gain(AnomaliesZonesFractal_y_scale, 0.6);

            return AnomaliesZonesFractal_Bias;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
