using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utopia.Shared.Configuration;
using Utopia.Shared.RealmEditor;

namespace Utopia.Shared.LandscapeEntities
{
    public partial class LandscapeEntityStaticItem
    {
        [TypeConverter(typeof(StaticEntityConverter))] //Display Cube List
        [DisplayName("Entity")]
        public string EntityName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return EditorConfigHelper.Config.BluePrints[ItemblueprintId].Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                ItemblueprintId = EditorConfigHelper.Config.BluePrints.Values.First(x => x.Name == value).BluePrintId;
            }
        }
    }
}
