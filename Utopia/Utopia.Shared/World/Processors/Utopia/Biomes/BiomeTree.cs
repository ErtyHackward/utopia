using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utopia.Shared.LandscapeEntities.Trees;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    [ProtoContract]
    public partial class BiomeTree
    {
        [Browsable(false)]
        [ProtoMember(1)]
        public uint TemplateId { get; set; }
        [ProtoMember(2)]
        public uint SpawnChances { get; set; }

        public override string ToString()
        {
            return this.Tree; 
        }
    }
}
