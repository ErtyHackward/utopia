using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Entities.Concrete;
using System.Drawing.Design;
using Utopia.Shared.Services;
using Utopia.Shared.Structs;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for growing static or tree entity
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(PlantGrowingEntity))]
    [ProtoInclude(101, typeof(TreeGrowingEntity))]
    public abstract partial class GrowingEntity : BlockLinkedItem
    {
        /// <summary>
        /// Last grow service check
        /// </summary>
        [Browsable(false)]
        [Category("Growing")]
        [ProtoMember(3)]
        public UtopiaTime LastGrowUpdate { get; set; }

        [Category("Growing")]
        [Editor(typeof(Season.SeasonsEditor), typeof(UITypeEditor))]
        [ProtoMember(4)]
        public List<string> GrowingSeasons { get; set; }

        [Category("Growing")]
        [Description("Specify block types where the entity can grow. It will not grow on other block types")]
        [Editor(typeof(MultiBlockListEditor), typeof(UITypeEditor))]
        [ProtoMember(5)]
        public List<byte> GrowingBlocks { get; set; }

        
        [Category("Growing")]
        [Description("Do the entity need the light to grow?")]
        [ProtoMember(6)]
        public bool NeedLight { get; set; }

        
        [Category("Growing")]
        [Description("Probability of entity to rotten at grow level 0. [0;1]")]
        [ProtoMember(7)]
        public float RottenChance { get; set; }

        /// <summary>
        /// How much grow time passed from the last level change
        /// </summary>
        [Browsable(false)]
        [ProtoMember(8)]
        public UtopiaTimeSpan CurrentGrowTime { get; set; }
        
        protected GrowingEntity()
        {
            GrowingSeasons = new List<string>();
            GrowingBlocks = new List<byte>();
        }

        public override object Clone()
        {
            var cloned = (GrowingEntity)base.Clone();
            
            cloned.GrowingBlocks = new List<byte>(GrowingBlocks);
            cloned.GrowingSeasons = new List<string>(GrowingSeasons);

            return cloned;
        }
    }

    [ProtoContract]
    public class GrowLevel : ICloneable
    {
        [ProtoMember(1)]
        [Browsable(false)]
        public string LevelId { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        [TypeConverter(typeof(ModelStateConverter))]
        public string ModelState { get; set; }

        [ProtoMember(4)]
        [Browsable(false)]
        public UtopiaTimeSpan GrowTime { get; set; }

        [ProtoMember(5)]
        [Description("Generated entities when harvested")]
        public List<InitSlot> HarvestSlots { get; set; }

        [DisplayName("Grow Time (Hours)")]
        public double GrowTimeH
        {
            get { return GrowTime.TotalSeconds / 3600.0f; }
            set { GrowTime = new UtopiaTimeSpan() { TotalSeconds = (long)(value * 3600.0f) }; }
        }

        public GrowLevel()
        {
            HarvestSlots = new List<InitSlot>();
        }
        
        public object Clone()
        {
            var cloned = (GrowLevel)MemberwiseClone();
            cloned.HarvestSlots = new List<InitSlot>(HarvestSlots);
            return cloned;
        }
    }



}