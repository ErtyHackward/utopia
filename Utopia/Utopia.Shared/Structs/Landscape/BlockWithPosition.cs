using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using S33M3Resources.Structs;

namespace Utopia.Shared.Structs.Landscape
{
    [ProtoContract]
    public struct BlockWithPosition
    {
        [ProtoMember(1)]
        public byte BlockId;
        [ProtoMember(2)]
        public Vector3I WorldPosition;
        [ProtoMember(3)]
        public Vector3I ChunkPosition;
        [ProtoMember(4)]
        public bool isMandatory;
    }
}
