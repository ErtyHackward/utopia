using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.ResultModifier
{
    public class Gain : INoise
    {
        #region Private Variables
        private INoise _source;
        private NoiseParam _gain;
        #endregion

        #region Public Properties
        #endregion

        /// <summary>
        ///  Has the effect of pushing the values of the input either toward the ends and away from the middle (if bias is >0.5) or pushing values toward the middle and away from the ends (if bias is below 0.5)
        /// </summary>
        /// <param name="source">Noise source, prefered to be in 0 to 1 range</param>
        /// <param name="gain">Bias value, below 0.5 to push toward the end of the noise range, and above 0.5 to push toward the beggining of the noise range</param>
        public Gain(INoise source, double gain)
        {
            _source = source;
            _gain = new NoiseParam(gain);
        }

        /// <summary>
        ///  Has the effect of pushing the values of the input either toward the ends and away from the middle (if bias is >0.5) or pushing values toward the middle and away from the ends (if bias is < 0.5)
        /// </summary>
        /// <param name="source">Noise source, prefered to be in 0 to 1 range</param>
        /// <param name="gain">Bias value, below 0.5 to push toward the end of the noise range, and above 0.5 to push toward the beggining of the noise range</param>
        public Gain(INoise source, INoise gain)
        {
            _source = source;
            _gain = new NoiseParam(gain);
        }

        #region Public Methods
        public double Get(double x)
        {
            double value = _source.Get(x);
            double gain = _gain.Get(x);

            if (gain < 0.5)
            {
                return Math.Pow(2.0 * value, Math.Log(1.0 - gain) / Math.Log(0.5)) / 2.0;
            }
            else
            {
                return 1.0 - Math.Pow(2.0 - 2.0 * value, Math.Log(1.0 - gain) / Math.Log(0.5)) / 2.0;
            }
        }

        public double Get(double x, double y)
        {
            double value = _source.Get(x, y);
            double gain = _gain.Get(x, y);

            if (gain < 0.5)
            {
                return Math.Pow(2.0 * value, Math.Log(1.0 - gain) / Math.Log(0.5)) / 2.0;
            }
            else
            {
                return 1.0 - Math.Pow(2.0 - 2.0 * value, Math.Log(1.0 - gain) / Math.Log(0.5)) / 2.0;
            }
        }

        public double Get(double x, double y, double z)
        {
            double value = _source.Get(x, y, z);
            double gain = _gain.Get(x, y, z);

            if (gain < 0.5)
            {
                return Math.Pow(2.0 * value, Math.Log(1.0 - gain) / Math.Log(0.5)) / 2.0;
            }
            else
            {
                return 1.0 - Math.Pow(2.0 - 2.0 * value, Math.Log(1.0 - gain) / Math.Log(0.5)) / 2.0;
            }
        }

        public double Get(double x, double y, double z, double w)
        {
            double value = _source.Get(x, y, z, w);
            double gain = _gain.Get(x, y, z, w);

            if (gain < 0.5)
            {
                return Math.Pow(2.0 * value, Math.Log(1.0 - gain) / Math.Log(0.5)) / 2.0;
            }
            else
            {
                return 1.0 - Math.Pow(2.0 - 2.0 * value, Math.Log(1.0 - gain) / Math.Log(0.5)) / 2.0;
            }
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
