using System.ComponentModel;
using System.Linq;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Tools
{
    /// <summary>
    /// Extends property grid to allow to select a set for a container
    /// </summary>
    public class ContainerSetSelector : StringConverter
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

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(Configuration.ContainerSets.Keys.ToArray());
        }

    }
}
