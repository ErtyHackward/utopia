using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;
using Utopia.Shared.Cubes;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public static class TreeTemplates
    {
        public static readonly TreeTemplate[] Templates = new TreeTemplate[4];

        public enum TreeType
        {
            Small = 0,
            Medium = 1,
            Big = 2,
            Cactus = 3
        }

        static TreeTemplates()
        {
            //Create Tree Templates ========================
            //Small Tree
            TreeTemplate smallTreeTemplate = new TreeTemplate()
            {
                TreeType = TreeType.Small,
                Radius = 3,
                TrunkSize = new RangeB(4,5),
                TrunkCubeId = CubeId.Trunk,
                FoliageCubeId = CubeId.Foliage
            };
            smallTreeTemplate.FoliageStructure = new List<int>() 
                    { 5,1,6,6,2,2,5,5,  -3,-5,
                      2,1,1,1,1,6,2,2,2,2,6,1,1,1,1,6,2,2,2,2,6,1,1,1,1,  -3,-5, 
                      2,2,2,5,-1,1,5,2,2
                    };
            Templates[(int)TreeType.Small] = smallTreeTemplate;

            //Medium Tree
            TreeTemplate mediumTreeTemplate = new TreeTemplate()
            {
                TreeType = TreeType.Small,
                Radius = 3,
                TrunkSize = new RangeB(4, 5),
                TrunkCubeId = CubeId.Trunk,
                FoliageCubeId = CubeId.Foliage
            };
            mediumTreeTemplate.FoliageStructure = new List<int>() 
                    { 5,1,6,6,2,2,5,5,  -3,-5,
                      2,1,1,1,1,6,2,2,2,2,6,1,1,1,1,6,2,2,2,2,6,1,1,1,1,  -3,-5, 
                      2,2,2,5,-1,1,5,2,2
                    };
            Templates[(int)TreeType.Medium] = mediumTreeTemplate;

            //Big Tree
            TreeTemplate bigTreeTemplate = new TreeTemplate()
            {
                TreeType = TreeType.Small,
                Radius = 3,
                TrunkSize = new RangeB(4, 8),
                TrunkCubeId = CubeId.Trunk,
                FoliageCubeId = CubeId.Foliage
            };
            bigTreeTemplate.FoliageStructure = new List<int>() 
                    { 5,1,6,6,2,2,5,5,  -3,-5,
                      2,1,1,1,1,6,2,2,2,2,6,1,1,1,1,6,2,2,2,2,6,1,1,1,1,  -3,-5, 
                      2,2,2,5,-1,1,5,2,2
                    };
            Templates[(int)TreeType.Big] = bigTreeTemplate;

            //Cactus 
            TreeTemplate cactusTemplate = new TreeTemplate()
            {
                TreeType = TreeType.Small,
                Radius = 3,
                TrunkSize = new RangeB(3, 5),
                TrunkCubeId = CubeId.Trunk,
                FoliageCubeId = CubeId.Foliage
            };
            cactusTemplate.FoliageStructure = new List<int>() { };
            Templates[(int)TreeType.Cactus] = cactusTemplate;
        }

        public class TreeTemplate
        {
            public TreeType TreeType;
            public int Radius;
            public RangeB TrunkSize;
            public byte TrunkCubeId;
            public byte FoliageCubeId;
            //Foliage structure construction, always as Offset from last trunk block position
            // Move the cursor in the chunk
            // 0 = X_Plus1
            // 1 = X_Minus1
            // 2 = Y_Plus1
            // 3 = Y_Minus1
            // 4 = Z_Plus1
            // 5 = Z_Minus1
            public List<int> FoliageStructure;
        }

    }
}
