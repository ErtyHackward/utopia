using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.ResultModifier
{
    public class ScaleOffset : INoise
    {
        #region Private Variables
        private NoiseParam _source;
        private NoiseParam _scale;
        private NoiseParam _offset;
        #endregion

        #region Public Properties
        #endregion

        public ScaleOffset(object source, object scale, object offset)
        {
            _source = new NoiseParam(source);
            _scale = new NoiseParam(scale);
            _offset = new NoiseParam(offset);
        }

        #region Public Methods
        public double Get(double x, double y)
        {
            return _source.Get(x, y) * _scale.Get(x, y) + _offset.Get(x, y);
        }

        public double Get(double x, double y, double z)
        {
            return _source.Get(x, y, z) * _scale.Get(x, y, z) + _offset.Get(x, y, z);
        }

        public double Get(double x, double y, double z, double w)
        {
            return _source.Get(x, y, z, w) * _scale.Get(x, y, z, w) + _offset.Get(x, y, z, w);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
