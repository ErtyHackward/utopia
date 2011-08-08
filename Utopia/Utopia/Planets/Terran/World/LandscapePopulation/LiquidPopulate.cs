using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Planets.Terran.Cube;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Landscaping;

namespace Utopia.Planets.Terran.World.LandscapePopulation
{
    public static class LiquidPopulate
    {
        public static void Generate(LandScape landScape, ref Range<int> workingRange, int MaxChunkDensity,  Random rnd)
        {
            TerraCube cube;
            int cubeIndex;
            int x, y, z;
            int cptAir = 0;
            int cptStone = 0;

            // Try to populate MaxChunkDensity times !
            for (int i = 0; i < MaxChunkDensity; i++)
            {
                //Find point !
                x = workingRange.Min.X + rnd.Next(1,14);
                y = workingRange.Min.Y + rnd.Next(64, 128);
                z = workingRange.Min.Z + rnd.Next(1,14);

                //Does this point satisfy the spawnind condition ?
                cubeIndex = landScape.Index(x, y, z);
                cube = landScape.Cubes[cubeIndex];

                if (cube.Id == CubeId.Stone)
                {
                    var surroundingCube = landScape.GetSurroundingBlocksIndex(cubeIndex, x, y, z);
                    //Block below or Above must be stone
                    if (landScape.Cubes[surroundingCube[4].Index].Id != CubeId.Stone || landScape.Cubes[surroundingCube[5].Index].Id != CubeId.Stone) continue;
                    //I must have one side to free air, and the other one surrended by stone
                    for (int neightb = 0; neightb < 4; neightb++)
                    {
                        cube = landScape.Cubes[surroundingCube[neightb].Index];
                        if (cube.Id == CubeId.Stone) cptStone++;
                        else if (cube.Id == CubeId.Air) cptAir++;
                    }

                    if (cptStone == 3 && cptAir == 1)
                    {
                        landScape.Cubes[cubeIndex] = new TerraCube(CubeId.WaterSource);
                        //Console.Write("New Sources");
                    }
                }

            }
        }
    }
}
