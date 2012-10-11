using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Settings;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public partial class CubeVein : IBinaryStorable
    {
        private byte _cubeId;
        private CubeProfile _cubeProfile;

        [Browsable(false)]
        public byte CubeId { get { return _cubeId; } set { _cubeId = value; RefreshCubeProfile(); } }
        [Browsable(false)]
        public CubeProfile CubeProfile { get { return _cubeProfile; } }
        public string Name { get; set; }
        public int VeinSize { get; set; }
        public RangeB SpawningHeight { get; set; }
        public int VeinPerChunk { get; set; }
        [Browsable(false)]
        public double ChanceOfSpawning { get; set; }

        private void RefreshCubeProfile()
        {
            _cubeProfile = RealmConfiguration.CubeProfiles[_cubeId];
        }
    }
}
