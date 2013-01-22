using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.LandscapeEntities
{
    public class LandscapeEntityChunkMesh
    {
        public Vector2I ChunkLocation { get; set; }
        public List<BlockWithPosition> Blocks { get; set; }
    }
}
