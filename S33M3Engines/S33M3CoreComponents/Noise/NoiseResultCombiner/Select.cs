using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;

namespace S33M3CoreComponents.Noise.NoiseResultCombiner
{
    public class Select : INoise
    {
        #region Private Variables
        private NoiseParam _lowSource;
        private NoiseParam _highSource;
        private NoiseParam _control;
        private NoiseParam _threshold;
        private NoiseParam _falloff;
        #endregion

        #region Public Properties
        public NoiseParam LowSource { get { return _lowSource; } }
        public NoiseParam HighSource { get { return _highSource; } }
        public NoiseParam Control { get { return _control; } }
        public NoiseParam Threshold { get { return _threshold; } }
        public NoiseParam Falloff { get { return _falloff; } }

        public string Name { get; set; }
        #endregion

        public Select(object lowSource, object highSource, object control, object threshold, object falloff)
        {
            _lowSource = new NoiseParam(lowSource);
            _highSource = new NoiseParam(highSource);
            _control = new NoiseParam(control);
            _threshold = new NoiseParam(threshold);
            _falloff = new NoiseParam(falloff);
        }

        public Select(object lowSource, object highSource, object control, object threshold)
            : this(lowSource, highSource, control, threshold, 0.0)
        {
        }

        #region Public Methods
        public double Get(double x)
        {
            double control = _control.Get(x);
            double falloff = _falloff.Get(x);
            double threshold = _threshold.Get(x);

            if (falloff > 0.0)
            {
                if (control < (threshold - falloff))
                {
                    // Lies outside of falloff area below threshold, return first source
                    return _lowSource.Get(x);
                }
                else if (control > (threshold + falloff))
                {
                    // Lise outside of falloff area above threshold, return second source
                    return _highSource.Get(x);
                }
                else
                {
                    // Lies within falloff area.
                    double lower = threshold - falloff;
                    double upper = threshold + falloff;
                    double blend = MathHelper.SCurve5((control - lower) / (upper - lower));
                    return MathHelper.Lerp(_lowSource.Get(x), _highSource.Get(x), blend);
                }
            }
            else
            {
                if (control < threshold) return _lowSource.Get(x);
                else return _highSource.Get(x);
            }
        }

        public double Get(double x, double y)
        {
            double control = _control.Get(x, y);
            double falloff = _falloff.Get(x, y);
            double threshold = _threshold.Get(x, y);

            if (falloff > 0.0)
            {
                if (control < (threshold - falloff))
                {
                    // Lies outside of falloff area below threshold, return first source
                    return _lowSource.Get(x, y);
                }
                else if (control > (threshold + falloff))
                {
                    // Lise outside of falloff area above threshold, return second source
                    return _highSource.Get(x, y);
                }
                else
                {
                    // Lies within falloff area.
                    double lower = threshold - falloff;
                    double upper = threshold + falloff;
                    double blend = MathHelper.SCurve5((control - lower) / (upper - lower));
                    return MathHelper.Lerp(_lowSource.Get(x, y), _highSource.Get(x, y), blend);
                }
            }
            else
            {
                if (control < threshold) return _lowSource.Get(x, y);
                else return _highSource.Get(x, y);
            }
        }

        public double Get(double x, double y, double z)
        {
            double control = _control.Get(x, y, z);
            double falloff = _falloff.Get(x, y, z);
            double threshold = _threshold.Get(x, y, z);

            if (falloff > 0.0)
            {
                if (control < (threshold - falloff))
                {
                    // Lies outside of falloff area below threshold, return first source
                    return _lowSource.Get(x, y, z);
                }
                else if (control > (threshold + falloff))
                {
                    // Lise outside of falloff area above threshold, return second source
                    return _highSource.Get(x, y, z);
                }
                else
                {
                    // Lies within falloff area.
                    double lower = threshold - falloff;
                    double upper = threshold + falloff;
                    double blend = MathHelper.SCurve5((control - lower) / (upper - lower));
                    return MathHelper.Lerp(_lowSource.Get(x, y, z), _highSource.Get(x, y, z), blend);
                }
            }
            else
            {
                if (control < threshold) return _lowSource.Get(x, y, z);
                else return _highSource.Get(x, y, z);
            }
        }

        public double Get(double x, double y, double z, double w)
        {
            double control = _control.Get(x, y, z, w);
            double falloff = _falloff.Get(x, y, z, w);
            double threshold = _threshold.Get(x, y, z, w);

            if (falloff > 0.0)
            {
                if (control < (threshold - falloff))
                {
                    // Lies outside of falloff area below threshold, return first source
                    return _lowSource.Get(x, y, z, w);
                }
                else if (control > (threshold + falloff))
                {
                    // Lise outside of falloff area above threshold, return second source
                    return _highSource.Get(x, y, z, w);
                }
                else
                {
                    // Lies within falloff area.
                    double lower = threshold - falloff;
                    double upper = threshold + falloff;
                    double blend = MathHelper.SCurve5((control - lower) / (upper - lower));
                    return MathHelper.Lerp(_lowSource.Get(x, y, z, w), _highSource.Get(x, y, z, w), blend);
                }
            }
            else
            {
                if (control < threshold) return _lowSource.Get(x, y, z);
                else return _highSource.Get(x, y, z);
            }
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
