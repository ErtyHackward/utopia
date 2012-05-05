using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise.Generator;

namespace S33M3CoreComponents.Noise.Fractal
{
    public class FractalHybridMulti : FractalBase, INoise
    {
        #region Private Variables
        #endregion

        #region Public Properties
        #endregion

        public FractalHybridMulti(INoise source, int NumOctaves, double Frequency, enuBaseNoiseRange fractalOutputRange = enuBaseNoiseRange.MinOneToOne)
            :base(source, fractalOutputRange)
        {
            _frequency = Frequency;
            _numOctaves = NumOctaves;
            _lacunarity = 2.0;
            _h = 0.25;
            _gain = 1.0;
            _offset = 0.7;
            _withValueRemap = true;

            CalcWeights();
        }

        #region Public Methods
        public double Get(double x, double y)
        {
            double value, signal, weight;
            x*=_frequency;
            y*=_frequency;


            value = _source.Get(x,y) + _offset;
            weight = _gain * value;
            x*=_lacunarity;
            y*=_lacunarity;

            for(int i=1; i< _numOctaves; ++i)
            {
                if(weight>1.0) weight=1.0;
                signal = (_source.Get(x,y) + _offset) * _exparray[i];
                value += weight*signal;
                weight *=_gain * signal;
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
            double value, signal, weight;
            x *= _frequency;
            y *= _frequency;
            z *= _frequency;


            value = _source.Get(x, y, z) + _offset;
            weight = _gain * value;
            x *= _lacunarity;
            y *= _lacunarity;
            z *= _lacunarity;

            for (int i = 1; i < _numOctaves; ++i)
            {
                if (weight > 1.0) weight = 1.0;
                signal = (_source.Get(x, y, z) + _offset) * _exparray[i];
                value += weight * signal;
                weight *= _gain * signal;
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
            double value, signal, weight;
            x *= _frequency;
            y *= _frequency;
            z *= _frequency;
            w *= _frequency;

            value = _source.Get(x, y, z, w) + _offset;
            weight = _gain * value;
            x *= _lacunarity;
            y *= _lacunarity;
            z *= _lacunarity;
            w *= _lacunarity;

            for (int i = 1; i < _numOctaves; ++i)
            {
                if (weight > 1.0) weight = 1.0;
                signal = (_source.Get(x, y, z, w) + _offset) * _exparray[i];
                value += weight * signal;
                weight *= _gain * signal;
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
            double weightmin, weightmax;
            double A = -1.0, B = 1.0, scale, bias;

            minvalue = _offset - 1.0;
            maxvalue = _offset + 1.0;
            weightmin = _gain * minvalue;
            weightmax = _gain * maxvalue;

            scale = (B - A) / (maxvalue - minvalue);
            bias = A - minvalue * scale;
            _fractalRemap[0].Bias = bias;
            _fractalRemap[0].Scale = scale;


            for (int i = 1; i < MAXOCTAVE; ++i)
            {
                if (weightmin > 1.0) weightmin = 1.0;
                if (weightmax > 1.0) weightmax = 1.0;

                double signal = (_offset - 1.0) * _exparray[i];
                minvalue += signal * weightmin;
                weightmin *= _gain * signal;

                signal = (_offset + 1.0) * _exparray[i];
                maxvalue += signal * weightmax;
                weightmax *= _gain * signal;


                scale = (B - A) / (maxvalue - minvalue);
                bias = A - minvalue * scale;
                _fractalRemap[i].Bias = bias;
                _fractalRemap[i].Scale = scale;
            }
        }
        #endregion
    }
}
