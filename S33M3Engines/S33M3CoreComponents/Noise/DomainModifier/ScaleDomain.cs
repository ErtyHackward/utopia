using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.DomainModifier
{
    public class ScaleDomain : INoise
    {
        #region Private Variables
        private NoiseParam _xScale;
        private NoiseParam _yScale;
        private NoiseParam _zScale;
        private NoiseParam _wScale;
        private NoiseParam _source;
        #endregion

        #region Public Properties
        #endregion

        public ScaleDomain(object source, object XScale, object YScale, object ZScale, object WScale)
        {
            _source = new NoiseParam(source);
            _xScale = new NoiseParam(XScale);
            _yScale = new NoiseParam(YScale);
            _zScale = new NoiseParam(ZScale);
            _wScale = new NoiseParam(WScale);
        }

        public ScaleDomain(object source, object XScale, object YScale, object ZScale)
            : this(source, XScale, YScale, ZScale, 1.0)
        {
        }

        public ScaleDomain(object source, object XScale, object YScale)
            : this(source, XScale, YScale, 1.0, 1.0)
        {
        }

        #region Public Methods
        public double Get(double x, double y)
        {
            return _source.Get(x * _xScale.Get(x, y), y * _yScale.Get(x, y));
        }

        public double Get(double x, double y, double z)
        {
            return _source.Get(x * _xScale.Get(x, y, z), y * _yScale.Get(x, y, z), z * _zScale.Get(x, y, z));
        }

        public double Get(double x, double y, double z, double w)
        {
            return _source.Get(x * _xScale.Get(x, y, z, w), y * _yScale.Get(x, y, z, w), z * _zScale.Get(x, y, z, w), w * _wScale.Get(x, y, z, w));
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
