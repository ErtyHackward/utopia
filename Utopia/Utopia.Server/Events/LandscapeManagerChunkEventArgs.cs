using System;
using Utopia.Server.Structs;

namespace Utopia.Server.Events
{
    public class LandscapeManagerChunkEventArgs : EventArgs
    {
        public ServerChunk Chunk { get; set; }
    }
}