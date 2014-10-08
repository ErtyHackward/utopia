using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.ResultModifier;
using S33M3CoreComponents.Noise.Various;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.World.Processors.Utopia.OtherFct
{
    public class VoronoiZones
    {
        #region Private Variables
        private int _seed;
        private double _freq;
        #endregion

        public VoronoiZones(int seed, double frequency = 1)
        {
            _seed = seed;
            _freq = frequency;
        }

        public INoise GetLandFormFct()
        {
            //INoise Temperature_fractal = new ScaleOffset(new Voronoi2(_seed, _freq, 0.8, new FractalFbm(new Perlin(7894), 2, 0.8)), 0.5, 0);
            INoise Temperature_fractal = new Voronoi2(_seed, _freq, 0.8);
            INoise ClampedValue = new Clamp(Temperature_fractal, 0, 1);
            return Temperature_fractal;
        }
    }
}
