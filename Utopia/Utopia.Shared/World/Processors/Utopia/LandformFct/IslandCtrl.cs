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
    public class IslandCtrl : ITerrainGenerator
    {
        #region Private Variables
        private int _seed;
        #endregion

        #region Public Properties
        #endregion

        public IslandCtrl(int seed)
        {
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()
        {
            //Create a Landscape 2D (Forced no matter the sampling realized on it).
            INoise islandDisk =new Sphere(0.7, 0, 0);

            //Perturbation on the X and Y space
            INoise islandDiskXPerturb = new FractalFbm(new Perlin(_seed + 3), 10, 3);
            INoise islandDiskYPerturb = new FractalFbm(new Perlin(_seed + 5), 10, 3);

            INoise islandTurbulence = new NoiseAccess(new Turbulence(islandDisk, islandDiskXPerturb, 0.0, islandDiskYPerturb), NoiseAccess.enuDimUsage.Noise2D, true);

            return islandTurbulence;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
