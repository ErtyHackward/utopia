using System.Collections.Generic;
using System.Linq;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.LandscapeEntities
{
    public static class LandscapeEntityParser
    {
        public static List<LandscapeEntity> GlobalMesh2ChunkMesh(IEnumerable<BlockWithPosition> globalMesh, Vector3I worldRootLocation, int landscapeEntityId, int generationSeed)
        {
            var chunks = new Dictionary<Vector3I, LandscapeEntity>();

            foreach (var data in globalMesh)
            {
                BlockWithPosition localData = data;
                //Get chunk position
                var chunkLocation = BlockHelper.BlockToChunkPosition(localData.WorldPosition);
                
                LandscapeEntity chunkMesh;
                if (chunks.TryGetValue(chunkLocation, out chunkMesh) == false)
                {
                    chunkMesh = new LandscapeEntity();
                    chunkMesh.LandscapeEntityId = landscapeEntityId;
                    chunkMesh.ChunkLocation = chunkLocation;
                    chunkMesh.GenerationSeed = generationSeed;
                    chunkMesh.Blocks = new List<BlockWithPosition>();
                    chunkMesh.RootLocation = new Vector3I(worldRootLocation.X - (chunkLocation.X * AbstractChunk.ChunkSize.X),
                                                          worldRootLocation.Y,
                                                          worldRootLocation.Z - (chunkLocation.Z * AbstractChunk.ChunkSize.Z)
                                                          );
                    chunks.Add(chunkLocation, chunkMesh);
                }
                //Tranform World position to chunk position
                localData.ChunkPosition.X = localData.WorldPosition.X - (chunkLocation.X * AbstractChunk.ChunkSize.X);
                localData.ChunkPosition.Y = localData.WorldPosition.Y;
                localData.ChunkPosition.Z = localData.WorldPosition.Z - (chunkLocation.Z * AbstractChunk.ChunkSize.Z);
                chunkMesh.Blocks.Add(localData);
            }

            return chunks.Values.ToList();
        }
    }
}
