using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_Resources.Structs;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public struct Cavern
    {
        public byte CubeId;
        public RangeB CavernHeightSize;
        public RangeB SpawningHeight;
        public int CavernPerChunk;
        public double ChanceOfSpawning;
    }
}
