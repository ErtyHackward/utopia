using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Entities;
using Utopia.Shared.Services;
using Utopia.Shared.Settings;
using Utopia.Shared.Structs;
using Utopia.Shared.Tools;
using System.Drawing.Design;
using Utopia.Shared.Entities.Concrete;

namespace Utopia.Shared.LandscapeEntities.Trees
{
    [ProtoContract]
    public partial class TreeBluePrint : LandscapeEntityBluePrint
    {
        [Category("Configuration")]
        [ProtoMember(2)]
        public string Axiom { get; set; }
        [Category("Configuration")]
        [DisplayName("Rule A")]
        [ProtoMember(3)]
        public LSystemRule Rules_a { get; set; }
        [Category("Configuration")]
        [DisplayName("Rule B")]
        [ProtoMember(4)]
        public LSystemRule Rules_b { get; set; }
        [Category("Configuration")]
        [DisplayName("Rule C")]
        [ProtoMember(5)]
        public LSystemRule Rules_c { get; set; }
        [Category("Configuration")]
        [DisplayName("Rule D")]
        [ProtoMember(6)]
        public LSystemRule Rules_d { get; set; }
        [Category("Configuration")]
        [ProtoMember(7)]
        [Editor(typeof(BlueprintTypeEditor<BlockProfile>), typeof(UITypeEditor))]
        [TypeConverter(typeof(BlueprintTextHintConverter))]
        public byte TrunkBlock { get; set; }
        [Category("Configuration")]
        [ProtoMember(8)]
        [Editor(typeof(BlueprintTypeEditor<BlockProfile>), typeof(UITypeEditor))]
        [TypeConverter(typeof(BlueprintTextHintConverter))]
        public byte FoliageBlock { get; set; }
        [Category("Configuration")]
        [ProtoMember(9)]
        public double Angle { get; set; }
        [Category("Configuration")]
        [ProtoMember(10)]
        public int Iteration { get; set; }
        [Category("Configuration")]
        [DisplayName("Iteration Rnd Level")]
        [ProtoMember(11)]
        public int IterationRndLevel { get; set; }
        [Category("Configuration")]
        [ProtoMember(13)]
        [DisplayName("Trunk Type")]
        public TrunkType TrunkType { get; set; }
        [Category("Configuration")]
        [DisplayName("is using small branch")]
        [ProtoMember(14)]
        public bool SmallBranches { get; set; }
        [Category("Configuration")]
        [DisplayName("Foliage start Iteration")]
        [ProtoMember(15)]
        public int FoliageGenerationStart { get; set; }
        [Category("Configuration")]
        [DisplayName("Foliage size")]
        [ProtoMember(16)]
        public Vector3I FoliageSize { get; set; }
        
        [Category("Growing")]
        [Description("Tree grow time from seed to final state")]
        [TypeConverter(typeof(UtopiaTimeSpanConverter))]
        [ProtoMember(17)]
        public UtopiaTimeSpan GrowTime { get; set; }
        [Category("Growing")]
        [Description("Will grow only in specified seasond. If not set will grow all the time")]
        [Editor(typeof(Season.SeasonsEditor), typeof(UITypeEditor))]
        [ProtoMember(18)]
        public List<string> GrowingSeasons { get; set; }
        [Category("Growing")]
        [Description("Tree seed inventory icon model")]
        [Editor(typeof(ModelSelector), typeof(UITypeEditor))]
        [ProtoMember(19)]
        public string SeedModel { get; set; }
        [Category("Growing")]
        [Description("Will grow only on specified blocks. If not set will grow anywhere")]
        [Editor(typeof(GrowingEntity.MultiBlockListEditor), typeof(UITypeEditor))]
        [ProtoMember(20)]
        public List<byte> GrowingBlocks { get; set; }
        [Category("Growing")]
        [Description("How much game time should pass between items regenerations")]
        [TypeConverter(typeof(UtopiaTimeSpanConverter))]
        [ProtoMember(21)]
        public UtopiaTimeSpan ItemsRegenerationTime { get; set; }

        public TreeBluePrint()
        {
            GrowingSeasons = new List<string>();
        }

        public override string ToString()
        {
            return base.Name;
        }
    }

    [ProtoContract]
    [TypeConverter(typeof(LSystemRuleTypeConverter))]
    public struct LSystemRule
    {
        [ProtoMember(1)]
        public string Rule { get; set; }
        [ProtoMember(2)]
        [DisplayName("Probability")]
        public float Prob { get; set; }

        //Property Grid editing Purpose
        internal class LSystemRuleTypeConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {   //This method is used to shown information in the PropertyGrid.
                if (destinationType == typeof(string))
                {
                    return ("Rule : " + ((LSystemRule)value).Rule + " Proba : " + ((LSystemRule)value).Prob);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(LSystemRule), attributes).Sort(new string[] { "Rule", "Prob" });
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
            {
                return new LSystemRule() { Rule = (string)propertyValues["Rule"], Prob = (float)propertyValues["Prob"]};
            }
        }
    }

    public enum TrunkType
    {
        Single,
        Double,
        Crossed
    }
}
