using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World.Processors.Utopia.LandformFct;
using Utopia.Shared.Settings;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    /// <summary>
    /// Contains all properties and methods related to Showing Biome information in GridProperties and serializing the informations
    /// </summary>
    public partial class Biome 
    {
        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Surface Cube"), Category("Composition")]
        public string SurfaceCubeName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return _config.BlockProfiles.First(x => x.Id == SurfaceCube).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                SurfaceCube = _config.BlockProfiles.First(x => x.Name == value).Id;
            }
        }

        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Under-surface Cube"), Category("Composition")]
        public string UnderSurfaceCubeName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return _config.BlockProfiles.First(x => x.Id == UnderSurfaceCube).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                UnderSurfaceCube = _config.BlockProfiles.First(x => x.Name == value).Id;
            }
        }

        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Ground Cube"), Category("Composition")]
        public string GroundCubeName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return _config.BlockProfiles.First(x => x.Id == GroundCube).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                GroundCube = _config.BlockProfiles.First(x => x.Name == value).Id;
            }
        }

        //Should only be used by Editor
        public Biome()
        {
            _config = EditorConfigHelper.Config;
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
                return new StandardValuesCollection(EditorConfigHelper.Config.BlockProfiles.Where(x => x != null).Select(x => x.Name).Where(x => x != "System Reserved").OrderBy(x => x).ToList());
            }
        }
    }
}
