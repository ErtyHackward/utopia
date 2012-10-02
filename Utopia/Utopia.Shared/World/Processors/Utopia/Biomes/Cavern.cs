using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Settings;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class Cavern
    {
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

        [Browsable(false)]
        public byte CubeId { get; set; }
        public string Name { get; set; }
        public RangeB CavernHeightSize { get; set; }
        public RangeB SpawningHeight { get; set; }
        public int CavernPerChunk { get; set; }
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
                return new StandardValuesCollection(RealmConfiguration.CubeProfiles.Select(x => x.Name).Where(x => x != "System Reserved").OrderBy(x => x).ToList());
            }
        }
    }
}
