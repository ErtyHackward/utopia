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
        #endregion

        #region Public Properties
        #endregion

        public SurfaceCtrl(int seed)
        {
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()
        {
            INoise SurfaceBiomeFractal = new FractalHybridMulti(new Perlin(_seed), 2, 1.5, enuBaseNoiseRange.ZeroToOne);
            INoise SurfaceBiomeFractal_y_scale = new NoiseAccess(SurfaceBiomeFractal, NoiseAccess.enuDimUsage.Noise2D, true);

            return SurfaceBiomeFractal_y_scale;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
