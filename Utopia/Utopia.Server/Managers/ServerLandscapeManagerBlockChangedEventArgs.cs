using System;
using S33M3Resources.Structs;
using Utopia.Server.Utils;
using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Server.Managers
{
    public class ServerLandscapeManagerBlockChangedEventArgs : EventArgs
    {
        public int Count { get; set; }

        public ServerLandscapeManagerBlockChangedEventArgs(IChunkLayout2D chunk, ChunkDataProviderDataChangedEventArgs e)
        {
            Count = e.Count;

            Locations = new Vector3I[Count];
            e.Locations.CopyTo(Locations, 0);
            BlockHelper.ConvertToGlobal(chunk.Position, Locations);

            Values = e.Bytes;

            if (e.Tags != null)
                Tags = e.Tags;
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
