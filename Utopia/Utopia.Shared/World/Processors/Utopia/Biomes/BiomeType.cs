using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public static class BiomeType
    {
        public const byte Grassland = 0;
        public const byte Desert = 1;
        public const byte Forest = 2;
        public const byte Ocean = 3;
        public const byte Montain = 4;
        public const byte Plain = 5;

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