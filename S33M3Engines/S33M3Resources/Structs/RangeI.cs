using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace S33M3_Resources.Structs
{
    [TypeConverter(typeof(RangeITypeConverter))]
    public struct RangeI
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public RangeI(int min, int max)
        : this()    
        {
            Min = min;
            Max = max;
        }

        //Property Grid editing Purpose
        internal class RangeITypeConverter : ExpandableObjectConverter 
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {   //This method is used to shown information in the PropertyGrid.
                if (destinationType == typeof(string))
                {
                    return ("Min : " + ((RangeI)value).Min + "; Max : " + ((RangeI)value).Max);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(RangeI), attributes).Sort(new string[] { "Min", "Max" });
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
                return new RangeI((int)propertyValues["Min"], (int)propertyValues["Max"]);
            }

        }

    }
}
