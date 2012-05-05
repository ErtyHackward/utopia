using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise.Generator;

namespace S33M3CoreComponents.Noise.Fractal
{
    public abstract class FractalBase
    {
        protected struct FractalRemap
        {
            public double Scale;
            public double Bias;
        }

        #region Private variable
        protected const int MAXOCTAVE = 20;
        protected FractalRemap[] _fractalRemap;
        protected bool _withValueRemap;
        protected int _numOctaves;
        protected INoise _source;
        protected double _frequency;
        protected double _lacunarity;
        protected double _h;
        protected double _offset;
        protected double _gain;
        protected double[] _exparray;
        protected enuBaseNoiseRange _defaultRange;
        #endregion

        #region Public properties
        public INoise Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public double Frequency
        {
            get { return _frequency; }
            set { _frequency = value; }
        }

        public double Lacunarity
        {
            get { return _lacunarity; }
            set
            {
                if (_lacunarity != value)
                {
                    _lacunarity = value;
                    CalcWeights();
                }
            }
        }

        public double H
        {
            get { return _h; }
            set
            {
                if (_h != value)
                {
                    _h = value;
                    CalcWeights();
                }
            }
        }

        public bool WithValueRemap
        {
            get { return _withValueRemap; }
            set { _withValueRemap = value; }
        }

        public int NumOctaves
        {
            get { return _numOctaves; }
            set
            {
                if (value < MAXOCTAVE)
                {
                    _numOctaves = value;
                }
            }
        }
        #endregion

        public FractalBase(INoise source, enuBaseNoiseRange defaultRange)
        {
            _defaultRange = defaultRange;
            _source = source;
            _fractalRemap = new FractalRemap[MAXOCTAVE];
        }

        #region Public methods
        #endregion

        #region Private methods
        protected abstract void CalcWeights();
        #endregion

    }
}
