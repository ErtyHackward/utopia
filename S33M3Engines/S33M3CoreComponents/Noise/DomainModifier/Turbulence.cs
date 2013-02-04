using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.DomainModifier
{
    //== Translate or Offset Domain !
    public class Turbulence : INoise
    {
        #region Private Variables
        private NoiseParam _source;
        private NoiseParam _xOffset;
        private NoiseParam _yOffset;
        private NoiseParam _zOffset;
        private NoiseParam _wOffset;
        #endregion

        #region Public Properties
        #endregion

        public Turbulence(object source, object xOffset, object yOffset, object zOffset, object wOffset)
        {
            _source = new NoiseParam(source);
            _xOffset = new NoiseParam(xOffset);
            _yOffset = new NoiseParam(yOffset);
            _zOffset = new NoiseParam(zOffset);
            _wOffset = new NoiseParam(wOffset);
        }

        public Turbulence(object source, object xOffset, object yOffset, object zOffset)
            :this(source, xOffset, yOffset, zOffset, 0.0)
        {
        }

        public Turbulence(object source, object xOffset, object yOffset)
            : this(source, xOffset, yOffset, 0.0, 0.0)
        {
        }

        #region Public Methods
        public double Get(double x)
        {
            return _source.Get(x + _xOffset.Get(x));
        }

        public double Get(double x, double y)
        {
            return _source.Get(x + _xOffset.Get(x,y), y + _yOffset.Get(x, y));
        }

        public double Get(double x, double y, double z)
        {
            return _source.Get(x + _xOffset.Get(x, y, z), y + _yOffset.Get(x, y, z), z + _zOffset.Get(x, y, z));
        }

        public double Get(double x, double y, double z, double w)
        {
            return _source.Get(x + _xOffset.Get(x, y, z, w), y + _yOffset.Get(x, y, z, w), z + _zOffset.Get(x, y, z, w), w + _wOffset.Get(x, y, z, w));
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
