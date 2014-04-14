using System;
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
        public int LandscapeEntityId { get; set; }

        [ProtoMember(2)]
        public Vector3I ChunkLocation { get; set; }

        [ProtoMember(3, OverwriteList = true)]
        public List<BlockWithPosition> Blocks { get; set; }

        [ProtoMember(4)]
        public Vector3I RootLocation { get; set; }

        [ProtoMember(5)]
        public int GenerationSeed { get; set; }
    }

}
