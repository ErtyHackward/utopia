using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utopia.Shared.LandscapeEntities.Trees;
using Utopia.Shared.RealmEditor;
using System.Drawing.Design;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    [ProtoContract]
    public partial class BiomeTree
    {
        [Browsable(false)]
        [ProtoMember(1)]
        public int TemplateId { get; set; }

        [ProtoMember(2)]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMaxAttribute(0, 1000)]
        [DisplayName("Spanwing Distribution"), Description("Spawning distribution chance")]
        public int SpawnDistribution { get; set; }

        [Browsable(false)]
        public int SpawnDistributionThreshold { get; set; }

        public override string ToString()
        {
            return this.Tree; 
        }
    }
}
