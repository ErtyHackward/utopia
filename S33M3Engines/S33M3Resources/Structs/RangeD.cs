using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace S33M3_Resources.Structs
{
    [TypeConverter(typeof(RangeDTypeConverter))]
    public struct RangeD
    {
        public double Min;
        public double Max;

        public RangeD(double min, double max)
        {
            Min = min;
            Max = max;
        }

        //Property Grid editing Purpose
        internal class RangeDTypeConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {   //This method is used to shown information in the PropertyGrid.
                if (destinationType == typeof(string))
                {
                    return ("Min : " + ((RangeD)value).Min.ToString("0.00") + "; Max : " + ((RangeD)value).Max.ToString("0.00"));
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(RangeD), attributes).Sort(new string[] { "Min", "Max" });
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
                return new RangeD((double)propertyValues["Min"], (double)propertyValues["Max"]);
            }

        }
    }
}
