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
    public partial class CubeVein
    {
        private byte _cubeId;

        [Browsable(false)]
        public byte CubeId { get { return _cubeId; } set { _cubeId = value; RefreshCubeProfile(); } }
        [Browsable(false)]
        public CubeProfile CubeProfile { get; set; }
        public string Name { get; set; }
        public int VeinSize { get; set; }
        public RangeB SpawningHeight { get; set; }
        public int VeinPerChunk { get; set; }
        public double ChanceOfSpawning { get; set; }


        private void RefreshCubeProfile()
        {
            CubeProfile = RealmConfiguration.CubeProfiles[_cubeId];
        }
    }
}
