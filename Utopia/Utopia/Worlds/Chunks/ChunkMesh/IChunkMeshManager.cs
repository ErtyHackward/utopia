using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Worlds.Chunks.ChunkMesh
{
    public interface IChunkMeshManager
    {
        void CreateChunkMesh(VisualChunk2D chunk);
        IWorldChunks2D WorldChunks { get; set; }
    }
}
