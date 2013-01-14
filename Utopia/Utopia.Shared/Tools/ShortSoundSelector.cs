using System.ComponentModel;

namespace Utopia.Shared.Tools
{
    public class ShortSoundSelector : StringConverter
    {
        public static string[] PossibleSound;

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(PossibleSound);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value == null) return null;
            return new Utopia.Shared.Entities.Sound.StaticEntitySoundSource()
            {
                FilePath = (string)value,
                Alias = (string)value
            };
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return true;
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if (value == null) return null;
            if (value is Utopia.Shared.Entities.Sound.StaticEntitySoundSource)
            {
                return ((Utopia.Shared.Entities.Sound.StaticEntitySoundSource)value).FilePath;
            }
            else
            {
                return (string)value;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            return  true;
        }
    }
}