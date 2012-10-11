using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World.Processors.Utopia.LandformFct;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    /// <summary>
    /// Contains all properties and methods related to Showing Biome information in GridProperties and serializing the informations
    /// </summary>
    public partial class Biome 
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
            BiomeTrees.Save(writer);
            writer.Write(TemperatureFilter.Min);
            writer.Write(TemperatureFilter.Max);
            writer.Write(MoistureFilter.Min);
            writer.Write(MoistureFilter.Max);

            writer.Write(LandFormFilters.Count);
            foreach (enuLandFormType landType in LandFormFilters)
            {
                writer.Write((int)landType);
            }

            writer.Write(Caverns.Count);
            foreach (Cavern cavern in Caverns)
            {
                cavern.Save(writer);
            }

            writer.Write(BiomeEntities.Count);
            foreach (BiomeEntity entity in BiomeEntities)
            {
                entity.Save(writer);
            }

            writer.Write(CubeVeins.Count);
            foreach (CubeVein cubeVein in CubeVeins)
            {
                cubeVein.Save(writer);
            }
        }

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();
            SurfaceCube = reader.ReadByte();
            UnderSurfaceCube = reader.ReadByte();
            UnderSurfaceLayers = new RangeI(reader.ReadInt32(), reader.ReadInt32());
            GroundCube = reader.ReadByte();
            BiomeTrees.Load(reader);
            TemperatureFilter = new RangeD(reader.ReadDouble(), reader.ReadDouble());
            MoistureFilter = new RangeD(reader.ReadDouble(), reader.ReadDouble());

            int nbrObjectsInCollection;

            LandFormFilters.Clear();
            nbrObjectsInCollection = reader.ReadInt32();
            for (int i = 0; i < nbrObjectsInCollection; i++)
            {
                LandFormFilters.Add((enuLandFormType)reader.ReadInt32());
            }

            Caverns.Clear();
            nbrObjectsInCollection = reader.ReadInt32();
            for (int i = 0; i < nbrObjectsInCollection; i++)
            {
                Cavern cavern = new Cavern();
                cavern.Load(reader);
                Caverns.Add(cavern);
            }

            BiomeEntities.Clear();
            nbrObjectsInCollection = reader.ReadInt32();
            for (int i = 0; i < nbrObjectsInCollection; i++)
            {
                BiomeEntity entity = new BiomeEntity();
                entity.Load(reader);
                BiomeEntities.Add(entity);
            }

            CubeVeins.Clear();
            nbrObjectsInCollection = reader.ReadInt32();
            for (int i = 0; i < nbrObjectsInCollection; i++)
            {
                CubeVein vein = new CubeVein();
                vein.Load(reader);
                CubeVeins.Add(vein);
            }

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
