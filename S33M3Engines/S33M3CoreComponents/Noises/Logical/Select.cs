using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noises.Interfaces;
using S33M3CoreComponents.Maths;
using S33M3CoreComponents.Noises.Filter;

namespace S33M3CoreComponents.Noises.Logical
{
    public class Select : INoise2, INoise3
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        private float _fallof, _minFallof, _maxFallof;
        private float _threshold;
        #endregion

        #region Public variables
        /// <summary>
        /// If the MainSource value is below this threshold then then LowSource value will be outputed
        /// If the MainSource value is above or equal to this threshold then then HighSource value will be outputed
        /// </summary>
        public float Threshold
        {
            get { return _threshold; }
            set
            {
                _threshold = value;
                RefreshMinMaxFallof();
            }
        }
        /// <summary>
        /// A value that will give the possibility to go from LowSource to Highsource when around Threshold, slowly instead of brutally when set to 0
        /// It define a range from Threshold - Fallof to Threshold + Fallof where the data form both Low and High sources will be Lerped
        /// </summary>
        public float Fallof
        {
            get { return _fallof; }
            set
            {
                _fallof = value;
                RefreshMinMaxFallof();
            }
        }
        /// <summary>
        /// The Noise result that will be used against the Threshold in order the output LowSource or HighSource value
        /// </summary>
        public INoiseAccess SelectSource { get; set; }
        /// <summary>
        /// will give the value that will be outputed in case if the MainSource value is below the Threshold
        /// </summary>
        public INoiseAccess LowSource { get; set; }
        /// <summary>
        /// will give the value that will be outputed in case if the MainSource value is above or equal to the Threshold
        /// </summary>
        public INoiseAccess HighSource { get; set; }
        #endregion

        /// <summary>
        /// Function that will help to select result from LowSource or Highsource based on MainSource data and the threshold
        /// </summary>
        /// <param name="selectSource">The Noise result that will be used against the Threshold in order the output LowSource or HighSource value</param>
        /// <param name="lowSource">will give the value that will be outputed in case if the MainSource value is below the Threshold</param>
        /// <param name="highSource">will give the value that will be outputed in case if the MainSource value is above or equal to the Threshold</param>
        /// <param name="threshold">Variable use by mainsource to decide if the output must be choosen from lowSource or highSource</param>
        public Select(INoiseAccess selectSource, INoiseAccess lowSource, INoiseAccess highSource, float threshold)
        {
            SelectSource = selectSource;
            LowSource = lowSource;
            HighSource = highSource;
            Threshold = threshold;
        }

        #region  Public methods
        public float GetValue(float x, float z)
        {
            //Get the MainSource Value
            float SelectSourceValue = SelectSource.GetValue(x, z);

            //Am I in the Falloff zone ?
            if (Fallof == 0 || SelectSourceValue < _minFallof || SelectSourceValue > _maxFallof)
            {
                if (SelectSourceValue < Threshold)
                {
                    return LowSource.GetValue(x, z);
                }
                else
                {
                    return HighSource.GetValue(x, z);
                }
            }
            else
            {
                //My SelectSourceValue is inside the Falloff zone
                float lowSourceValue = LowSource.GetValue(x, z);
                float highSourceValue = HighSource.GetValue(x, z);

                float lerpedOutValue = MathHelper.FullLerp(lowSourceValue, highSourceValue, _minFallof, _maxFallof, SelectSourceValue);
                return lerpedOutValue;
            }
        }

        public float GetValue(float x, float y, float z)
        {
            //Get the MainSource Value
            float SelectSourceValue = SelectSource.GetValue(x, y, z);

            //Am I in the Falloff zone ?
            if (Fallof == 0 || SelectSourceValue < _minFallof || SelectSourceValue > _maxFallof)
            {
                if (SelectSourceValue < Threshold)
                {
                    return LowSource.GetValue(x, y, z);
                }
                else
                {
                    return HighSource.GetValue(x, y, z);
                }
            }
            else
            {
                //My SelectSourceValue is inside the Falloff zone
                float lowSourceValue = LowSource.GetValue(x, y, z);
                float highSourceValue = HighSource.GetValue(x, y, z);

                float lerpedOutValue = MathHelper.FullLerp(lowSourceValue, highSourceValue, _minFallof, _maxFallof, SelectSourceValue);
                return lerpedOutValue;
            }
        }
        #endregion

        #region Private methods
        private void RefreshMinMaxFallof()
        {
            _minFallof = Threshold - _fallof;
            _maxFallof = Threshold + _fallof;
        }
        #endregion
    }
}
