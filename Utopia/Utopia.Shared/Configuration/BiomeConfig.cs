using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utopia.Shared.Settings;
using Utopia.Shared.World.Processors.Utopia.Biomes;

namespace Utopia.Shared.Configuration
{
    /// <summary>
    /// Special Class that will wrap the biome class in order to support special PropertyGrid behaviours (Property in drop down list, ....)
    /// </summary>
    public class BiomeConfig : Biome
    {

        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Surface Cube")]
        public string SurfaceCubeName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return RealmConfiguration.CubeProfiles.First(x => x.Id == SurfaceCube).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                SurfaceCube = RealmConfiguration.CubeProfiles.First(x => x.Name == value).Id;
            }
        }

        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Under-surface Cube")]
        public string UnderSurfaceCubeName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return RealmConfiguration.CubeProfiles.First(x => x.Id == UnderSurfaceCube).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                UnderSurfaceCube = RealmConfiguration.CubeProfiles.First(x => x.Name == value).Id;
            }
        }

        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Ground Cube")]
        public string GroundCubeName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return RealmConfiguration.CubeProfiles.First(x => x.Id == GroundCube).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                GroundCube = RealmConfiguration.CubeProfiles.First(x => x.Name == value).Id;
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
                return new StandardValuesCollection(RealmConfiguration.CubeProfiles.Select(x => x.Name).Where(x => x != "System Reserved").OrderBy(x => x).ToList());
            }
        }
    }
}
