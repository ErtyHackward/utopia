using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Settings;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public partial class Cavern : IBinaryStorable
    {
        [Browsable(false)]
        public byte CubeId { get; set; }
        public string Name { get; set; }
        public RangeB CavernHeightSize { get; set; }
        public RangeB SpawningHeight { get; set; }
        public int CavernPerChunk { get; set; }
        public double ChanceOfSpawning { get; set; }
    }
}
