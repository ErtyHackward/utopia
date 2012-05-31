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
            /// 1 = X_Plus1;
            /// 2 = X_Minus1;
            /// 3 = Y_Plus1;
            /// 4 = Y_Minus1;
            /// 5 = Z_Plus1;
            /// 6 = Z_Minus1;
            //Create Tree Templates ========================
            //Small Tree
            TreeTemplate smallTreeTemplate = new TreeTemplate()
            {
                TreeType = TreeType.Small,
                Radius = 3,
                TrunkSize = new RangeB(5,6),
                TrunkCubeId = CubeId.Trunk,
                FoliageCubeId = CubeId.Foliage
            };
            smallTreeTemplate.FoliageStructure = new List<List<int>>();
            smallTreeTemplate.FoliageStructure.Add (new List<int>() 
                    { -6,
                      3,1,4,4,2,2,3,3,  -5,-3,
                      -2,1,1,1,-1,4,2,2,2,2,4,1,-1,1,1,4,2,2,2,2,-4,1,1,1,-1,  -5,-3, 
                      -2,2,-2,3,1,1,-3,2
                    }
            );
            Templates[(int)smallTreeTemplate.TreeType] = smallTreeTemplate;

            //Medium Tree
            TreeTemplate mediumTreeTemplate = new TreeTemplate()
            {
                TreeType = TreeType.Medium,
                Radius = 4,
                TrunkSize = new RangeB(5,7),
                TrunkCubeId = CubeId.Trunk,
                FoliageCubeId = CubeId.Foliage
            };
            mediumTreeTemplate.FoliageStructure = new List<List<int>>();
            mediumTreeTemplate.FoliageStructure.Add(new List<int>() 
                    { -6,
                      3,1,4,4,2,2,3,3,  -5,-3,
                      2,1,1,1,1,4,2,2,2,2,4,1,-1,1,1,4,2,2,2,2,4,1,1,1,1,  
                      5,1,-4,2,2,2,2,2,-2,3,1,-2,3,3,3,3,-3,1,4,-3,1,1,1,1,-1,4,2,-1,4,4,4,-2,-2,-2,3
                    }
            );
            Templates[(int)mediumTreeTemplate.TreeType] = mediumTreeTemplate;

            //Big Tree
            TreeTemplate bigTreeTemplate = new TreeTemplate()
            {
                TreeType = TreeType.Big,
                Radius = 5,
                TrunkSize = new RangeB(9, 13),
                TrunkCubeId = CubeId.Trunk,
                FoliageCubeId = CubeId.Foliage
            };
            bigTreeTemplate.FoliageStructure = new List<List<int>>();
            bigTreeTemplate.FoliageStructure.Add(new List<int>() 
                    { -6,-6,-6, -6,
                      3,1,4,4,2,2,3,3,  -5,-3,
                      -2,1,1,1,-1,4,2,2,2,2,4,1,1,1,1,4,2,2,2,2,-4,1,1,1,-1,  -5,-3, 
                      -2,2,-2,3,1,1,-3,2,-4
                    }
            );
            bigTreeTemplate.FoliageStructure.Add(new List<int>() 
                    { -6,
                      3,1,4,4,2,2,3,3,  -5,-3,
                      -2,1,1,1,-1,4,2,2,2,2,4,1,1,1,1,4,2,2,2,2,-4,1,1,1,-1,  -5,-3, 
                      -2,2,-2,3,1,1,-3,2,-4
                    }
            );
            bigTreeTemplate.FoliageStructure.Add(new List<int>() 
                    { 
                      3,1,4,4,2,2,3,3,  -5,-3,
                      -2,1,1,1,-1,4,2,2,2,2,4,1,1,1,1,4,2,2,2,2,-4,1,1,1,-1,  -5,-3, 
                      -2,2,-2,3,1,1,-3,2,-4
                    }
            );
            bigTreeTemplate.FoliageStructure.Add(new List<int>() 
                    { -6,
                      3,1,4,4,2,2,3,3,  -5,-3,
                      -2,1,1,1,-1,4,2,2,2,2,4,1,1,1,1,4,2,2,2,2,-4,1,1,1,-1,  -5,-3, 
                      -2,2,-2,3,1,1,-3,2,-4
                    }
            );
            bigTreeTemplate.FoliageStructure.Add(new List<int>() 
                    { -6,
                      3,1,4,4,2,2,3,3,  -5,-3,
                      -2,1,1,1,-1,4,2,2,2,2,4,1,1,1,1,4,2,2,2,2,-4,1,1,1,-1,  -5,-3, 
                      -2,2,-2,3,1,1,-3,2,-4
                    }
            );
            Templates[(int)bigTreeTemplate.TreeType] = bigTreeTemplate;

            //Cactus 
            TreeTemplate cactusTemplate = new TreeTemplate()
            {
                TreeType = TreeType.Cactus,
                Radius = 2,
                TrunkSize = new RangeB(1, 2),
                TrunkCubeId = CubeId.Cactus,
                FoliageCubeId = CubeId.CactusTop
            };
            cactusTemplate.FoliageStructure = new List<List<int>>();
            cactusTemplate.FoliageStructure.Add(new List<int>() { 5 });
            Templates[(int)cactusTemplate.TreeType] = cactusTemplate;
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
            // 1 = X_Plus1
            // 2 = X_Minus1
            // 3 = Y_Plus1
            // 4 = Y_Minus1
            // 5 = Z_Plus1
            // 6 = Z_Minus1
            public List<List<int>> FoliageStructure;
        }

    }
}
