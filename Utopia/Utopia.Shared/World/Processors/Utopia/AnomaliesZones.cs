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

namespace Utopia.Shared.World.Processors.Utopia
{
    public class AnomaliesZones : ITerrainGenerator
    {
        #region Private Variables
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public AnomaliesZones(int seed)
        {
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()
        {
            INoise AnomaliesZonesFractal = new FractalFbm(new Perlin(_seed), 3, 2, enuBaseNoiseRange.ZeroToOne);
            INoise AnomaliesZonesFractal_y_scale = new ScaleDomain(AnomaliesZonesFractal, 1.0, 0.0, 1.0);

            return AnomaliesZonesFractal_y_scale;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
