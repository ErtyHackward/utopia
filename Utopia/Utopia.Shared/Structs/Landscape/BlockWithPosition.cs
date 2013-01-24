using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;

namespace Utopia.Shared.Structs.Landscape
{
    public struct BlockWithPosition
    {
        public byte BlockId;
        public Vector3I WorldPosition;
        public Vector3I ChunkPosition;
    }
}
