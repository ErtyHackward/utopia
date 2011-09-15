using System;
using Utopia.Server.Structs;

namespace Utopia.Server.Managers
{
    public class LandscapeManagerChunkEventArgs : EventArgs
    {
        public ServerChunk Chunk { get; set; }
    }
}