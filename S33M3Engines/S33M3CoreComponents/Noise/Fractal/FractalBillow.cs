using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise.Generator;

namespace S33M3CoreComponents.Noise.Fractal
{
    public class FractalBillow : FractalBase, INoise
    {
        #region Private Variables
        #endregion

        #region Public Properties
        #endregion

        public FractalBillow(INoise source, int NumOctaves, double Frequency, enuBaseNoiseRange fractalOutputRange = enuBaseNoiseRange.MinOneToOne)
            :base(source, fractalOutputRange)
        {
            _frequency = Frequency;
            _numOctaves = NumOctaves;
            _lacunarity = 2.0;
            _h = 1.0;
            _gain = 0.0;
            _offset = 0.0;
            _withValueRemap = true;

            CalcWeights();
        }

        #region Public Methods
        public double Get(double x, double y)
        {
            double value=0.0, signal;
            x*=_frequency;
            y*=_frequency;

            for(int i=0; i< _numOctaves; ++i)
            {
                signal = _source.Get(x,y);
                signal = 2.0 * Math.Abs(signal) - 1.0;
                value += signal * _exparray[i];

                x*=_lacunarity;
                y*=_lacunarity;
            }

            value += 0.5;

            if (_withValueRemap)
            {
                FractalRemap remap = _fractalRemap[_numOctaves - 1];
                if (base._defaultRange == enuBaseNoiseRange.MinOneToOne)
                {
                    return value * remap.Scale + remap.Bias;
                }
                else
                {
                    return ((value * remap.Scale + remap.Bias) + 1.0) / 2.0;
                }
            }
            else
            {
                return value;
            }
        }

        public double Get(double x, double y, double z)
        {
            double value = 0.0, signal;
            x *= _frequency;
            y *= _frequency;
            z *= _frequency;

            for (int i = 0; i < _numOctaves; ++i)
            {
                signal = _source.Get(x, y, z);
                signal = 2.0 * Math.Abs(signal) - 1.0;
                value += signal * _exparray[i];

                x *= _lacunarity;
                y *= _lacunarity;
                z *= _lacunarity;

            }

            value += 0.5;

            if (_withValueRemap)
            {
                FractalRemap remap = _fractalRemap[_numOctaves - 1];
                if (base._defaultRange == enuBaseNoiseRange.MinOneToOne)
                {
                    return value * remap.Scale + remap.Bias;
                }
                else
                {
                    return ((value * remap.Scale + remap.Bias) + 1.0) / 2.0;
                }
            }
            else
            {
                return value;
            }
        }

        public double Get(double x, double y, double z, double w)
        {
            double value = 0.0, signal;
            x *= _frequency;
            y *= _frequency;
            z *= _frequency;
            w *= _frequency;

            for (int i = 0; i < _numOctaves; ++i)
            {
                signal = _source.Get(x, y, z, w);
                signal = 2.0 * Math.Abs(signal) - 1.0;
                value += signal * _exparray[i];

                x *= _lacunarity;
                y *= _lacunarity;
                z *= _lacunarity;
                w *= _lacunarity;

            }

            value += 0.5;

            if (_withValueRemap)
            {
                FractalRemap remap = _fractalRemap[_numOctaves - 1];
                if (base._defaultRange == enuBaseNoiseRange.MinOneToOne)
                {
                    return value * remap.Scale + remap.Bias;
                }
                else
                {
                    return ((value * remap.Scale + remap.Bias) + 1.0) / 2.0;
                }
            }
            else
            {
                return value;
            }
        }
        #endregion

        #region Private Methods
        protected override void CalcWeights()
        {
            _exparray = new double[MAXOCTAVE];

            for (int i = 0; i < MAXOCTAVE; ++i)
            {
                _exparray[i] = Math.Pow(_lacunarity, -i * _h);
            }

            // Calculate scale/bias pairs by guessing at minimum and maximum values and remapping to [-1,1]
            double minvalue = 0.0, maxvalue = 0.0;
            for (int i = 0; i < MAXOCTAVE; ++i)
            {
                minvalue += -1.0 * _exparray[i];
                maxvalue += 1.0 * _exparray[i];

                double A = -1.0, B = 1.0;
                double scale = (B - A) / (maxvalue - minvalue);
                double bias = A - minvalue * scale;

                _fractalRemap[i].Bias = bias;
                _fractalRemap[i].Scale = scale;
            }
        }
        #endregion
    }
}
