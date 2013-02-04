using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.Various
{
    public class NoiseAccess : INoise
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public enum enuDimUsage
        {
            Noise1D,
            Noise2D,
            Noise3D,
            Noise4D
        }

        #region Private Variables
        private INoise _source;
        private enuDimUsage _noiseDimUsage = enuDimUsage.Noise2D;
        private INoise2 _noise2D;
        private INoise3 _noise3D;
        private INoise4 _noise4D;
        #endregion

        #region Public Properties
        #endregion

        public NoiseAccess(INoise source, enuDimUsage noiseDimUsage, bool withCaching = false)
        {
            if (withCaching)
            {
                source = new Cache<INoise>(source);
            }

            switch (noiseDimUsage)
            {
                case enuDimUsage.Noise2D:
                    _noise2D = source as INoise2;
                    break;
                case enuDimUsage.Noise3D:
                    _noise3D = source as INoise3;
                    break;
                case enuDimUsage.Noise4D:
                    _noise4D = source as INoise4;
                    break;
                default:
                    break;
            }

            noiseDimUsage = _noiseDimUsage;
            _source = source;
        }

        #region Public Methods
        public double Get(double x)
        {
            switch (_noiseDimUsage)
            {
                case enuDimUsage.Noise2D:
                    return _noise2D.Get(x, 0);
                case enuDimUsage.Noise3D:
                    return _noise3D.Get(x, 0, 0);
                case enuDimUsage.Noise4D:
                    return _noise4D.Get(x, 0, 0, 0);
                default:
                    return _noise2D.Get(x, 0);
            }
        }

        public double Get(double x, double y)
        {
            switch (_noiseDimUsage)
            {
                case enuDimUsage.Noise3D:
                    return _noise3D.Get(x, 0, y);
                case enuDimUsage.Noise4D:
                    return _noise4D.Get(x, y, 0, 0);
                default:
                    return _noise2D.Get(x, y);
            }
        }

        public double Get(double x, double y, double z)
        {
            switch (_noiseDimUsage)
            {
                case enuDimUsage.Noise2D:
                    return _noise2D.Get(x, z);
                case enuDimUsage.Noise4D:
                    return _noise4D.Get(x, y, z, 0);
                default:
                    return _noise3D.Get(x, y, z);
            }
        }

        public double Get(double x, double y, double z, double w)
        {
            switch (_noiseDimUsage)
            {
                case enuDimUsage.Noise2D:
                    return _noise2D.Get(x, y);
                case enuDimUsage.Noise3D:
                    return _noise3D.Get(x, y, z);
                default:
                    return _noise4D.Get(x, y, z, w);
            }
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
