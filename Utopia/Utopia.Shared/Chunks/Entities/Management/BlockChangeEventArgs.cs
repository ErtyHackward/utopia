using System;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Management
{
    public class BlockChangeEventArgs : EventArgs
    {
        public Location3<int> BlockPosition { get; set; }
        public byte CubeId { get; set; }
    }
}