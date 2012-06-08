using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class BiomeEntity
    {
        public static BiomeEntity None = new BiomeEntity() { EntityId = 0, ChanceOfSpawning = 0.0, EntityPerChunk = 0 };

        public ushort EntityId;
        public int EntityPerChunk;
        public double ChanceOfSpawning;
    }
}
