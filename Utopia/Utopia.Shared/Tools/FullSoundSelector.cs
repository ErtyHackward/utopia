using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Sound;

namespace Utopia.Shared.Tools
{
    public class FullSoundSelector : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return false;
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return Activator.CreateInstance(Type.GetType((string)value));
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            ISoundDataSourceBase v = value as ISoundDataSourceBase;
            if (v == null || v.FilePath == null) return "";
            return v.FilePath;
        }
    }
}
