using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public partial class BiomeTree
    {
        [TypeConverter(typeof(TreeConverter))]
        [DisplayName("Tree")]
        public string Tree
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                if (EditorConfigHelper.Config.TreeBluePrints.Exists(x => x.Id == LandscapeEntityBluePrintId) == false)
                {
                    LandscapeEntityBluePrintId = EditorConfigHelper.Config.TreeBluePrints[0].Id;
                }
                return EditorConfigHelper.Config.TreeBluePrints.First(x => x.Id == LandscapeEntityBluePrintId).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                LandscapeEntityBluePrintId = EditorConfigHelper.Config.TreeBluePrints.First(x => x.Name == value).Id;
            }
        }

        internal class TreeConverter : StringConverter
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
                return new StandardValuesCollection(EditorConfigHelper.Config.TreeBluePrints.OrderBy(x => x.Name).ToList());
            }
        }
    }
}
