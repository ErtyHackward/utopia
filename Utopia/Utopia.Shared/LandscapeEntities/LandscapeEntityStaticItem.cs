using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ProtoBuf;
using S33M3Resources.Structs;

namespace Utopia.Shared.LandscapeEntities
{
    [ProtoContract]
    public partial class LandscapeEntityStaticItem
    {
        [Browsable(false)]
        [ProtoMember(1)]
        public ushort ItemblueprintId { get; set; }
        [ProtoMember(2)]
        public float SpawningRange { get; set; }
        [ProtoMember(3)]
        public RangeI Quantity { get; set; }
        [ProtoMember(4)]
        public SpawningType SpawningType { get; set; }

        public override string ToString()
        {
            return EntityName;
        }

        public LandscapeEntityStaticItem()
        {
            ItemblueprintId = 256;
        }
    }

    public enum SpawningType
    {
        Ground,
        Ceiling,
        Both
    }
}
