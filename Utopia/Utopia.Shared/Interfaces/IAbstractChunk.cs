using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;

namespace Utopia.Shared.Interfaces
{
    public interface IAbstractChunk
    {
        /// <summary>
        /// Gets or sets position of the chunk
        /// </summary>
        Vector3I Position { get; set; }

        /// <summary>
        /// Gets a chunk blocks data provider
        /// </summary>
        ChunkDataProvider BlockData { get; }

        /// <summary>
        /// Gets entity collection of the chunk
        /// </summary>
        EntityCollection Entities { get; }
    }
}