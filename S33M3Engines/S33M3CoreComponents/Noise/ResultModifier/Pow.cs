using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.ResultModifier
{
    public class Pow : INoise
    {
        #region Private Variables
        private NoiseParam _source;
        private NoiseParam _power;
        #endregion

        #region Public Properties
        #endregion

        public Pow(object source, object power)
        {
            _source = new NoiseParam(source);
            _power = new NoiseParam(power);
        }

        #region Public Methods
        public double Get(double x, double y)
        {
            return Math.Pow(_source.Get(x, y), _power.Get(x, y));
        }

        public double Get(double x, double y, double z)
        {
            return Math.Pow(_source.Get(x, y, z), _power.Get(x, y, z));
        }

        public double Get(double x, double y, double z, double w)
        {
            return Math.Pow(_source.Get(x, y, z, w), _power.Get(x, y, z, w));
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
