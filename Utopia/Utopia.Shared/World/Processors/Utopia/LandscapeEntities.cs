using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.LandscapeEntities;
using Utopia.Shared.World.Processors.Utopia.Biomes;

namespace Utopia.Shared.World.Processors.Utopia
{
    public class LandscapeEntities
    {
        #region Private Variables
        private TreeGenerator _treeGenerator = new TreeGenerator();
        #endregion

        #region Public Properties
        #endregion

        #region Public Methods
        public void GenerateChunkItems(ByteChunkCursor cursor, GeneratedChunk chunk, Biome biome, ChunkColumnInfo[] columndInfo, FastRandom chunkRnd)
        {
            //Generate landscape trees
            TreeGeneration(cursor, chunk, biome, columndInfo, chunkRnd);
        }
        #endregion

        #region Private Methods
        private void TreeGeneration(ByteChunkCursor cursor, GeneratedChunk chunk, Biome biome, ChunkColumnInfo[] columndInfo, FastRandom rnd)
        {
            int nbrTree = rnd.Next(biome.BiomeTrees.TreePerChunks.Min, biome.BiomeTrees.TreePerChunks.Max + 1);
            for (int i = 0; i < nbrTree; i++)
            {
                PopulateChunkWithTree(cursor, chunk, biome.BiomeTrees, columndInfo, rnd);
            }
        }

        private void PopulateChunkWithTree(ByteChunkCursor cursor, GeneratedChunk chunk, BiomeTrees biomeTrees, ChunkColumnInfo[] columndInfo, FastRandom rnd)
        {
            var treeTemplate = TreeTemplates.Templates[(int)biomeTrees.GetNextTreeType(rnd)];

            //Get Rnd chunk Location.
            int x = rnd.Next(0, 16);
            int z = rnd.Next(0, 16);
            int y = columndInfo[x * AbstractChunk.ChunkSize.Z + z].MaxGroundHeight + 1;
            x += (chunk.Position.X * AbstractChunk.ChunkSize.X);
            z += (chunk.Position.Y * AbstractChunk.ChunkSize.Z);
            Vector3I worldPosition = new Vector3I(x, y, z);

            //Generate Tree mesh !
            foreach (var chunkMesh in LandscapeEntityParser.GlobalMesh2ChunkMesh(_treeGenerator.GenerateMesh(worldPosition, treeTemplate.TrunkCubeId, treeTemplate.FoliageCubeId, rnd)))
            {
                if (chunkMesh.ChunkLocation == chunk.Position)
                {
                    foreach (var block in chunkMesh.Blocks)
                    {
                        cursor.SetInternalPosition(block.WorldPosition);
                        cursor.Write(block.BlockId);
                    }
                }
                else
                {
                    //Send this Mesh to the landscape entity Manager ! This part of the mesh must be constructed inside another chunk !
                }
            }
        }
        #endregion
    }
}
