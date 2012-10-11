using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public partial class Cavern
    {
        #region Private Variables
        #endregion

        #region Public Properties
        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Cube")]
        public string CubeName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return RealmConfiguration.CubeProfiles.First(x => x.Id == CubeId).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                CubeId = RealmConfiguration.CubeProfiles.First(x => x.Name == value).Id;
            }
        }
        #endregion

        #region Public Methods
        public void Save(System.IO.BinaryWriter writer)
        {
            writer.Write(CubeId);
            writer.Write(Name);
            writer.Write(CavernHeightSize.Min);
            writer.Write(CavernHeightSize.Max);
            writer.Write(SpawningHeight.Min);
            writer.Write(SpawningHeight.Max);
            writer.Write(CavernPerChunk);
            writer.Write(ChanceOfSpawning);
        }

        public void Load(System.IO.BinaryReader reader)
        {
            CubeId = reader.ReadByte();
            Name = reader.ReadString();
            CavernHeightSize = new RangeB(reader.ReadByte(), reader.ReadByte());
            SpawningHeight = new RangeB(reader.ReadByte(), reader.ReadByte());
            CavernPerChunk = reader.ReadInt32();
            ChanceOfSpawning = reader.ReadDouble();
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
