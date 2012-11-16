using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S33M3Resources.Structs;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks;

namespace Utopia.Tests
{
    [TestClass]
    public class BBoxOptimizeTest
    {
        [TestMethod]
        public void BBoxOptimize()
        {
            var blocks = new InsideDataProvider();

            blocks.UpdateChunkSize(new Vector3I(5, 5, 5));

            for (int x = 0; x < blocks.ChunkSize.X; x++)
            {
                for (int y = 0; y < blocks.ChunkSize.Y; y++)
                {
                    for (int z = 0; z < blocks.ChunkSize.Z; z++)
                    {
                        blocks.SetBlock(new Vector3I(x, y, z), 1);
                    }
                }
            }

            var list = VoxelMeshFactory.GenerateShapeBBoxes(blocks);

            Assert.AreEqual(1, list.Count);

            for (int x = 0; x < blocks.ChunkSize.X; x++)
            {
                blocks.SetBlock(new Vector3I(x, blocks.ChunkSize.Y - 1, 3), 0);
            }

            list = VoxelMeshFactory.GenerateShapeBBoxes(blocks);

        }
    }
}
