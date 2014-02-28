using S33M3CoreComponents.Noise;
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
            INoise Temperature_fractal = new Voronoi2(_seed, _freq);
            INoise ClampedValue = new Clamp(Temperature_fractal, 0, 1);
            return Temperature_fractal;
        }
    }
}
