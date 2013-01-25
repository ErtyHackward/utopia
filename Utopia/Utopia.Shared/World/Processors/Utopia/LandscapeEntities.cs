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
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.World.Processors.Utopia.Biomes;

namespace Utopia.Shared.World.Processors.Utopia
{
    public class LandscapeEntities
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private TreeLSystem _treeGenerator = new TreeLSystem();
        private List<TreeTemplate> _treeTemplates = new List<TreeTemplate>();
        private LandscapeEntityManager _entityManager;
        #endregion

        #region Public Properties
        #endregion

        public LandscapeEntities(LandscapeEntityManager entityManager)
        {
            _entityManager = entityManager;

            #region Templates
            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Tree 1",
                Axiom = "FFFFAFFFFFFFAFFFFA",
                Rules_a = new LSystemRule() { Rule = "[&FFFFFA]////[&FFFFFA]////[&FFFFFA]", Prob = 0.5f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 35,
                Iteration = 4,
                RandomeLevel = 0,
                TrunkType = TrunkType.Crossed,
                SmallBranches = true,
                FoliageGenerationStart = 2
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Tree 2",
                Axiom = "FFFFFFA",
                Rules_a = new LSystemRule() { Rule = "[&FFBFA]////[&BFFFA]////[&FBFFAFFA]", Prob = 0.5f },
                Rules_b = new LSystemRule() { Rule = "[&FFFAFFFF]////[&FFFAFFF]////[&FFFAFFAA]", Prob = 0.4f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 35,
                Iteration = 4,
                RandomeLevel = 0,
                TrunkType = TrunkType.Double,
                SmallBranches = true,
                FoliageGenerationStart = 2
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Tree 3",
                Axiom = "FFFFAFFFFBFFFFAFFFFBFFFFAFFFFBFF",
                Rules_a = new LSystemRule() { Rule = "[&FFFAFFF]////[&FFAFFF]////[&FFFAFFF]", Prob = 0.5f },
                Rules_b = new LSystemRule() { Rule = "[&FAF]////[&FAF]////[&FAF]", Prob = 0.4f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 35,
                Iteration = 4,
                RandomeLevel = 0,
                TrunkType = TrunkType.Crossed,
                SmallBranches = true,
                FoliageGenerationStart = 2
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Tree 4",
                Axiom = "FFFFFFA",
                Rules_a = new LSystemRule() { Rule = "[&FFBFA]////[&BFFFA]////[&FBFFA]", Prob = 0.5f },
                Rules_b = new LSystemRule() { Rule = "[&FFFA]////[&FFFA]////[&FFFA]", Prob = 0.4f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 35,
                Iteration = 4,
                RandomeLevel = 0,
                TrunkType = TrunkType.Single,
                SmallBranches = true,
                FoliageGenerationStart = 2
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Tree 5",
                Axiom = "FFFFFAFAFAF",
                Rules_a = new LSystemRule() { Rule = "[&FFAFF]////[&FFAFF]////[&FFAFF]", Prob = 0.5f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 40,
                Iteration = 4,
                RandomeLevel = 0,
                TrunkType = TrunkType.Crossed,
                SmallBranches = true,
                FoliageGenerationStart = 2
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Beech",
                Axiom = "FFFFFAFA",
                Rules_a = new LSystemRule() { Rule = "[&&&F[++^Fb][--&b]//b[+^b][--&b]]////[&&&F[++^Fb][--&b]//b[+^b][--&b]]////[&&&F[++^Fb][--&Fb]//b[+^b][--&b]]", Prob = 0.9f },
                Rules_b = new LSystemRule() { Rule = "F", Prob = 0.6f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Single,
                SmallBranches = true,
                FoliageGenerationStart = 1
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Apple Tree",
                Axiom = "FFFFFAFFBF",
                Rules_a = new LSystemRule() { Rule ="[&&&FFFFF&&FFFF][&&&++++FFFFF&&FFFF][&&&----FFFFF&&FFFF]", Prob = 0.9f },
                Rules_b = new LSystemRule() { Rule = "[&&&++FFFFF&&FFFF][&&&--FFFFF&&FFFF][&&&------FFFFF&&FFFF]", Prob = 0.8f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Single,
                SmallBranches = true,
                FoliageGenerationStart = 1
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Oak",
                Axiom = "FFFFFFA",
                Rules_a = new LSystemRule() { Rule = "[&FFBFA]////[&BFFFA]////[&FBFFA]", Prob = 0.9f },
                Rules_b = new LSystemRule() { Rule = "[&FFFA]////[&FFFA]////[&FFFA]", Prob = 0.8f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 5,
                RandomeLevel = 2,
                TrunkType = TrunkType.Crossed,
                FoliageGenerationStart = 1
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Sequoia",
                Axiom = "FFFFFFFFFFddccA///cccFddcFA///ddFcFA/cFFddFcdBddd/A/ccdcddd/ccAddddcFBcccAccFdFcFBcccc/BFdFFcFFdcccc/B",
                Rules_a = new LSystemRule() { Rule = "[&&&GGF[++^FFdd][--&Fddd]//Fdd[+^Fd][--&Fdd]]////[&&&GGF[++^FFdd][--&Fddd]//Fdd[+^Fd][--&Fdd]]////[&&&GGF[++^FFdd][--&Fddd]//Fdd[+^Fd][--&Fdd]]", Prob = 0.9f },
                Rules_b = new LSystemRule() { Rule = "[&&&GGF[++^Fdd][--&Fdd]//dd[+^d][--&Fd]]////[&&&GGF[++^Fdd][--&Fdd]//dd[+^d][--&Fd]]////[&&&GGF[++^Fdd][--&Fdd]//dd[+^d][--&Fd]]", Prob = 0.8f },
                Rules_c = new LSystemRule() { Rule = "/", Prob = 0.7f },
                Rules_d = new LSystemRule() { Rule = "F", Prob = 0.6f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Crossed,
                SmallBranches = true,
                FoliageGenerationStart = 1
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Birch 1",
                Axiom = "FFFFFdddccA/FFFFFFcA/FFFFFFcB",
                Rules_a = new LSystemRule() { Rule = "[&&&dddd^^ddddddd][&&&---dddd^^ddddddd][&&&+++dddd^^ddddddd][&&&++++++dddd^^ddddddd]", Prob = 0.9f },
                Rules_b = new LSystemRule() { Rule = "[&&&ddd^^ddddd][&&&---ddd^^ddddd][&&&+++ddd^^ddddd][&&&++++++ddd^^ddddd]", Prob = 0.8f },
                Rules_c = new LSystemRule() { Rule = "/", Prob = 0.7f },
                Rules_d = new LSystemRule() { Rule = "F", Prob = 0.6f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Single,
                SmallBranches = true,
                FoliageGenerationStart = 1
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Birch 2",
                Axiom = "FFFdddccA/FFFFFccA/FFFFFccB",
                Rules_a = new LSystemRule() { Rule = "[&&&dFFF^^FFFdd][&&&---dFFF^^FFFdd][&&&+++dFFF^^FFFdd][&&&++++++dFFF^^FFFdd]", Prob = 0.9f },
                Rules_b = new LSystemRule() { Rule = "[&&&dFF^^FFFd][&&&---dFFF^^FFFd][&&&+++dFF^^FFFd][&&&++++++dFF^^FFFd]", Prob = 0.8f },
                Rules_c = new LSystemRule() { Rule = "/", Prob = 0.7f },
                Rules_d = new LSystemRule() { Rule = "F", Prob = 0.6f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Single,
                SmallBranches = true,
                FoliageGenerationStart = 1
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Palm",
                Axiom = "FFccc&FFFFFdddFA//A//A//A//A//A",
                Rules_a = new LSystemRule() { Rule = "[&fb&bbb[++f--&ffff&ff][--f++&ffff&ff]&ffff&bbbb&b]", Prob = 0.9f },
                Rules_b = new LSystemRule() { Rule = "f", Prob = 0.8f },
                Rules_c = new LSystemRule() { Rule = "/", Prob = 0.7f },
                Rules_d = new LSystemRule() { Rule = "F", Prob = 0.6f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Single,
                SmallBranches = true,
                FoliageGenerationStart = 1
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Spruce 1",
                Axiom = "FFFFFAFFFFFFBFFFFFFCFFFFFFDFFFFFF[&&&F^^FF][&&&++F^^FF][&&&++++F^^FF][&&&++++++F^^FF][&&&--F^^FF][&&&----F^^FF][FFFFf]",
                Rules_a = new LSystemRule() { Rule = "[&&&FFFFFF^^FFF][&&&++FFFFFF^^FFF][&&&++++FFFFFF^^FFF][&&&++++++FFFFFF^^FFF][&&&--FFFFFF^^FFF][&&&----FFFFFF^^FFF]", Prob = 0.9f },
                Rules_b = new LSystemRule() { Rule = "[&&&FFFFF^^FFF][&&&++FFFFF^^FFF][&&&++++FFFFF^^FFF][&&&++++++FFFFF^^FFF][&&&--FFFFF^^FFF][&&&----FFFFF^^FFF]", Prob = 0.8f },
                Rules_c = new LSystemRule() { Rule = "[&&&FFFF^^FFF][&&&++FFFF^^FFF][&&&++++FFFF^^FFF][&&&++++++FFFF^^FFF][&&&--FFFF^^FFF][&&&----FFFF^^FFF]", Prob = 0.7f },
                Rules_d = new LSystemRule() { Rule = "[&&&FFF^^FFF][&&&++FFF^^FFF][&&&++++FFF^^FFF][&&&++++++FFF^^FFF][&&&--FFF^^FFF][&&&----FFF^^FFF]", Prob = 0.6f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Crossed,
                SmallBranches = true,
                FoliageGenerationStart = 1
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Spruce 2",
                Axiom = "FFFFFFBFFFFFFCFFFFFFDFFFFFF[&&&F^^FF][&&&++F^^FF][&&&++++F^^FF][&&&++++++F^^FF][&&&--F^^FF][&&&----F^^FF][FFFFf]",
                Rules_b = new LSystemRule() { Rule = "[&&&FFFFF^^FFF][&&&++FFFFF^^FFF][&&&++++FFFFF^^FFF][&&&++++++FFFFF^^FFF][&&&--FFFFF^^FFF][&&&----FFFFF^^FFF]", Prob = 0.8f },
                Rules_c = new LSystemRule() { Rule = "[&&&FFFF^^FFF][&&&++FFFF^^FFF][&&&++++FFFF^^FFF][&&&++++++FFFF^^FFF][&&&--FFFF^^FFF][&&&----FFFF^^FFF]", Prob = 0.7f },
                Rules_d = new LSystemRule() { Rule = "[&&&FFF^^FFF][&&&++FFF^^FFF][&&&++++FFF^^FFF][&&&++++++FFF^^FFF][&&&--FFF^^FFF][&&&----FFF^^FFF]", Prob = 0.6f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Crossed,
                SmallBranches = true,
                FoliageGenerationStart = 1
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Pine",
                Axiom = "FFFFFcccdddB///cFdFB////cFdFB///cFdFB///cFdFA///cFdFA///cFdFB[FF]f",
                Rules_a = new LSystemRule() { Rule = "[&&&TTTT[++^TFdd][--&TFd]//Tdd[+^Fd][--&Fdd]]", Prob = 0.9f },
                Rules_b = new LSystemRule() { Rule = "[&&&TTT[++^Fdd][--&Fdd]//dd[+^d][--&Fd]]", Prob = 0.8f },
                Rules_c = new LSystemRule() { Rule = "/", Prob = 0.7f },
                Rules_d = new LSystemRule() { Rule = "F", Prob = 0.6f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Single,
                SmallBranches = true,
                FoliageGenerationStart = 1
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Willow",
                Axiom = "FFFFFFFFccA",
                Rules_a = new LSystemRule() { Rule = "[&FF&FFFF&&F&FFFFFFFdddd][**&FF&FFFF&&F&FFFFFFFdddd][//&FF&FFFF&&F&FFFFFFFdddd][////&FF&FFFF&&F&FFFFFFFdddd][//////&FF&FFFF&&F&FFFFFFFdddd][////////&FF&FFFF&&F&FFFFFFFdddd]", Prob = 0.9f },
                Rules_c = new LSystemRule() { Rule = "/", Prob = 0.7f },
                Rules_d = new LSystemRule() { Rule = "F", Prob = 0.6f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 30,
                Iteration = 2,
                RandomeLevel = 0,
                TrunkType = TrunkType.Crossed,
                SmallBranches = true,
                FoliageGenerationStart = 1
            });

            _treeTemplates.Add(new TreeTemplate()
            {
                Name = "Rubber",
                Axiom = "FFFFA",
                Rules_a = new LSystemRule() { Rule = "[&FFBFA]////[&BFFFA]////[&FBFFA]", Prob = 0.9f },
                Rules_b = new LSystemRule() { Rule = "[&FFA]////[&FFA]////[&FFA]", Prob = 0.8f },
                TrunkBlock = UtopiaProcessorParams.CubeId.Trunk,
                FoliageBlock = UtopiaProcessorParams.CubeId.Foliage,
                Angle = 35,
                Iteration = 3,
                RandomeLevel = 1,
                TrunkType = TrunkType.Double,
                SmallBranches = true,
                FoliageGenerationStart = 1
            });

            #endregion
        }

        #region Public Methods
        public void GenerateChunkItems(Vector2I chunkPosition, Biome biome, ChunkColumnInfo[] columndInfo, FastRandom chunkRnd)
        {
            //Generate landscape trees
            foreach (var entities in TreeGeneration(chunkPosition, biome, columndInfo, chunkRnd))
            {
                _entityManager.Insert(entities.ChunkLocation, entities);
            }
        }
        #endregion

        #region Private Methods
        private List<LandscapeEntity> TreeGeneration(Vector2I chunkPosition, Biome biome, ChunkColumnInfo[] columndInfo, FastRandom rnd)
        {
            List<LandscapeEntity> GlobalList = new List<LandscapeEntity>();

            int nbrTree = rnd.Next(biome.BiomeTrees.TreePerChunks.Min, biome.BiomeTrees.TreePerChunks.Max + 1);
            //for (int i = 0; i < nbrTree; i++)
            //{
            //    PopulateChunkWithTree(chunkPosition, biome.BiomeTrees, columndInfo, rnd);
            //}
            if (chunkPosition == new Vector2I(3, 2))
                GlobalList.AddRange(PopulateChunksWithTree(chunkPosition, columndInfo, rnd));


            return GlobalList;
        }

        private List<LandscapeEntity> PopulateChunksWithTree(Vector2I chunkPosition, ChunkColumnInfo[] columndInfo, FastRandom rnd)
        {
            //Get Rnd chunk Location.
            int x = rnd.Next(0, 16);
            int z = rnd.Next(0, 16);
            x = 7;
            z = 7;
            int y = columndInfo[x * AbstractChunk.ChunkSize.Z + z].MaxGroundHeight + 1;

            y = 70;

            x += (chunkPosition.X * AbstractChunk.ChunkSize.X);
            z += (chunkPosition.Y * AbstractChunk.ChunkSize.Z);
            Vector3I worldPosition = new Vector3I(x, y, z);

            //Generate Tree mesh !
            //TreeLSystem generator = _treeGenerator[rnd.Next(0, 5)];
            TreeTemplate treeType = _treeTemplates[1];

            return LandscapeEntityParser.GlobalMesh2ChunkMesh(_treeGenerator.Generate(rnd, worldPosition, treeType), LandscapeEntityType.Tree);
        }
        #endregion
    }
}
