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
    public class AnomaliesType : ITerrainGenerator
    {
        #region Private Variables
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public AnomaliesType(int seed)
        {
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()
        {
            INoise AnomaliesTypeFractal = new FractalFbm(new Simplex(_seed), 3, 2, enuBaseNoiseRange.ZeroToOne);
            INoise AnomaliesTypeFractal_y_scale = new ScaleDomain(AnomaliesTypeFractal, 1.0, 0.0, 1.0);

            return AnomaliesTypeFractal_y_scale;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
