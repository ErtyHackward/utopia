using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Entities.Concrete;
using System.Drawing.Design;
using Utopia.Shared.Chunks;
using Utopia.Shared.Services;
using Utopia.Shared.Tools;

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
        [Editor(typeof(Season.SeasonsEditor), typeof(UITypeEditor))]
        public List<string> GrowingSeasons { get; set; }

        [ProtoMember(5)]
        [Description("Specify block types where the entity can grow. It will not grow on other block types")]
        public List<byte> GrowingBlocks { get; set; }

        [ProtoMember(6)]
        [Description("Do the entity need the light to grow?")]
        public bool NeedLight { get; set; }

        [ProtoMember(7)]
        [Description("Probability of entity to rotten at grow level 0. [0;1]")]
        public float RottenChance { get; set; }

        public GrowingEntity()
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
        public TimeSpan GrowTime { get; set; }
    }

    public class ModelStateConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //true means show a combobox
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            //true will limit to list. false will show the list, 
            //but allow free-form entry
            return true;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            //return new StandardValuesCollection(EditorConfigHelper.Config.BlockProfiles.Where(x => x != null).Select(x => x.Name).Where(x => x != "System Reserved").OrderBy(x => x).ToList());
            return new StandardValuesCollection(ModelStateSelector.PossibleValues);
        }
    }
}