using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Shared.Interfaces
{
    /// <summary>
    /// Represents a chunk that have 2d layout with other chunks
    /// </summary>
    public interface IChunkLayout2D : IAbstractChunk
    {
        /// <summary>
        /// Gets or sets position of the chunk
        /// </summary>
        Vector2I Position { get; set; }
    }
}
