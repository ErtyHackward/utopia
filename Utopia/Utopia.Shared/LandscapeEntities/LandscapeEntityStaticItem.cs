using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Tools;
using Utopia.Shared.Entities;
using System.Drawing.Design;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.LandscapeEntities
{
    [ProtoContract]
    public partial class LandscapeEntityStaticItem
    {
        [ProtoMember(1)]
        [Editor(typeof(BlueprintTypeEditor<Entity>), typeof(UITypeEditor))]
        [TypeConverter(typeof(BlueprintTextHintConverter))]
        public ushort ItemblueprintId { get; set; }
        [ProtoMember(2)]
        public float SpawningRange { get; set; }
        [ProtoMember(3)]
        public RangeI Quantity { get; set; }
        [ProtoMember(4)]
        public SpawningType SpawningType { get; set; }

        public override string ToString()
        {
            return EditorConfigHelper.Config.BluePrints[ItemblueprintId].Name;
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
