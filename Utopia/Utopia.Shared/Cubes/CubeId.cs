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
        public const byte Water = 5;
        public const byte LightRed = 6;
        public const byte LightGreen = 7;
        public const byte LightBlue = 8;
        public const byte LightYellow = 9;
        public const byte LightViolet = 10;
        public const byte LightWhite = 11;
        public const byte Stone2 = 12;
        public const byte Stone3 = 13;
        public const byte Rock = 14;
        public const byte Sand = 15;
        public const byte Gravel = 16;
        public const byte Trunk = 17;
        public const byte Minerai1 = 18;
        public const byte Minerai2 = 19;
        public const byte Minerai3 = 20;
        public const byte Minerai4 = 21;
        public const byte Minerai5 = 22;
        public const byte Stone4 = 23;
        public const byte Minerai6 = 24;
        public const byte Brick = 25;
        public const byte WaterSource = 26;
        public const byte PlayerHead = 27;
        public const byte Leaves = 28;
        public const byte Glass = 29;
        public const byte HalfWoodPlank = 30;
        public const byte QuickStep = 31;

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
                case 5: return "Water";
                case 6: return "LightRed";
                case 7: return "LightGreen";
                case 8: return "LightBlue";
                case 9: return "LightYellow";
                case 10: return "LightViolet";
                case 11: return "LightWhite";
                case 12: return "Stone2";
                case 13: return "Stone3";
                case 14: return "Rock";
                case 15: return "Sand";
                case 16: return "Gravel";
                case 17: return "Trunk";
                case 18: return "Minerai1";
                case 19: return "Minerai2";
                case 20: return "Minerai3";
                case 21: return "Minerai4";
                case 22: return "Minerai5";
                case 23: return "Stone4";
                case 24: return "Minerai6";
                case 25: return "Brick";
                case 26: return "WaterSource";
                case 27: return "PlayerHead";
                case 28: return "Leaves";
                case 29: return "Glass";
                case 30: return "HalfWoodPlank";
                case 31: return "QuickStep";
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
            yield return Water;
            yield return LightRed;
            yield return LightGreen;
            yield return LightBlue;
            yield return LightYellow;
            yield return LightViolet;
            yield return LightWhite;
            yield return Stone2;
            yield return Stone3;
            yield return Rock;
            yield return Sand;
            yield return Gravel;
            yield return Trunk;
            yield return Minerai1;
            yield return Minerai2;
            yield return Minerai3;
            yield return Minerai4;
            yield return Minerai5;
            yield return Stone4;
            yield return Minerai6;
            yield return Brick;
            yield return WaterSource;
            yield return PlayerHead;
            yield return Leaves;
            yield return Glass;
            yield return HalfWoodPlank;
            yield return QuickStep;
        }
    }
}
