using System;
using Utopia.Shared.Chunks;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Events
{
    public class BlocksChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Internal block locations
        /// </summary>
        public Vector3I[] Locations { get; set; }

        /// <summary>
        /// Global positions
        /// </summary>
        public Vector3I[] GlobalLocations { get; set; }

        /// <summary>
        /// New blocks values
        /// </summary>
        public byte[] BlockValues { get; set; }

        /// <summary>
        /// Chunk position
        /// </summary>
        public Vector3I ChunkPosition { get; set; }

        /// <summary>
        /// Associated tags
        /// </summary>
        public BlockTag[] Tags { get; set; }

        /// <summary>
        /// Id of the entity responsible for the change
        /// </summary>
        public uint SourceEntityId { get; set; }
    }
}
