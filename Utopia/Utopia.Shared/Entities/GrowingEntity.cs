using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Entities.Concrete;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for growing static or tree entity
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(PlantGrowingEntity))]
    [ProtoInclude(101, typeof(TreeGrowingEntity))]
    public abstract class GrowingEntity : BlockLinkedItem
    {
        [ProtoMember(1)]
        public List<GrowLevel> GrowLevels { get; set; }

        [ProtoMember(2)]
        [Browsable(false)]
        public int CurrentGrowLevel { get; set; }

        [ProtoMember(3)]
        [Browsable(false)]
        public DateTime LastLevelUpdate { get; set; }

        [ProtoMember(4)]
        public List<string> GrowingSeasons { get; set; }

        [ProtoMember(5)]
        [Description("Specify block types where the entity can grow. It will not grow on other block types")]
        public List<byte> GrowingBlocks { get; set; }

        [ProtoMember(6)]
        [Description("Do the entity need the light to grow?")]
        public bool NeedLight { get; set; }

        [ProtoMember(7)]
        [Description("Probability of entity to rotten. [0;1]")]
        public float RottenChance { get; set; }
    }

    [ProtoContract]
    public struct GrowLevel
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public string ModelState { get; set; }

        [ProtoMember(3)]
        public TimeSpan GrowTime { get; set; }
    }
}