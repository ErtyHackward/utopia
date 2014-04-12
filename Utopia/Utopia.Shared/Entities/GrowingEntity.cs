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
        [ProtoMember(1)]
        [Category("Growing")]
        public List<GrowLevel> GrowLevels { get; set; }

        [ProtoMember(2)]
        [Browsable(false)]
        [Category("Growing")]
        public int CurrentGrowLevelIndex { get; set; }

        [ProtoMember(3)]
        [Browsable(false)]
        [Category("Growing")]
        public UtopiaTime LastGrowUpdate { get; set; }

        [ProtoMember(4)]
        [Category("Growing")]
        [Editor(typeof(Season.SeasonsEditor), typeof(UITypeEditor))]
        public List<string> GrowingSeasons { get; set; }

        [ProtoMember(5)]
        [Category("Growing")]
        [Description("Specify block types where the entity can grow. It will not grow on other block types")]
        [Editor(typeof(MultiBlockListEditor), typeof(UITypeEditor))]
        public List<byte> GrowingBlocks { get; set; }

        [ProtoMember(6)]
        [Category("Growing")]
        [Description("Do the entity need the light to grow?")]
        public bool NeedLight { get; set; }

        [ProtoMember(7)]
        [Category("Growing")]
        [Description("Probability of entity to rotten at grow level 0. [0;1]")]
        public float RottenChance { get; set; }

        /// <summary>
        /// When the last Grow refresh logic has been done on the entity.
        /// </summary>
        [ProtoMember(8)]
        [Browsable(false)]
        public UtopiaTime LastGrowRefresh { get; set; }
        
        [Browsable(false)]
        public GrowLevel CurrentGrowLevel { get { return GrowLevels.Count > CurrentGrowLevelIndex ? GrowLevels[CurrentGrowLevelIndex] : default(GrowLevel); } }

        [Browsable(false)]
        public bool IsLastGrowLevel
        {
            get { return CurrentGrowLevelIndex == GrowLevels.Count - 1; }
        }

        protected GrowingEntity()
        {
            GrowLevels = new List<GrowLevel>();
        }
    }

    [ProtoContract]
    public struct GrowLevel
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
        [Description("Generated entity when harvested")]
        [TypeConverter(typeof(ValueTypeTypeConverter<InitSlot>))]
        public InitSlot HarvestSlot { get; set; }

        [DisplayName("Grow Time (Hours)")]
        public double GrowTimeH
        {
            get { return GrowTime.TotalSeconds / 3600.0f; }
            set { GrowTime = new UtopiaTimeSpan() { TotalSeconds = (long)(value * 3600.0f) }; }
        }
    }



}