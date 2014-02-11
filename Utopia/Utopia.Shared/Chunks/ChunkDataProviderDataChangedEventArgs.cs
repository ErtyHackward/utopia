using System;
using S33M3Resources.Structs;

namespace Utopia.Shared.Chunks
{
    public class ChunkDataProviderDataChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets count of blocks modified
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets locations array of the blocks
        /// </summary>
        public Vector3I[] Locations { get; set; }

        /// <summary>
        /// Gets values array of the blocks
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// Gets optional tags array
        /// </summary>
        public BlockTag[] Tags { get; set; }

        /// <summary>
        /// Gets optional id of the entity which is responsible for the change
        /// </summary>
        public uint SourceDynamicId { get; set; }
    }
}