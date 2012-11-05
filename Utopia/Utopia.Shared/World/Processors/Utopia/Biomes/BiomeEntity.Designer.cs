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
        #region Private Variables
        #endregion

        #region Public Properties
        [TypeConverter(typeof(CubeConverter))] //Display Cube List
        [DisplayName("Entity")]
        public string EntityName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return EditorConfigHelper.Config.Entities.First(x => x.BluePrintId == BluePrintId).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                BluePrintId = EditorConfigHelper.Config.Entities.First(x => x.Name == value).BluePrintId;
            }
        }
        #endregion

        #region Public Methods
        public void Save(System.IO.BinaryWriter writer)
        {
            writer.Write(BluePrintId);
            writer.Write(EntityPerChunk);
            writer.Write(ChanceOfSpawning);
        }

        public void Load(System.IO.BinaryReader reader)
        {
            BluePrintId = reader.ReadUInt16();
            EntityPerChunk = reader.ReadInt32();
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
                return new StandardValuesCollection(EditorConfigHelper.Config.Entities.Select(x => x.Name).OrderBy(x => x).ToList());
            }
        }
    }
}
