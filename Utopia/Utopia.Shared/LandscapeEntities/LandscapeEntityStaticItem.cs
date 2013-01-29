using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using S33M3Resources.Structs;

namespace Utopia.Shared.LandscapeEntities
{
    [ProtoContract]
    public class LandscapeEntityStaticItem
    {
        [ProtoMember(1)]
        public int ItemblueprintId { get; set; }
        [ProtoMember(2)]
        public float SpawningRange { get; set; }
        [ProtoMember(3)]
        public RangeI Quantity { get; set; }
        [ProtoMember(4)]
        public SpawningType SpawningType { get; set; }
    }

    public enum SpawningType
    {
        Ground,
        Ceiling,
        Both
    }
}
