using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Configuration;
using Utopia.Shared.LandscapeEntities;
using Utopia.Shared.LandscapeEntities.Trees;
using Utopia.Shared.World.Processors.Utopia.Biomes;

namespace Utopia.Shared.World.Processors.Utopia
{
    public class LandscapeEntities
    {
        #region Private Variables
        private TreeLSystem _treeGenerator = new TreeLSystem();
        private List<TreeTemplate> _treeTemplates = new List<TreeTemplate>();
        #endregion

        #region Public Properties
        #endregion

        public LandscapeEntities()
        {
            _treeTemplates.Add(new TreeTemplate()
            {
                Axiom = "FFFFFBFB",
                Rules_a = "[&&&GGF[++^Fd][--&Fd]//Fd[+^Fd][--&Fd]]////[&&&GGF[++^Fd][--&Fd]//Fd[+^Fd][--&Fd]]////[&&&GGF[++^Fd][--&Fd]//Fd[+^Fd][--&Fdd]]",
                Rules_b = "[&&&F[++^Fd][--&d]//d[+^d][--&d]]////[&&&F[++^Fd][--&d]//d[+^d][--&d]]////[&&&F[++^Fd][--&Fd]//d[+^d][--&d]]",
                Rules_c = "/",
                Rules_d = "F",
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Single,
                SmallBranches = true
            });
            

        }

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
            //int nbrTree = rnd.Next(biome.BiomeTrees.TreePerChunks.Min, biome.BiomeTrees.TreePerChunks.Max + 1);
            //for (int i = 0; i < nbrTree; i++)
            //{
            //    PopulateChunkWithTree(cursor, chunk, biome.BiomeTrees, columndInfo, rnd);
            //}
            if (chunk.Position == new Vector2I(0, 0))
                PopulateChunkWithTree(cursor, chunk, columndInfo, rnd);

        }

        private void PopulateChunkWithTree(ByteChunkCursor cursor, GeneratedChunk chunk, ChunkColumnInfo[] columndInfo, FastRandom rnd)
        {
            //Get Rnd chunk Location.
            int x = rnd.Next(0, 16);
            int z = rnd.Next(0, 16);
            x = 7;
            z = 7;
            int y = columndInfo[x * AbstractChunk.ChunkSize.Z + z].MaxGroundHeight + 1;
            x += (chunk.Position.X * AbstractChunk.ChunkSize.X);
            z += (chunk.Position.Y * AbstractChunk.ChunkSize.Z);
            Vector3I worldPosition = new Vector3I(x, y, z);

            //Generate Tree mesh !
            //TreeLSystem generator = _treeGenerator[rnd.Next(0, 5)];
            TreeTemplate treeType = _treeTemplates[rnd.Next(0, _treeTemplates.Count)];


            foreach (var chunkMesh in LandscapeEntityParser.GlobalMesh2ChunkMesh(_treeGenerator.Generate(rnd, worldPosition, treeType)))
            {
                if (chunkMesh.ChunkLocation == chunk.Position)
                {
                    foreach (var block in chunkMesh.Blocks)
                    {
                        cursor.SetInternalPosition(block.WorldPosition);
                        byte blockId = cursor.Read();
                        if (blockId == UtopiaProcessorParams.CubeId.Air) cursor.Write(block.BlockId);
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
