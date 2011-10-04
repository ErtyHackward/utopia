using Utopia.Shared.Chunks;
using Utopia.Shared.Chunks.Entities;

namespace Utopia.Shared.Interfaces
{
    public interface IAbstractChunk
    {
        /// <summary>
        /// Gets a chunk blocks data provider
        /// </summary>
        ChunkDataProvider BlockData { get; }

        /// <summary>
        /// Gets entity collection of the chunk
        /// </summary>
        EntityCollection Entities { get; set; }
    }
}