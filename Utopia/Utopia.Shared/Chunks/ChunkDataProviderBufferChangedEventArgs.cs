using System;

namespace Utopia.Shared.Chunks
{
    public class ChunkDataProviderBufferChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a new buffer
        /// </summary>
        public byte[] NewBuffer { get; set; }
    }
}