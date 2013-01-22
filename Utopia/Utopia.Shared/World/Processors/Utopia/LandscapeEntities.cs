using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Configuration;
using Utopia.Shared.LandscapeEntities;
using Utopia.Shared.World.Processors.Utopia.Biomes;

namespace Utopia.Shared.World.Processors.Utopia
{
    public class LandscapeEntities
    {
        #region Private Variables
        private TreeLSystem[] _treeGenerator = new TreeLSystem[5];
        #endregion

        #region Public Properties
        #endregion

        public LandscapeEntities()
        {
            //Pine
            Dictionary<char, double> probs = new Dictionary<char,double>();
            probs.Add('A', 0.5);
            probs.Add('B', 0.4);

            Dictionary<char, string> rules = new Dictionary<char, string>();
            rules.Add('A', "[&FFFFFA]////[&FFFFFA]////[&FFFFFA]");
            _treeGenerator[0] = new TreeLSystem("FFFFAFFFFFFFAFFFFA", rules, probs, 4, 35, UtopiaProcessorParams.CubeId.Trunk, UtopiaProcessorParams.CubeId.Foliage);

            rules = new Dictionary<char, string>();
            rules.Add('A', "[&FFBFA]////[&BFFFA]////[&FBFFAFFA]");
            rules.Add('B', "[&FFFAFFFF]////[&FFFAFFF]////[&FFFAFFAA]");
            _treeGenerator[1] = new TreeLSystem("FFFFFFA", rules, probs, 4, 35, UtopiaProcessorParams.CubeId.Trunk, UtopiaProcessorParams.CubeId.Foliage);

            rules = new Dictionary<char, string>();
            rules.Add('A', "[&FFFAFFF]////[&FFAFFF]////[&FFFAFFF]");
            rules.Add('B', "[&FAF]////[&FAF]////[&FAF]");
            _treeGenerator[2] = new TreeLSystem("FFFFAFFFFBFFFFAFFFFBFFFFAFFFFBFF", rules, probs, 4, 35, UtopiaProcessorParams.CubeId.Trunk, UtopiaProcessorParams.CubeId.Foliage);

            rules = new Dictionary<char, string>();
            rules.Add('A', "[&FFBFA]////[&BFFFA]////[&FBFFA]");
            rules.Add('B', "[&FFFA]////[&FFFA]////[&FFFA]");
            _treeGenerator[3] = new TreeLSystem("FFFFFFA", rules, probs, 4, 35, UtopiaProcessorParams.CubeId.Trunk, UtopiaProcessorParams.CubeId.Foliage);

            rules = new Dictionary<char, string>();
            rules.Add('A', "[&FFAFF]////[&FFAFF]////[&FFAFF]");
            _treeGenerator[4] = new TreeLSystem("FFFFFAFAFAF", rules, probs, 4, 40, UtopiaProcessorParams.CubeId.Trunk, UtopiaProcessorParams.CubeId.Foliage);
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
            TreeLSystem generator = _treeGenerator[rnd.Next(0, 5)];

            foreach (var chunkMesh in LandscapeEntityParser.GlobalMesh2ChunkMesh(generator.Generate(rnd, worldPosition)))
            {
                if (chunkMesh.ChunkLocation == chunk.Position)
                {
                    foreach (var block in chunkMesh.Blocks)
                    {
                        cursor.SetInternalPosition(block.WorldPosition);
                        byte blockId = cursor.Read();
                        if (blockId == UtopiaProcessorParams.CubeId.Air || blockId == UtopiaProcessorParams.CubeId.Trunk) cursor.Write(block.BlockId);
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
