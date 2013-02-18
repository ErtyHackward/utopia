﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.LandscapeEntities.Trees
{
    public partial class TreeBluePrint
    {
        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Trunk Block")]
        [XmlIgnoreAttribute()]
        public string TrunkBlockName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                byte currentId = TrunkBlock;
                return EditorConfigHelper.Config.BlockProfiles.First(x => x.Id == currentId).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                TrunkBlock = EditorConfigHelper.Config.BlockProfiles.First(x => x.Name == value).Id;
            }
        }

        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Foliage Block")]
        [XmlIgnoreAttribute()]
        public string FoliageBlockName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                byte currentId = FoliageBlock;
                return EditorConfigHelper.Config.BlockProfiles.First(x => x.Id == currentId).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                FoliageBlock = EditorConfigHelper.Config.BlockProfiles.First(x => x.Name == value).Id;
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
                return new StandardValuesCollection(EditorConfigHelper.Config.BlockProfiles.Select(x => x.Name).Where(x => x != "System Reserved").OrderBy(x => x).ToList());
            }
        }
    }
}