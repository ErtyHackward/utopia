using System.ComponentModel;
using System.Linq;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Dynamic;

namespace Utopia.Shared.Tools
{
    public class BlueprintSelector : StringConverter
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
                return 0;

            var blueprint = Configuration.BluePrints.FirstOrDefault(bp => bp.Value.Name == (string)value);
            return blueprint.Key;

        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if (value is string)
                return value;

            if ((ushort)value == 0)
                return "Choose...";

            var blueprint = Configuration.BluePrints[(ushort)value];
            return blueprint.Name;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(Configuration.BluePrints.Select(bp => bp.Value.Name).ToArray());
        }
    }

    public class CharacterBlueprintSelector : StringConverter
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
                return 0;

            var blueprint = Configuration.BluePrints.FirstOrDefault(bp => bp.Value.Name == (string)value);
            return blueprint.Key;

        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if (value is string)
                return value;

            if ((ushort)value == 0)
                return "Choose...";

            var blueprint = Configuration.BluePrints[(ushort)value];
            return blueprint.Name;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(Configuration.BluePrints.Where(bp => bp.Value is CharacterEntity).Select(bp => bp.Value.Name).ToArray());
        }
    }
}