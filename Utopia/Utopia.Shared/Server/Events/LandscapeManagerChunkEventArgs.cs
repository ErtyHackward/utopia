using System;
using Utopia.Shared.Server.Structs;

namespace Utopia.Shared.Server.Events
{
    public class LandscapeManagerChunkEventArgs : EventArgs
    {
        public ServerChunk Chunk { get; set; }
    }
}