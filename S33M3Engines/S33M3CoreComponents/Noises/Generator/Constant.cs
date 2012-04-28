using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noises.Generator;
using S33M3CoreComponents.Noises.Interfaces;

namespace S33M3CoreComponents.Noises.Generator
{
    /// <summary>
    /// Constant value generator
    /// </summary>
    public class Constant : INoise2, INoise3
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        #endregion

        #region Public variables
        /// <summary>
        /// The constant value
        /// </summary>
        public float CstValue { get; set; }
        #endregion

        /// <summary>
        /// Create a component that will generate a constant value, that is not impacted by the input data
        /// </summary>
        /// <param name="cstValue">The constant value that will be generated</param>
        public Constant(float cstValue)
        {
            CstValue = cstValue;
        }

        #region Public methods
        /// <summary>
        /// Return a constant value, no matter the data inputed
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="z">Z value</param>
        /// <returns></returns>
        public float GetValue(float x, float z)
        {
            return CstValue;
        }
        
        /// <summary>
        /// Return a constant value, no matter the data inputed
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <param name="z">Z value</param>
        /// <returns></returns>
        public float GetValue(float x, float y, float z)
        {
            return CstValue;
        }
        #endregion

        #region Private methods
        #endregion
    }
}
