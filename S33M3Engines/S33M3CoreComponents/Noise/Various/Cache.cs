using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.Various
{
    public class Cache<T> : INoise where T : INoise
    {
        private class NoiseCache
        {
            public double x = 0;
            public double y = 0;
            public double z = 0;
            public double w = 0;
            public double u = 0;
            public double v = 0;
            public double val;
            public bool valid = false;
        }

        #region Private Variables
        private INoise _source;
        private T _sourceTyped;
        private NoiseCache _c2, _c3, _c4;
        #endregion

        #region Public Properties
        public T Source { get { return _sourceTyped; } }
        #endregion

        public Cache(T source)
        {
            _source = source;
            _sourceTyped = source;
            InitCache();
        }

        #region Public Methods
        public double Get(double x, double y)
        {
            if (!_c2.valid || _c2.x != x || _c2.y != y)
            {
                _c2.x = x;
                _c2.y = y;
                _c2.valid = true;
                _c2.val = _source.Get(x, y);
            }
            return _c2.val;
        }

        public double Get(double x, double y, double z)
        {
            if (!_c3.valid || _c3.x != x || _c3.y != y || _c3.z != z)
            {
                _c3.x = x;
                _c3.y = y;
                _c3.z = z;
                _c3.valid = true;
                _c3.val = _source.Get(x, y, z);
            }
            return _c3.val;
        }

        public double Get(double x, double y, double z, double w)
        {
            if (!_c4.valid || _c4.x != x || _c4.y != y || _c4.z != z || _c4.w != w)
            {
                _c4.x = x;
                _c4.y = y;
                _c4.z = z;
                _c4.valid = true;
                _c4.val = _source.Get(x, y, z);
            }
            return _c3.val;
        }
        #endregion

        #region Private Methods
        private void InitCache()
        {
            _c2 = new NoiseCache();
            _c3 = new NoiseCache();
            _c4 = new NoiseCache();
        }
        #endregion


    }
}
