using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Utopia.Shared.Tools
{
    /// <summary>
    /// Allows to convinient choose of the state of the model
    /// </summary>
    public class ModelStateConverter : StringConverter
    {
        public static string[] PossibleValues;

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
            return new StandardValuesCollection(PossibleValues);
        }

    }
}