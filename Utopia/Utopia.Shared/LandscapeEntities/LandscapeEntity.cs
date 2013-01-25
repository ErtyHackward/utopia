﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.LandscapeEntities
{
    [ProtoContract]
    public struct LandscapeEntity
    {
        [ProtoMember(1)]
        public LandscapeEntityType Type { get; set; }
        [ProtoMember(2)]
        public Vector2I ChunkLocation { get; set; }
        [ProtoMember(3, OverwriteList = true)]
        public List<BlockWithPosition> Blocks { get; set; }
        //Static Entities
    }

    public enum LandscapeEntityType
    {
        Tree
    }
}
