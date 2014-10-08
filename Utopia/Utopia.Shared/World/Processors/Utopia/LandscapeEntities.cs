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
        private LandscapeBufferManager _entityManager;
        private WorldParameters _worldParameters;
        #endregion

        #region Public Properties
        #endregion

        public LandscapeEntities(LandscapeBufferManager entityManager, WorldParameters worldParameters)
        {
            _entityManager = entityManager;
            _worldParameters = worldParameters;
        }

        #region Public Methods
        public void GenerateChunkItems(Vector3I chunkPosition, Biome biome, byte[] chunkBytes, ChunkColumnInfo[] columndInfo, FastRandom chunkRnd)
        {
            //Generate landscape trees
            foreach (var entities in TreeGeneration(chunkPosition, biome, chunkBytes, columndInfo, chunkRnd))
            {
                _entityManager.Insert(entities.ChunkLocation, entities);
            }
        }
        #endregion

        #region Private Methods
        private List<LandscapeEntity> TreeGeneration(Vector3I chunkPosition, Biome biome, byte[] chunkBytes, ChunkColumnInfo[] columndInfo, FastRandom rnd)
        {
            var globalList = new List<LandscapeEntity>();

            int nbrTree = rnd.Next(biome.BiomeTrees.TreePerChunks.Min, biome.BiomeTrees.TreePerChunks.Max + 1);
            for (int i = 0; i < nbrTree; i++)
            {
                //Check probability to make it spawn !
                if (rnd.Next(0, 100) < biome.BiomeTrees.ChanceOfSpawning)
                {
                    var treeEntities = PopulateChunksWithTree(chunkPosition, biome, chunkBytes, columndInfo, rnd);
                    if (treeEntities != null) 
                        globalList.AddRange(treeEntities);
                }
            }

            return globalList;
        }

        private List<LandscapeEntity> PopulateChunksWithTree(Vector3I chunkPosition, Biome biome, byte[] chunkBytes, ChunkColumnInfo[] columndInfo, FastRandom rnd)
        {
            if (biome.BiomeTrees.Trees.Count <= 0) return null;
            //Get Rnd chunk Location.
            int x = rnd.Next(0, 16);
            int z = rnd.Next(0, 16);
            int y = columndInfo[x * AbstractChunk.ChunkSize.Z + z].MaxGroundHeight + 1;

            //Validate position = Must be Air block (not water) !
            if (chunkBytes[((z * AbstractChunk.ChunkSize.X) + x) * AbstractChunk.ChunkSize.Y + y] != WorldConfiguration.CubeId.Air) return null;

            x += (chunkPosition.X * AbstractChunk.ChunkSize.X);
            z += (chunkPosition.Z * AbstractChunk.ChunkSize.Z);
            Vector3I worldPosition = new Vector3I(x, y, z);

            //Generate Tree mesh !
            //Get tree type following distribution chances inside the biome
            TreeBluePrint treeType = biome.BiomeTrees.GetTreeTemplate(rnd, _worldParameters.Configuration.TreeBluePrintsDico);
            int generationSeed = rnd.Next();
            return LandscapeEntityParser.GlobalMesh2ChunkMesh(_treeGenerator.Generate(generationSeed, worldPosition, treeType), worldPosition, treeType.Id, generationSeed);
        }
        #endregion
    }
}
