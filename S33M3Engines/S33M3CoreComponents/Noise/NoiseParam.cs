using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise
{
    public class NoiseParam
    {
        #region Private Variables
        private INoise _noiseParam;
        private double _scalarParam;
        #endregion

        #region Public Properties
        public INoise Param
        {
            get { return _noiseParam; }
        }
        public double ScalarParam
        {
            get { return _scalarParam; }
        }
        #endregion

        public NoiseParam(object param)
        {
            if (param is Enum || param is INoise || param.GetType() == typeof(double) || param.GetType() == typeof(int) || param is NoiseParam)
            {
                if (param is INoise)
                {
                    SetSource((INoise)param);
                }
                else
                {
                    if (param is double)
                    {
                        SetSource((double)param);
                    }
                    else
                    {
                        if (param is int || param is Enum)
                        {
                            SetSource((int)param);
                        }
                        else
                        {
                            NoiseParam p = (NoiseParam)param;
                            this._noiseParam = p.Param;
                            this._scalarParam = p.ScalarParam;
                        }
                    }
                }
            }
            else
            {
                throw new Exception("param from NoiseParam must be INoise fct or double");
            }
        }

        #region Public Methods
        public void SetSource(double scalarParam)
        {
            _noiseParam = null;
            _scalarParam = scalarParam;
        }

        public void SetSource(INoise noiseParam)
        {
            _noiseParam = noiseParam;
        }

        public double Get(double x)
        {
            return _noiseParam == null ? _scalarParam : _noiseParam.Get(x);
        }

        public double Get(double x, double y)
        {
            return _noiseParam == null ? _scalarParam : _noiseParam.Get(x, y);
        }

        public double Get(double x, double y, double z)
        {
            return _noiseParam == null ? _scalarParam : _noiseParam.Get(x, y, z);
        }

        public double Get(double x, double y, double z, double w)
        {
            return _noiseParam == null ? _scalarParam : _noiseParam.Get(x, y, z, w);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
