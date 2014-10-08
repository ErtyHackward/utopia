using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public partial class BiomeEntity
    {
        [TypeConverter(typeof(CubeConverter))] //Display Cube List
        [DisplayName("Entity")]
        public string EntityName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return EditorConfigHelper.Config.BluePrints[BluePrintId].Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                BluePrintId = EditorConfigHelper.Config.BluePrints.Values.First(x => x.Name == value).BluePrintId;
            }
        }

        internal class CubeConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                //true means show a combobox
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                //true will limit to list. false will show the list, 
                //but allow free-form entry
                return true;
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(EditorConfigHelper.Config.BluePrints.Values.Select(x => x.Name).OrderBy(x => x).ToList());
            }
        }

      

        public override string ToString()
        {
            return EntityName;
        }
    }
}
