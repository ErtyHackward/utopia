using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utopia.Shared.Configuration;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public partial class BiomeEntity : IBinaryStorable
    {
        [Browsable(false)]
        public ushort EntityId { get; set; }
        public int EntityPerChunk { get; set; }
        public double ChanceOfSpawning { get; set; }
    }
}
