using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noises.Interfaces;
using S33M3CoreComponents.Noises.Filter;

namespace S33M3CoreComponents.Noises.Filter
{
    /// <summary>
    /// Class that will be used to Force the INoise2 or INoise3 interface, no matter the GetValue() used to acces the data
    /// </summary>
    public class NoiseAccess : INoiseAccess
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        private INoise2 _noise2D;
        private INoise3 _noise3D;
        private INoise _noise;
        #endregion

        #region Public variables
        /// <summary>
        /// The constant value
        /// </summary>

        /// <summary>
        /// Set the GetValue that must be used with the INoise generator
        /// </summary>
        public enuDimUsage NoiseDimUsage { get; set; }
        #endregion

        /// <summary>
        /// Create a component that will generate a constant value, that is not impacted by the input data
        /// </summary>
        /// <param name="cstValue">The constant value that will be generated</param>
        public NoiseAccess(INoise noise, enuDimUsage noiseDimUsage = enuDimUsage.Noise3D)
        {
            //Check if the noise type is supporting the enuDimUsage
            if (noise is INoise2 == false && noiseDimUsage == enuDimUsage.Noise2D ||
                noise is INoise3 == false && noiseDimUsage == enuDimUsage.Noise3D)
            {
                logger.Error("Cannot use the passed noise Noise fct as {0} ", noiseDimUsage.ToString());
            }

            switch (noiseDimUsage)
            {
                case enuDimUsage.Noise2D:
                    _noise2D = noise as INoise2;
                    break;
                case enuDimUsage.Noise3D:
                    _noise3D = noise as INoise3;
                    break;
                default:
                    break;
            }

            _noise = noise;
            NoiseDimUsage = noiseDimUsage;
        }

        #region Public methods
        public float GetValue(float x, float z)
        {
            if (_noise2D != null) return _noise2D.GetValue(x, z);
            return _noise3D.GetValue(x, 0, z);
        }

        public float GetValue(float x, float y, float z)
        {
            if (_noise2D != null) return _noise2D.GetValue(x, z);
            return _noise3D.GetValue(x, y, z);
        }
        #endregion

        #region Private methods
        #endregion
    }
}
