using System;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Events
{
    public class BlocksChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Internal block locations
        /// </summary>
        public Vector3I[] Locations { get; set; }

        /// <summary>
        /// New blocks values
        /// </summary>
        public byte[] BlockValues { get; set; }

        /// <summary>
        /// Chunk position
        /// </summary>
        public Vector2I ChunkPosition { get; set; }
    }
}
