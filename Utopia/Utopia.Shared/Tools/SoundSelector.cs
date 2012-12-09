using System.ComponentModel;

namespace Utopia.Shared.Tools
{
    public class SoundSelector : StringConverter
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
    }
}