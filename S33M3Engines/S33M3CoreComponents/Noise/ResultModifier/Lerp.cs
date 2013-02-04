using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;

namespace S33M3CoreComponents.Noise.ResultModifier
{
    public class Lerp : INoise
    {
        #region Private Variables
        private NoiseParam _source;
        private double _targetMin, _targetMax, _sourceMin, _sourceMax;
        #endregion

        #region Public Properties
        #endregion

        public Lerp(object source, double targetFrom, double targetTo, double sourceFrom, double sourceTo)
        {
            _source = new NoiseParam(source);
            _targetMin = targetFrom;
            _targetMax = targetTo;
            _sourceMin = sourceFrom;
            _sourceMax = sourceTo;
        }

        #region Public Methods
        public double Get(double x)
        {
            return MathHelper.FullLerp(_targetMin, _targetMax, _sourceMin, _sourceMax, _source.Get(x));
        }

        public double Get(double x, double y)
        {
            return MathHelper.FullLerp(_targetMin, _targetMax, _sourceMin, _sourceMax, _source.Get(x, y));
        }

        public double Get(double x, double y, double z)
        {
            return MathHelper.FullLerp(_targetMin, _targetMax, _sourceMin, _sourceMax, _source.Get(x, y, z));
        }

        public double Get(double x, double y, double z, double w)
        {
            return MathHelper.FullLerp(_targetMin, _targetMax, _sourceMin, _sourceMax, _source.Get(x, y, z, w));
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
