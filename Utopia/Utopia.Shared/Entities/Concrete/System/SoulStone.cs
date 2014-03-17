using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Concrete.Interface;

namespace Utopia.Shared.Entities.Concrete.System
{
    [ProtoContract]
    [EditorHide]
    [Description("Entity that will be use as player soulstone")]
    public class SoulStone : OrientedBlockItem, IOwnerBindable
    {
        [ProtoMember(1)]
        [Browsable(false)]
        public int DynamicEntityOwnerID { get; set;}

        public SoulStone()
        {
            GroupName = "System entities";
            Name = "SoulStone";
        }
    }
}
