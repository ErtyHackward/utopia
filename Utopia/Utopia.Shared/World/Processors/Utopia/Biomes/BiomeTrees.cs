using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    [TypeConverter(typeof(BiomeTreesTypeConverter))]
    public class BiomeTrees
    {
        //Tree distribution in %, Total must be <= 100
        public double Small { get; set; }
        public double Medium { get; set; }
        public double Big { get; set; }
        public int Cactus { get; set; }

        public RangeI TreePerChunks { get; set; }

        public BiomeTrees()
        {
            Small = 0.5;
            Medium = 0.35;
            Big = 0.15;
            Cactus = 0;
            TreePerChunks = new RangeI(0, 0);
        }

        //Property Grid editing Purpose
        internal class BiomeTreesTypeConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {   //This method is used to shown information in the PropertyGrid.
                if (destinationType == typeof(string))
                {
                    return ("Tree distributions");
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(BiomeTrees), attributes).Sort(new string[] { "Small", "Medium", "Big", "Cactus" });
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}
