using System.ComponentModel;
using ProtoBuf;
using System;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    [Obsolete("Use ChunkSpawnableEntity instead !")]
    [ProtoContract]
    public partial class BiomeEntity
    {
        [Browsable(false)]
        [ProtoMember(1)]
        public ushort BluePrintId { get; set; }

        [ProtoMember(2)]
        public int EntityPerChunk { get; set; }

        [ProtoMember(3)]
        public double ChanceOfSpawning { get; set; }

        public BiomeEntity()
        {
            BluePrintId = 256; //Default first True entity
        }
    }
}
