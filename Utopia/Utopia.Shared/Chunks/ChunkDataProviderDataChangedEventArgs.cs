using System;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

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
    }
}