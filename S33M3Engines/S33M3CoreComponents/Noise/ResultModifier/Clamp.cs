using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;

namespace S33M3CoreComponents.Noise.ResultModifier
{
    public class Clamp : INoise
    {
        #region Private Variables
        private INoise _source;
        private double _lowClamp;
        private double _highClamp;
        #endregion

        #region Public Properties
        #endregion

        public Clamp(INoise source, double lowClamp, double highClamp)
        {
            _source = source;
            _lowClamp = lowClamp;
            _highClamp = highClamp;
        }

        #region Public Methods
        public double Get(double x)
        {
            return MathHelper.Clamp(_source.Get(x), _lowClamp, _highClamp);
        }

        public double Get(double x, double y)
        {
            return MathHelper.Clamp(_source.Get(x, y), _lowClamp, _highClamp);
        }

        public double Get(double x, double y, double z)
        {
            return MathHelper.Clamp(_source.Get(x, y, z), _lowClamp, _highClamp);
        }

        public double Get(double x, double y, double z, double w)
        {
            return MathHelper.Clamp(_source.Get(x, y, z, w), _lowClamp, _highClamp);
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
