using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Worlds.Chunks.ChunkWrapper
{
    public interface IChunksWrapper
    {
        IWorldChunks2D WorldChunks { get; set; }

        void AddWrapOperation(ChunkWrapType operationType);
    }
}
