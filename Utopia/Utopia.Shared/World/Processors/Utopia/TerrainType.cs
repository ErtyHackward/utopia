using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Fractal;
using Ninject.Activation.Caching;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.Various;

namespace Utopia.Shared.World.Processors.Utopia
{
    public class TerrainType : ITerrainGenerator
    {
        #region Private Variables
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public TerrainType(int seed)
        {
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()
        {
            INoise terraintypeFractal = new FractalFbm(new Simplex(_seed), 3, 0.5, enuBaseNoiseRange.ZeroToOne);
            INoise terrainTypeCache = new Cache<INoise>(terraintypeFractal);

            return terrainTypeCache;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
