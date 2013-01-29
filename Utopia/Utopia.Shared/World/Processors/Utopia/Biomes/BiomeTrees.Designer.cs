using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public partial class BiomeTrees
    {
        //Property Grid editing Purpose
        public class BiomeTreesTypeConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {   //This method is used to shown information in the PropertyGrid.
                if (destinationType == typeof(string))
                {
                    return ("Trees spawning config");
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
