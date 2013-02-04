using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;

namespace S33M3CoreComponents.Noise.Generator
{
    public class White : INoise
    {
        #region Private variable
        private int _seed;
        private FastRandom _rnd;
        #endregion

        #region Public variables/properties
        #endregion

        /// <summary>
        /// Will generate value in the 0 to 1 range
        /// </summary>
        /// <param name="seed">Seed used by the RND function</param>
        public White(int seed)
        {
            _seed = seed;
            _rnd = new FastRandom(_seed);
        }

        #region Public methods
        public double Get(double x)
        {
            return _rnd.NextDouble();
        }

        public double Get(double x, double y)
        {
            return _rnd.NextDouble();
        }

        public double Get(double x, double y, double z)
        {
            return _rnd.NextDouble();
        }

        public double Get(double x, double y, double z, double w)
        {
            return _rnd.NextDouble();
        }
        #endregion

        #region Private methods
        #endregion
    }
}
