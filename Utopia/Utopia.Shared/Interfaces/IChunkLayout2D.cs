using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;

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
