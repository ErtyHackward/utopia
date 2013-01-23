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
                Name = "Beech",
                Axiom = "FFFFFAFA",
                Rules_a = "[&&&F[++^Fd][--&d]//d[+^d][--&d]]////[&&&F[++^Fd][--&d]//d[+^d][--&d]]////[&&&F[++^Fd][--&Fd]//d[+^d][--&d]]",
                Rules_d = "F",
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Single,
                SmallBranches = true
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Apple Tree",
                Axiom = "FFFFFAFFBF",
                Rules_a = "[&&&FFFFF&&FFFF][&&&++++FFFFF&&FFFF][&&&----FFFFF&&FFFF]",
                Rules_b = "[&&&++FFFFF&&FFFF][&&&--FFFFF&&FFFF][&&&------FFFFF&&FFFF]",
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Single,
                SmallBranches = true
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Oak",
                Axiom = "FFFFFFA",
                Rules_a = "[&FFBFA]////[&BFFFA]////[&FBFFA]",
                Rules_b = "[&FFFA]////[&FFFA]////[&FFFA]",
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 5,
                RandomeLevel = 2,
                TrunkType = TrunkType.Crossed,
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Sequoia",
                Axiom = "FFFFFFFFFFddccA///cccFddcFA///ddFcFA/cFFddFcdBddd/A/ccdcddd/ccAddddcFBcccAccFdFcFBcccc/BFdFFcFFdcccc/B",
                Rules_a = "[&&&GGF[++^FFdd][--&Fddd]//Fdd[+^Fd][--&Fdd]]////[&&&GGF[++^FFdd][--&Fddd]//Fdd[+^Fd][--&Fdd]]////[&&&GGF[++^FFdd][--&Fddd]//Fdd[+^Fd][--&Fdd]]",
                Rules_b = "[&&&GGF[++^Fdd][--&Fdd]//dd[+^d][--&Fd]]////[&&&GGF[++^Fdd][--&Fdd]//dd[+^d][--&Fd]]////[&&&GGF[++^Fdd][--&Fdd]//dd[+^d][--&Fd]]",
                Rules_c = "/",
                Rules_d = "F",
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Crossed,
                SmallBranches = true
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Birch 1",
                Axiom = "FFFFFdddccA/FFFFFFcA/FFFFFFcB",
                Rules_a = "[&&&dddd^^ddddddd][&&&---dddd^^ddddddd][&&&+++dddd^^ddddddd][&&&++++++dddd^^ddddddd]",
                Rules_b = "[&&&ddd^^ddddd][&&&---ddd^^ddddd][&&&+++ddd^^ddddd][&&&++++++ddd^^ddddd]",
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

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Birch 2",
                Axiom = "FFFdddccA/FFFFFccA/FFFFFccB",
                Rules_a = "[&&&dFFF^^FFFdd][&&&---dFFF^^FFFdd][&&&+++dFFF^^FFFdd][&&&++++++dFFF^^FFFdd]",
                Rules_b = "[&&&dFF^^FFFd][&&&---dFFF^^FFFd][&&&+++dFF^^FFFd][&&&++++++dFF^^FFFd]",
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

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Palm",
                Axiom = "FFccc&FFFFFdddFA//A//A//A//A//A",
                Rules_a = "[&fb&bbb[++f--&ffff&ff][--f++&ffff&ff]&ffff&bbbb&b]",
                Rules_b = "f",
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

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Spruce 1",
                Axiom = "FFFFFAFFFFFFBFFFFFFCFFFFFFDFFFFFF[&&&F^^FF][&&&++F^^FF][&&&++++F^^FF][&&&++++++F^^FF][&&&--F^^FF][&&&----F^^FF][FFFFf]",
                Rules_a = "[&&&FFFFFF^^FFF][&&&++FFFFFF^^FFF][&&&++++FFFFFF^^FFF][&&&++++++FFFFFF^^FFF][&&&--FFFFFF^^FFF][&&&----FFFFFF^^FFF]",
                Rules_b = "[&&&FFFFF^^FFF][&&&++FFFFF^^FFF][&&&++++FFFFF^^FFF][&&&++++++FFFFF^^FFF][&&&--FFFFF^^FFF][&&&----FFFFF^^FFF]",
                Rules_c = "[&&&FFFF^^FFF][&&&++FFFF^^FFF][&&&++++FFFF^^FFF][&&&++++++FFFF^^FFF][&&&--FFFF^^FFF][&&&----FFFF^^FFF]",
                Rules_d = "[&&&FFF^^FFF][&&&++FFF^^FFF][&&&++++FFF^^FFF][&&&++++++FFF^^FFF][&&&--FFF^^FFF][&&&----FFF^^FFF]",
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Crossed,
                SmallBranches = true
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Spruce 2",
                Axiom = "FFFFFFBFFFFFFCFFFFFFDFFFFFF[&&&F^^FF][&&&++F^^FF][&&&++++F^^FF][&&&++++++F^^FF][&&&--F^^FF][&&&----F^^FF][FFFFf]",
                Rules_b = "[&&&FFFFF^^FFF][&&&++FFFFF^^FFF][&&&++++FFFFF^^FFF][&&&++++++FFFFF^^FFF][&&&--FFFFF^^FFF][&&&----FFFFF^^FFF]",
                Rules_c = "[&&&FFFF^^FFF][&&&++FFFF^^FFF][&&&++++FFFF^^FFF][&&&++++++FFFF^^FFF][&&&--FFFF^^FFF][&&&----FFFF^^FFF]",
                Rules_d = "[&&&FFF^^FFF][&&&++FFF^^FFF][&&&++++FFF^^FFF][&&&++++++FFF^^FFF][&&&--FFF^^FFF][&&&----FFF^^FFF]",
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Crossed,
                SmallBranches = true
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Pine",
                Axiom = "FFFFFcccdddB///cFdFB////cFdFB///cFdFB///cFdFA///cFdFA///cFdFB[FF]f",
                Rules_a = "[&&&TTTT[++^TFdd][--&TFd]//Tdd[+^Fd][--&Fdd]]",
                Rules_b = "[&&&TTT[++^Fdd][--&Fdd]//dd[+^d][--&Fd]]",
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

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Willow",
                Axiom = "FFFFFFFFccA",
                Rules_a = "[&FF&FFFF&&F&FFFFFFFdddd][**&FF&FFFF&&F&FFFFFFFdddd][//&FF&FFFF&&F&FFFFFFFdddd][////&FF&FFFF&&F&FFFFFFFdddd][//////&FF&FFFF&&F&FFFFFFFdddd][////////&FF&FFFF&&F&FFFFFFFdddd]",
                Rules_c = "/",
                Rules_d = "F",
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Crossed,
                SmallBranches = true
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Rubber",
                Axiom = "FFFFA",
                Rules_a = "[&FFBFA]////[&BFFFA]////[&FBFFA]",
                Rules_b = "[&FFA]////[&FFA]////[&FFA]",
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 35,
                Iteration = 3,
                RandomeLevel = 1,
                TrunkType = TrunkType.Double,
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
            if (chunk.Position == new Vector2I(1, 0))
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
            treeType = _treeTemplates[2];


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
