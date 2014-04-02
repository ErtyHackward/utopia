using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Chunks
{
    public interface IEntitySpawningControler
    {
        bool TryGetSpawnLocation(ChunkSpawnableEntity entity, AbstractChunk chunk, ByteChunkCursor cursor, FastRandom rnd, out Vector3D entitySpawnLocation);
    }
}
