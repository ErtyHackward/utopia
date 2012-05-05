using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;
using S33M3CoreComponents.Maths;

namespace S33M3CoreComponents.Noise.ResultCombiner
{
    public class Blend : INoise
    {
        #region Private Variables
        private static readonly RangeD DEFAULTRANGE = new RangeD(-1,1);

        private NoiseParam _lowSource;
        private NoiseParam _highSource;
        private NoiseParam _controler;
        private RangeD _controlerRange;
        #endregion

        #region Public Properties
        #endregion

        /// <summary>
        /// Will blend two noise source together with a LERP alpha from the controlerRange
        /// </summary>
        /// <param name="lowSource">lowSource, MUST be a double or a INoise fct, otherwhile it will raised an error at run time !</param>
        /// <param name="highSource">HighSource, MUST be a double or a INoise fct, otherwhile it will raised an error at run time !</param>
        /// <param name="controler">LERP alpha value source, MUST be a double or a INoise fct, otherwhile it will raised an error at run time !</param>
        /// <param name="controlerRange">The range of the Alpha controler value</param>
        public Blend(object lowSource, object highSource, object controler, RangeD controlerRange)
        {
            _lowSource = new NoiseParam(lowSource);
            _highSource = new NoiseParam(highSource);
            _controler = new NoiseParam(controler);
            _controlerRange = controlerRange;
        }

        /// <summary>
        /// Will blend two noise source together with a LERP alpha from the controlerRange, the controler range will be defaulted to -1;1
        /// </summary>
        /// <param name="lowSource">lowSource, MUST be a double or a INoise fct, otherwhile it will raised an error at run time !</param>
        /// <param name="highSource">HighSource, MUST be a double or a INoise fct, otherwhile it will raised an error at run time !</param>
        /// <param name="controler">LERP alpha value source, MUST be a double or a INoise fct, otherwhile it will raised an error at run time !</param>
        public Blend(object lowSource, object highSource, object controler)
            :this(lowSource, highSource, controler, Blend.DEFAULTRANGE)
        {
        }

        #region Public Methods
        public double Get(double x, double y)
        {
            double vLow = _lowSource.Get(x, y);
            double vHigh = _highSource.Get(x, y);
            double blend = _controler.Get(x, y);

            return MathHelper.FullLerp(vLow, vHigh, _controlerRange.Min, _controlerRange.Max, blend);
        }

        public double Get(double x, double y, double z)
        {
            double vLow = _lowSource.Get(x, y, z);
            double vHigh = _highSource.Get(x, y, z);
            double blend = _controler.Get(x, y, z);

            return MathHelper.FullLerp(vLow, vHigh, _controlerRange.Min, _controlerRange.Max, blend);
        }

        public double Get(double x, double y, double z, double w)
        {
            double vLow = _lowSource.Get(x, y, z, w);
            double vHigh = _highSource.Get(x, y, z, w);
            double blend = _controler.Get(x, y, z, w);

            return MathHelper.FullLerp(vLow, vHigh, _controlerRange.Min, _controlerRange.Max, blend);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
