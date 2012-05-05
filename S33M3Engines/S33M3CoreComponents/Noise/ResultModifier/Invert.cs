using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.ResultModifier
{
    public class Invert : INoise
    {
        #region Private Variables
        private INoise _source;
        #endregion

        #region Public Properties
        #endregion

        public Invert(INoise source)
        {
            _source = source;
        }

        #region Public Methods
        public double Get(double x, double y)
        {
            return -1.0 * _source.Get(x, y);
        }

        public double Get(double x, double y, double z)
        {
            return -1.0 * _source.Get(x, y, z);
        }

        public double Get(double x, double y, double z, double w)
        {
            return -1.0 * _source.Get(x, y, z, w);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
