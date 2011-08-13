using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;
using Utopia.Shared.Math;

namespace Utopia.Shared.Landscaping
{
    public class FlatLandscape : LandscapeBuilder
    {

        public override void Initialize(int worldSize)
        {
            //Get the number of visible chunks from the config size 
            ChunkGridSize = worldSize;
            ChunkGridSurface = ChunkGridSize * ChunkGridSize;
            Worldsize = new Location3<int>(Chunksize * ChunkGridSize, WorldHeight, Chunksize * ChunkGridSize); 

        }


        protected override void CreateChunkLandscape(byte[] Cubes, Structs.Landscape.TerraCube[] TerraCubes, ref Structs.Range<int> workingRange, bool withRangeClearing)
        {
          //  if (withRangeClearing) ClearRangeArea(Cubes, TerraCubes, ref workingRange);

            int floor = 93;//for testing portals, made ground level with grassland
            //chunk.UpperGroundHeight = floor; //this one is useful for populators like itempopulator
            for (int x = workingRange.Min.X; x < workingRange.Max.X; x++) //X
            {
                for (int z = workingRange.Min.Z; z < workingRange.Max.Z; z++) //Z
                {
                    for (int y = workingRange.Min.Y; y < workingRange.Max.Y; y++) //X
                    {

                        byte block = CubeId.Air;

                        int xAbs = System.Math.Abs(x);
                        int yAbs = System.Math.Abs(y);
                        int zAbs = System.Math.Abs(z);



                        if (y < workingRange.Max.Y / 4)
                            block = CubeId.Rock;//TODO was lava but lava has special rendering now
                        /*
                         * else if (y == (sizeY / 2) - 1) // test caves visibility 
                         * block.Type = Type.empty;
                         */
                        else if (y < floor)
                            block = CubeId.Rock;
                        else if (y == floor)
                        {
                            // block = alternateGroundPerchunk(chunk, block);
                            if (x == 1) block = (byte)(zAbs % 5 + 1);
                            else if (z == workingRange.Max.Z - 2) block = (byte)(xAbs % 5 + 1);
                            else block = CubeId.Sand;
                        }
                        else
                        {
                            if (y == floor + 1 && (x == 0 || x == workingRange.Max.X - 1 || z == 0 || z == workingRange.Max.Z - 1))
                                block = CubeId.Brick;
                            else
                                block = CubeId.Air;
                        }

                        // byte h = (byte)(chunk.Index.Z % 2 == 0 && y > 1 ? y - 1 : y); //stairs for debugging corner cases with water for example

                        if (block > 26){
                        System.Diagnostics.Debugger.Break();
                        }

                        if (TerraCubes != null) TerraCubes[RenderIndex(x, y, z)].Id = block;
                        if (Cubes != null)
                        {
                            Cubes[x * workingRange.Max.X * workingRange.Max.Y + y * workingRange.Max.Y + z] = block;
                        }

                    }
                }
            }

        }

    }
}
