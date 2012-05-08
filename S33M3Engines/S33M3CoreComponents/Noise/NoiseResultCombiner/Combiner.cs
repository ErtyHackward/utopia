using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.ResultCombiner
{
    public class Combiner : INoise
    {
        public enum CombinerType
        {
            Add,
            Multiply,
            Max,
            Min,
            Avg
        }

        #region Private Variables
        private INoise _source;
        private CombinerType _combinerType;
        private List<INoise> _noises;
        #endregion

        #region Public Properties
        public List<INoise> Noises
        {
            get { return _noises; }
        }
        #endregion

        public Combiner(CombinerType combinerType)
        {
            _noises = new List<INoise>();
            _combinerType = combinerType;
        }

        #region Public Methods
        public double Get(double x, double y)
        {
            switch (_combinerType)
            {
                case CombinerType.Add:
                    return AddGet(x, y);
                case CombinerType.Multiply:
                    return MultiplyGet(x, y);
                case CombinerType.Max:
                    return MaxGet(x, y);                    
                case CombinerType.Min:
                    return MinGet(x, y);
                case CombinerType.Avg:
                    return AvgGet(x, y);
                default:
                    return 0.0;
            }
        }

        public double Get(double x, double y, double z)
        {
            switch (_combinerType)
            {
                case CombinerType.Add:
                    return AddGet(x, y, z);
                case CombinerType.Multiply:
                    return MultiplyGet(x, y, z);
                case CombinerType.Max:
                    return MaxGet(x, y, z);
                case CombinerType.Min:
                    return MinGet(x, y, z);
                case CombinerType.Avg:
                    return AvgGet(x, y, z);
                default:
                    return 0.0;
            }
        }

        public double Get(double x, double y, double z, double w)
        {
            switch (_combinerType)
            {
                case CombinerType.Add:
                    return AddGet(x, y, z, w);
                case CombinerType.Multiply:
                    return MultiplyGet(x, y, z, w);
                case CombinerType.Max:
                    return MaxGet(x, y, z, w);
                case CombinerType.Min:
                    return MinGet(x, y, z, w);
                case CombinerType.Avg:
                    return AvgGet(x, y, z, w);
                default:
                    return 0.0;
            }
        }

        #endregion

        #region Private Methods
        #region Add
        private double AddGet(double x, double y)
        {
            double result = 0.0;
            for (int i = 0; i < _noises.Count; i++)
            {
                result += _noises[i].Get(x, y);
            }
            return result;
        }

        private double AddGet(double x, double y, double z)
        {
            double result = 0.0;
            for (int i = 0; i < _noises.Count; i++)
            {
                result += _noises[i].Get(x, y, z);
            }
            return result;
        }

        private double AddGet(double x, double y, double z, double w)
        {
            double result = 0.0;
            for (int i = 0; i < _noises.Count; i++)
            {
                result += _noises[i].Get(x, y, z, w);
            }
            return result;
        }
        #endregion

        #region Multiply
        private double MultiplyGet(double x, double y)
        {
            double result = 1.0;
            for (int i = 0; i < _noises.Count; i++)
            {
                result *= _noises[i].Get(x, y);
            }
            return result;
        }

        private double MultiplyGet(double x, double y, double z)
        {
            double result = 1.0;
            for (int i = 0; i < _noises.Count; i++)
            {
                result *= _noises[i].Get(x, y, z);
            }
            return result;
        }

        private double MultiplyGet(double x, double y, double z, double w)
        {
            double result = 1.0;
            for (int i = 0; i < _noises.Count; i++)
            {
                result *= _noises[i].Get(x, y, z, w);
            }
            return result;
        }
        #endregion

        #region Min
        private double MinGet(double x, double y)
        {
            double minValue = 0.0;
            if (_noises.Count > 0) minValue = _noises[0].Get(x, y);

            for (int i = 1; i < _noises.Count; i++)
            {
                double value = _noises[i].Get(x, y);
                if (value < minValue) minValue = value;
            }

            return minValue;
        }

        private double MinGet(double x, double y, double z)
        {
            double minValue = 0.0;
            if (_noises.Count > 0) minValue = _noises[0].Get(x, y, z);

            for (int i = 1; i < _noises.Count; i++)
            {
                double value = _noises[i].Get(x, y, z);
                if (value < minValue) minValue = value;
            }

            return minValue;
        }

        private double MinGet(double x, double y, double z, double w)
        {
            double minValue = 0.0;
            if (_noises.Count > 0) minValue = _noises[0].Get(x, y, z, w);

            for (int i = 1; i < _noises.Count; i++)
            {
                double value = _noises[i].Get(x, y, z, w);
                if (value < minValue) minValue = value;
            }

            return minValue;
        }
        #endregion

        #region Max
        private double MaxGet(double x, double y)
        {
            double maxValue = 0.0;
            if (_noises.Count > 0) maxValue = _noises[0].Get(x, y);

            for (int i = 1; i < _noises.Count; i++)
            {
                double value = _noises[i].Get(x, y);
                if (value > maxValue) maxValue = value;
            }

            return maxValue;
        }

        private double MaxGet(double x, double y, double z)
        {
            double maxValue = 0.0;
            if (_noises.Count > 0) maxValue = _noises[0].Get(x, y, z);

            for (int i = 1; i < _noises.Count; i++)
            {
                double value = _noises[i].Get(x, y, z);
                if (value > maxValue) maxValue = value;
            }

            return maxValue;
        }

        private double MaxGet(double x, double y, double z, double w)
        {
            double maxValue = 0.0;
            if (_noises.Count > 0) maxValue = _noises[0].Get(x, y, z, w);

            for (int i = 1; i < _noises.Count; i++)
            {
                double value = _noises[i].Get(x, y, z, w);
                if (value > maxValue) maxValue = value;
            }

            return maxValue;
        }
        #endregion

        #region Avg
        private double AvgGet(double x, double y)
        {
            double result = 0.0;
            if (_noises.Count == 0) return result;
            for (int i = 0; i < _noises.Count; i++)
            {
                result += _noises[i].Get(x, y);
            }
            return result / _noises.Count;
        }

        private double AvgGet(double x, double y, double z)
        {
            double result = 0.0;
            if (_noises.Count == 0) return result;
            for (int i = 0; i < _noises.Count; i++)
            {
                result += _noises[i].Get(x, y, z);
            }
            return result / _noises.Count;
        }

        private double AvgGet(double x, double y, double z, double w)
        {
            double result = 0.0;
            if (_noises.Count == 0) return result;
            for (int i = 0; i < _noises.Count; i++)
            {
                result += _noises[i].Get(x, y, z, w);
            }
            return result / _noises.Count;
        }
        #endregion
        #endregion
    }
}
