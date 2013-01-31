using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Utopia.Shared.LandscapeEntities
{
    [ProtoContract]
    public class LandscapeEntityBluePrint
    {
        [Browsable(false)]
        [ProtoMember(1)]
        public int Id { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3, OverwriteList=true)]
        [DisplayName("Linked static entities")]
        public List<LandscapeEntityStaticItem> StaticItems { get; set; }

        public LandscapeEntityBluePrint()
        {
            StaticItems = new List<LandscapeEntityStaticItem>();
        }
    }
}
