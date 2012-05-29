using System.Collections.Generic;

namespace Utopia.Shared.Cubes
{
    //Helper to find the Cube ID's from label.
    //The cubes are defined in the XML file in UtopiaContent/Models/CubesProfile.xml
    public static class CubeId
    {
        public const byte Air = 0;
        public const byte Stone = 1;
        public const byte Dirt = 2;
        public const byte Grass = 3;
        public const byte WoodPlank = 4;
        public const byte StillWater = 5;
        public const byte DynamicWater = 6;
        public const byte LightWhite = 7;
        public const byte Rock = 8;
        public const byte Sand = 9;
        public const byte Gravel = 10;
        public const byte Trunk = 11;
        public const byte GoldOre = 12;
        public const byte CoalOre = 13;
        public const byte MoonStone = 14;
        public const byte Brick = 15;
        public const byte Foliage = 16;
        public const byte Glass = 17;
        public const byte Snow = 18;
        public const byte Ice = 19;
        public const byte StillLava = 20;
        public const byte DynamicLava = 21;

        // note: when adding new block modify 2 methods below
        public const byte Error = 255;
        

        /// <summary>
        /// Returns name of the cube
        /// </summary>
        /// <param name="cubeId"></param>
        /// <returns></returns>
        public static string GetCubeTypeName(byte cubeId)
        {
            switch (cubeId)
            {
                case 0: return "Air";
                case 1: return "Stone";
                case 2: return "Dirt";
                case 3: return "Grass";
                case 4: return "WoodPlank";
                case 5: return "StillWater";
                case 6: return "DynamicWater";
                case 7: return "LightWhite";
                case 8: return "Rock";
                case 9: return "Sand";
                case 10: return "Gravel";
                case 11: return "Trunk";
                case 12: return "Minerai1";
                case 13: return "Minerai2";
                case 14: return "Minerai3";
                case 15: return "Brick";
                case 16: return "Foliage";
                case 17: return "Glass";
                case 18: return "Snow";
                case 19: return "Ice";
                case 20: return "StillLava";
                case 21: return "DynamicLava";
                case 255: return "Error";
                default: return "Unknown";
            }
        }

        public static string GetCubeDescription(byte cubeId)
        {
            switch (cubeId)
            {
                default:
                    return "Buiding material. Can be placed into the world";
            }
        }

        /// <summary>
        /// Enumerates all blocks except the Error block
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<byte> All()
        {
            yield return Air;
            yield return Stone;
            yield return Dirt;
            yield return Grass;
            yield return WoodPlank;
            yield return StillWater;
            yield return DynamicWater;
            yield return LightWhite;
            yield return Rock;
            yield return Sand;
            yield return Gravel;
            yield return Trunk;
            yield return GoldOre;
            yield return CoalOre;
            yield return MoonStone;
            yield return Brick;
            yield return Foliage;
            yield return Glass;
            yield return Snow;
            yield return Ice;
            yield return StillLava;
            yield return DynamicLava;
        }
    }
}
