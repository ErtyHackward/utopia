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
    /// Special Class that will wrap the biome class in order to support special PropertyGrid behaviours (Property in dorp down list, ....)
    /// </summary>
    public class BiomeConfig : Biome
    {
        private static List<CubeProfile> _cubeProfiles;

        public BiomeConfig(List<CubeProfile> cubeProfiles)
        {
            _cubeProfiles = cubeProfiles;
        }

        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Surface Cube")]
        public string SurfaceCubeName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return _cubeProfiles.First(x => x.Id == SurfaceCube).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                SurfaceCube = _cubeProfiles.First(x => x.Name == value).Id;
            }
        }

        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Under-surface Cube")]
        public string UnderSurfaceCubeName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return _cubeProfiles.First(x => x.Id == UnderSurfaceCube).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                UnderSurfaceCube = _cubeProfiles.First(x => x.Name == value).Id;
            }
        }

        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Ground Cube")]
        public string GroundCubeName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return _cubeProfiles.First(x => x.Id == GroundCube).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                GroundCube = _cubeProfiles.First(x => x.Name == value).Id;
            }
        }

        public class CubeConverter : StringConverter
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

            public override System.ComponentModel.TypeConverter.StandardValuesCollection
                   GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(_cubeProfiles.Select(x => x.Name).Where(x => x != "System Reserved").OrderBy(x => x).ToList());
            }
        }
    }
}
