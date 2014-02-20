using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Tools
{
    public class CubeSelector : StringConverter
    {
        public static WorldConfiguration Configuration { get; set; }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if ((string)value == "Choose...")
                return (byte)0;

            var blueprint = Configuration.BlockProfiles.FirstOrDefault(bp => bp.Name == (string)value);
            return blueprint == null ? (byte)0 : blueprint.Id;

        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if (value is string)
                return value;
            
            var blueprint = Configuration.BlockProfiles[(byte)value];
            return blueprint.Name;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(Configuration.BlockProfiles.Select(bp => bp.Name).Where(s => s != "System Reserved").ToArray());
        }
    }
}
