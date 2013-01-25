using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.LandscapeEntities
{
    public struct LandscapeEntity
    {
        public LandscapeEntityType Type;
        public Vector2I ChunkLocation;
        public List<BlockWithPosition> Blocks;
        //Static Entities
    }

    public enum LandscapeEntityType
    {
        Tree
    }
}
