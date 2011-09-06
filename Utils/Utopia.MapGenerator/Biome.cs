using System;
using System.Drawing;

namespace Utopia.MapGenerator
{
    public enum BiomeType
    {
        None,
        Snow,
        Tundra,
        Bare,
        Scorched,
        Taiga,
        Shrubland,
        TemperateDesert,
        TemperateRainForest,
        TemperateDeciduousForest,
        Grassland,
        TropicalRainForest,
        TropicalSeasonalForest,
        SubtropicalDesert
    }

    public class Biome
    {
        public Biome(ParameterVariation elevation, ParameterVariation moisture)
        {
            Elevation = elevation;
            Moisture = moisture;
        }

        public ParameterVariation Elevation { get; set; }

        public ParameterVariation Moisture { get; set; }

        public BiomeType GetBiomeWith(int elevation, int moisture)
        {
            if (!Elevation.Contains(elevation))
                throw new ArgumentException("Invalid elevation value");
            if(!Moisture.Contains(moisture))
                throw new ArgumentException("Invalid moisture value");

            var elevationPercent = Elevation.GetPercent(elevation);
            var moisturePercent = Moisture.GetPercent(moisture);

            if (elevationPercent >= 0 && elevationPercent < 25)
            {
                if (moisturePercent >= 0 && moisturePercent < 17)
                {
                    return BiomeType.SubtropicalDesert;
                }
                if (moisturePercent >= 17 && moisturePercent < 33)
                {
                    return BiomeType.Grassland;
                }
                if (moisturePercent >= 33 && moisturePercent < 66)
                {
                    return BiomeType.TropicalSeasonalForest;
                }
                return BiomeType.TropicalRainForest;
            }
            if (elevationPercent >= 25 && elevationPercent < 50)
            {
                if (moisturePercent >= 0 && moisturePercent < 17)
                {
                    return BiomeType.TemperateDesert;
                }
                if (moisturePercent >= 17 && moisturePercent < 50)
                {
                    return BiomeType.Grassland;
                }
                if (moisturePercent >= 50 && moisturePercent < 83)
                {
                    return BiomeType.TemperateDeciduousForest;
                }
                return BiomeType.TropicalRainForest;
            }
            if (elevationPercent >= 50 && elevationPercent < 75)
            {
                if (moisturePercent >= 0 && moisturePercent < 33)
                {
                    return BiomeType.TemperateDesert;
                }
                if (moisturePercent >= 33 && moisturePercent < 66)
                {
                    return BiomeType.Shrubland;
                }
                return BiomeType.Taiga;
            }
            if (elevationPercent >= 75 && elevationPercent <= 100)
            {
                if (moisturePercent >= 0 && moisturePercent < 17)
                {
                    return BiomeType.Scorched;
                }
                if (moisturePercent >= 17 && moisturePercent < 33)
                {
                    return BiomeType.Bare;
                }
                if (moisturePercent >= 33 && moisturePercent < 50)
                {
                    return BiomeType.Tundra;
                }
                return BiomeType.Snow;
            }

            return BiomeType.None;
        }

        /// <summary>
        /// Return color for specified biome type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Color GetBiomeColor(BiomeType type)
        {
            switch (type)
            {
                case BiomeType.None: return Color.Red;
                case BiomeType.Snow: return Color.White;
                case BiomeType.Tundra: return Color.FromArgb(221, 221, 187);
                case BiomeType.Bare: return Color.FromArgb(187, 187, 187);
                case BiomeType.Scorched: return Color.FromArgb(153, 153, 153);
                case BiomeType.Taiga: return Color.FromArgb(204, 212, 187);
                case BiomeType.Shrubland: return Color.FromArgb(196, 204, 187);
                case BiomeType.TemperateDesert: return Color.FromArgb(228,232,202);
                case BiomeType.TemperateRainForest: return Color.FromArgb(164, 196, 168);
                case BiomeType.TemperateDeciduousForest: return Color.FromArgb(180, 201, 196);
                case BiomeType.Grassland: return Color.FromArgb(196, 212, 170);
                case BiomeType.TropicalRainForest: return Color.FromArgb(156, 187, 169);
                case BiomeType.TropicalSeasonalForest: return Color.FromArgb(169, 204, 164);
                case BiomeType.SubtropicalDesert: return Color.FromArgb(233, 221, 199);
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }

    public struct ParameterVariation
    {
        private int _maximum;
        private int _minimum;

        public int Minimum
        {
            get { return _minimum; }
            set { _minimum = value; }
        }

        public int Maximum
        {
            get { return _maximum; }
            set { _maximum = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimum">Inclusive minimum</param>
        /// <param name="maximum">Inclusive maximum</param>
        public ParameterVariation(int minimum, int maximum)
        {
            _minimum = minimum;
            _maximum = maximum;
        }
        
        public bool Contains(int value)
        {
            return Minimum <= value && value <= Maximum;
        }

        public int GetPercent(int elevation)
        {
            return (int) (100 * ((double)elevation - Minimum) / (Maximum - Minimum));
        }
    }
}
