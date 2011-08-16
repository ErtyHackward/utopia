using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;
using Utopia.Shared.Interfaces;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Represents a chunk for 3d rendering
    /// </summary>
    public class VisualChunk : CompressibleChunk, ISingleArrayDataProviderUser
    {
        /// <summary>
        /// Gets or sets current chunk position
        /// </summary>
        public IntVector2 ChunkPosition { get; set; }

        public VisualChunk()
            : base(new SingleArrayDataProvider())
        {

        }

    }
}
