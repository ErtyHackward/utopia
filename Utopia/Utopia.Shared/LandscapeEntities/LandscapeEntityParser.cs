using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.LandscapeEntities
{
    public static class LandscapeEntityParser
    {
        public static List<LandscapeEntity> GlobalMesh2ChunkMesh(IEnumerable<BlockWithPosition> globalMesh, Vector3I worldRootLocation, int landscapeEntityId)
        {
            Dictionary<Vector2I, LandscapeEntity> chunks = new Dictionary<Vector2I, LandscapeEntity>();

            foreach (var data in globalMesh)
            {
                BlockWithPosition localData = data;
                //Ge chunk position
                Vector2I ChunkLocation;
                GetChunk(localData.WorldPosition, out ChunkLocation);

                LandscapeEntity chunkMesh;
                if (chunks.TryGetValue(ChunkLocation, out chunkMesh) == false)
                {
                    chunkMesh = new LandscapeEntity();
                    chunkMesh.LandscapeEntityId = landscapeEntityId;
                    chunkMesh.ChunkLocation = ChunkLocation;
                    chunkMesh.Blocks = new List<BlockWithPosition>();
                    chunkMesh.RootLocation = new Vector3I(worldRootLocation.X - (ChunkLocation.X * AbstractChunk.ChunkSize.X),
                                                          worldRootLocation.Y,
                                                          worldRootLocation.Z - (ChunkLocation.Y * AbstractChunk.ChunkSize.Z)
                                                          );
                    chunks.Add(ChunkLocation, chunkMesh);
                }
                //Tranform World position to chunk position
                localData.ChunkPosition.X = localData.WorldPosition.X - (ChunkLocation.X * AbstractChunk.ChunkSize.X);
                localData.ChunkPosition.Y = localData.WorldPosition.Y;
                localData.ChunkPosition.Z = localData.WorldPosition.Z - (ChunkLocation.Y * AbstractChunk.ChunkSize.Z);
                chunkMesh.Blocks.Add(localData);
            }

            return chunks.Values.ToList();
        }

        private static void GetChunk(Vector3I blockPosition, out Vector2I chunkPosition)
        {
            chunkPosition = new Vector2I(MathHelper.Floor((double)blockPosition.X / AbstractChunk.ChunkSize.X),
                                         MathHelper.Floor((double)blockPosition.Z / AbstractChunk.ChunkSize.Z));
        }
    }
}
