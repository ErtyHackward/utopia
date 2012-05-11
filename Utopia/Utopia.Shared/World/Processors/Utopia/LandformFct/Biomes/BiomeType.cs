using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.World.Processors.Utopia.LandformFct.Biomes
{
    public static class BiomeType
    {
        public const byte Snow = 0;
        public const byte Tundra = 1;
        public const byte Bare = 2;
        public const byte Scorched = 3;
        public const byte Taiga = 4;
        public const byte Shrubland = 5;
        public const byte TemperateDesert = 6;
        public const byte TemperateRainForest = 7;
        public const byte TemperateDeciduousForest = 8;
        public const byte Grassland = 9;
        public const byte TropicalRainForest = 10;
        public const byte TropicalSeasonalForest = 11;
        public const byte SubtropicalDesert = 12;

        public static readonly Dictionary<byte, string> BiomeTypesCollection;

        public static string GetBiomeName(byte Id)
        {
            string result = "Error";
            BiomeTypesCollection.TryGetValue(Id, out result);
            return result;
        }

        static BiomeType()
        {
            BiomeTypesCollection = new Dictionary<byte, string>();

            //Get the list of all biomes type
            foreach(var fieldInfo in typeof(BiomeType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(byte))
                {
                    BiomeTypesCollection.Add((byte)fieldInfo.GetValue(null), fieldInfo.Name);
                }
            }
        }
    }
}