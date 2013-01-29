using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Utopia.Shared.LandscapeEntities
{
    [ProtoContract]
    public class LandscapeEntityBluePrint
    {
        [ProtoMember(1)]
        public int Id { get; set; }
        [ProtoMember(2)]
        public int Name { get; set; }
        [ProtoMember(3, OverwriteList=true)]
        public List<LandscapeEntityStaticItem> StaticItems { get; set; }

        public LandscapeEntityBluePrint()
        {
            StaticItems = new List<LandscapeEntityStaticItem>();
        }
    }
}
