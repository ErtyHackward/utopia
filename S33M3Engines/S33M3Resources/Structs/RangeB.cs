using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace S33M3_Resources.Structs
{
    [TypeConverter(typeof(RangeBTypeConverter))]
    public struct RangeB
    {
        public byte Min { get; set; }
        public byte Max { get; set; }

        public RangeB(byte min, byte max)
            : this()
        {
            Min = min;
            Max = max;
        }

        //Property Grid editing Purpose
        public class RangeBTypeConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {   //This method is used to shown information in the PropertyGrid.
                if (destinationType == typeof(string))
                {
                    return ("Min : " + ((RangeB)value).Min + "; Max : " + ((RangeB)value).Max);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(RangeB), attributes).Sort(new string[] { "Min", "Max" });
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
                return new RangeB((byte)propertyValues["Min"], (byte)propertyValues["Max"]);
            }

        }
    }
}
