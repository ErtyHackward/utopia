using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.ResultModifier;
using S33M3CoreComponents.Noise.Various;

namespace Utopia.Shared.World.Processors.Utopia.LandformFct
{
    public class River
    {
        #region Private Variables
        private int _seed;
        private int _octave;
        private double _freq;
        #endregion

        #region Public Properties
        #endregion

        public River(int seed, int octave = 2, double frequency = 1)
        {
            _octave = octave;
            _freq = frequency;
            _seed = seed;
        }

        #region Public Methods
        public INoise GetLandFormFct()            
        {
            return new Voronoi2(_seed, 0.75, 0.6, new ScaleOffset(new FractalFbm(new Perlin(7894), 2, 1.5), 0.5, 0.0)) { Mode = Voronoi2.VoronoiMode.FrontierDetection };
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
