using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.ResultModifier
{
    public class Bias : INoise
    {
        #region Private Variables
        private INoise _source;
        private NoiseParam _bias;  
        #endregion

        #region Public Properties
        #endregion

        /// <summary>
        ///  Will "biases" the function toward one end of the noise range or the other.
        /// </summary>
        /// <param name="source">Noise source, prefered to be in 0 to 1 range</param>
        /// <param name="bias">Bias value, below 0.5 to push toward the end of the noise range, and above 0.5 to push toward the beggining of the noise range</param>
        public Bias(INoise source, double bias)
        {
            _source = source;
            _bias = new NoiseParam(bias);
        }

        /// <summary>
        ///  Will "biases" the function toward one end of the noise range or the other.
        /// </summary>
        /// <param name="source">Noise source, prefered to be in 0 to 1 range</param>
        /// <param name="bias">Bias value, below 0.5 to push toward the end of the noise range, and above 0.5 to push toward the beggining of the noise range</param>
        public Bias(INoise source, INoise bias)
        {
            _source = source;
            _bias = new NoiseParam(bias);
        }

        #region Public Methods
        public double Get(double x)
        {
            double value = _source.Get(x);
            return Math.Pow(value, Math.Log(_bias.Get(x)) / Math.Log(0.5));
        }

        public double Get(double x, double y)
        {
            double value = _source.Get(x, y);
            return Math.Pow(value, Math.Log(_bias.Get(x, y)) / Math.Log(0.5));
        }

        public double Get(double x, double y, double z)
        {
            double value = _source.Get(x, y, z);
            return Math.Pow(value, Math.Log(_bias.Get(x, y, z)) / Math.Log(0.5));
        }

        public double Get(double x, double y, double z, double w)
        {
            double value = _source.Get(x, y, z, w);
            return Math.Pow(value, Math.Log(_bias.Get(x, y, z, w)) / Math.Log(0.5));
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
