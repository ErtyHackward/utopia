using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Utopia.Shared.Configuration;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    /// <summary>
    /// Contains all properties and methods related to Showing Biome information in GridProperties and serializing the informations
    /// </summary>
    public partial class Biome : IBinaryStorable
    {
        #region Private Variables
        #endregion

        #region Public Properties
        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Surface Cube"), Category("Composition")]
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
        [DisplayName("Under-surface Cube"), Category("Composition")]
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
        [DisplayName("Ground Cube"), Category("Composition")]
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
        #endregion

        #region Public Methods
        public void Save(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(SurfaceCube);
            writer.Write(UnderSurfaceCube);
            writer.Write(UnderSurfaceLayers.Min);
            writer.Write(UnderSurfaceLayers.Max);
            writer.Write(GroundCube);
        }

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();
            SurfaceCube = reader.ReadByte();
            UnderSurfaceCube = reader.ReadByte();
            _underSurfaceLayers.Min = reader.ReadInt32();
            _underSurfaceLayers.Max = reader.ReadInt32();
            GroundCube = reader.ReadByte();
        }
        #endregion

        #region Private Methods
        #endregion

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
