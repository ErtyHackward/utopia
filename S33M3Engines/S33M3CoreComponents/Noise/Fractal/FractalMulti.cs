using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise.Generator;

namespace S33M3CoreComponents.Noise.Fractal
{
    public class FractalMulti : FractalBase , INoise
    {
        #region Private Variables
        #endregion

        #region Public Properties
        #endregion

        public FractalMulti(INoise source, int NumOctaves, double Frequency, enuBaseNoiseRange fractalOutputRange = enuBaseNoiseRange.MinOneToOne)
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
            double value=1.0;
            x*=_frequency;
            y*=_frequency;

            for(int i=0; i< _numOctaves; ++i)
            {
                value *= _source.Get(x,y) * _exparray[i] + 1.0;
                x*=_lacunarity;
                y*=_lacunarity;
            }

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
            double value = 1.0;
            x *= _frequency;
            y *= _frequency;
            z *= _frequency;

            for (int i = 0; i < _numOctaves; ++i)
            {
                value *= _source.Get(x, y, z) * _exparray[i] + 1.0;
                x *= _lacunarity;
                y *= _lacunarity;
                z *= _lacunarity;
            }

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
            double value = 1.0;
            x *= _frequency;
            y *= _frequency;
            z *= _frequency;
            w *= _frequency;

            for (int i = 0; i < _numOctaves; ++i)
            {
                value *= _source.Get(x, y, z, w) * _exparray[i] + 1.0;
                x *= _lacunarity;
                y *= _lacunarity;
                z *= _lacunarity;
                w *= _lacunarity;
            }

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
            double minvalue = 1.0, maxvalue = 1.0;
            for (int i = 0; i < MAXOCTAVE; ++i)
            {
                minvalue *= -1.0 * _exparray[i] + 1.0;
                maxvalue *= 1.0 * _exparray[i] + 1.0;

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
