using System;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;

namespace Utopia.Server.Managers
{
    public class ServerLandscapeManagerBlockChangedEventArgs : EventArgs
    {
        public int Count { get; set; }

        public ServerLandscapeManagerBlockChangedEventArgs(IChunkLayout2D chunk, ChunkDataProviderDataChangedEventArgs e)
        {
            Count = e.Count;

            Locations = new Vector3I[Count];
            Values = new byte[Count];
            if (e.Tags != null)
                Tags = new BlockTag[Count];

            var chunkPos = new Vector3I(chunk.Position.X * AbstractChunk.ChunkSize.X,0, chunk.Position.Y * AbstractChunk.ChunkSize.Z);

            for (int i = 0; i < Count; i++)
            {
                Locations[i] = chunkPos + e.Locations[i];
                Values[i] = e.Bytes[i];
                if (e.Tags != null)
                    Tags[i] = e.Tags[i];
            }

        }
        /// <summary>
        /// Global positions array
        /// </summary>
        public Vector3I[] Locations { get; set; }

        /// <summary>
        /// Array of the values
        /// </summary>
        public byte[] Values { get; set; }

        /// <summary>
        /// Array of tags
        /// </summary>
        public BlockTag[] Tags { get; set; }
    }
}
