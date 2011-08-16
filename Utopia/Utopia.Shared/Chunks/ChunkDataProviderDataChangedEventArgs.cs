using System;
using Utopia.Shared.Structs;

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
        public Location3<int>[] Locations { get; set; }

        /// <summary>
        /// Gets values array of the blocks
        /// </summary>
        public byte[] Bytes { get; set; }
    }
}