using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class BiomeEntity
    {
        [TypeConverter(typeof(CubeConverter))]
        [DisplayName("Entity")]
        public string EntityName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                return RealmConfiguration.Entities.First(x => x.Id == EntityId).Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                EntityId = RealmConfiguration.Entities.First(x => x.Name == value).Id;
            }
        }

        public static BiomeEntity None = new BiomeEntity() { EntityId = 0, ChanceOfSpawning = 0.0, EntityPerChunk = 0 };

        [Browsable(false)]
        public ushort EntityId { get; set; }
        public int EntityPerChunk { get; set; }
        public double ChanceOfSpawning { get; set; }

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
                return new StandardValuesCollection(RealmConfiguration.Entities.Select(x => x.Name).OrderBy(x => x).ToList());
            }
        }
    }
}
