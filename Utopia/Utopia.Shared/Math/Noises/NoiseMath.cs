using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Math.Noises
{
    public interface INoise2D
    {
        NoiseResult GetNoise2DValue(double X, double Z, int octaves, double persistence);
    }

    public interface INoise3D
    {
        NoiseResult GetNoise3DValue(double X, double Y, double Z, int octaves, double persistence);
    }

    public struct NoiseResult
    {
        public bool isSet;
        public double Value;
        public double MinValue;
        public double MaxValue;
    }

    public static class NoiseMath
    {

        public static NoiseResult Sum(NoiseResult NoiseResult1, double WeightNoiseResult1, NoiseResult NoiseResult2, double WeightNoiseResult2)
        {
            return new NoiseResult()
            {
                Value = (NoiseResult1.Value * WeightNoiseResult1) + (NoiseResult2.Value * WeightNoiseResult2),
                MinValue = (NoiseResult1.MinValue * WeightNoiseResult1) + (NoiseResult2.MinValue * WeightNoiseResult2),
                MaxValue = (NoiseResult1.MaxValue * WeightNoiseResult1) + (NoiseResult2.MaxValue * WeightNoiseResult2)
            };
        }

        public static NoiseResult Multiply(NoiseResult NoiseResult1, double WeightNoiseResult1, NoiseResult NoiseResult2, double WeightNoiseResult2)
        {
            return new NoiseResult()
            {
                Value = (NoiseResult1.Value * WeightNoiseResult1) * (NoiseResult2.Value * WeightNoiseResult2),
                MinValue = (NoiseResult1.MinValue * WeightNoiseResult1) * (NoiseResult2.MinValue * WeightNoiseResult2),
                MaxValue = (NoiseResult1.MaxValue * WeightNoiseResult1) * (NoiseResult2.MaxValue * WeightNoiseResult2)
            };
        }

        public static NoiseResult Min(NoiseResult NoiseResult1, NoiseResult NoiseResult2)
        {
            return new NoiseResult()
            {
                Value = NoiseResult1.Value < NoiseResult2.Value ? NoiseResult1.Value : NoiseResult2.Value,
                MinValue = NoiseResult1.MinValue < NoiseResult2.MinValue ? NoiseResult1.MinValue : NoiseResult2.MinValue,
                MaxValue = NoiseResult1.MaxValue < NoiseResult2.MaxValue ? NoiseResult2.MaxValue : NoiseResult1.MaxValue
            };
        }

    }
}
