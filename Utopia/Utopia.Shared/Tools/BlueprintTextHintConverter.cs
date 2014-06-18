using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Tools
{
    public class BlueprintTextHintConverter : TypeConverter
    {
        public static WorldConfiguration Configuration { get; set; }
        public static Dictionary<string, Image> Images { get; set; }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if (value is string)
                return value;

            ushort bpId;

            if (value is byte)
            {
                bpId = (byte)value;
            }
            else
            {
                bpId = (ushort)value;
            }

            if (bpId == 0)
                return "Choose...";

            if (bpId < 256)
            {
                return Configuration.BlockProfiles[bpId].Name;
            }

            return Configuration.BluePrints[bpId].Name;
        }
    }
}