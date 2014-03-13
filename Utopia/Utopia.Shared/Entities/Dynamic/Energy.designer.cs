using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Entities.Dynamic
{
    public partial class Energy
    {
        //Property Grid editing Purpose
        public class EnergyTypeConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {   //This method is used to shown information in the PropertyGrid.
                if (destinationType == typeof(string))
                {
                    return ("Energy configuration");
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
