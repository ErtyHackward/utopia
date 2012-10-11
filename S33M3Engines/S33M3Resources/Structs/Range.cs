using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace S33M3Resources.Structs
{
    [TypeConverter(typeof(RangeTypeConverter))]
    public struct Range
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public Range(float min, float max)
            : this()    
        {
            Min = min;
            Max = max;
        }

        //Property Grid editing Purpose
        internal class RangeTypeConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {   //This method is used to shown information in the PropertyGrid.
                if (destinationType == typeof(string))
                {
                    return ("Min : " + ((Range)value).Min.ToString("0.00") + "; Max : " + ((Range)value).Max.ToString("0.00"));
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(Range), attributes).Sort(new string[] { "Min", "Max" });
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
                return new Range((float)propertyValues["Min"], (float)propertyValues["Max"]);
            }

        }
    }
}
