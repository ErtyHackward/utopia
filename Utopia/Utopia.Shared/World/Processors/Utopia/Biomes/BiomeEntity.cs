using System.ComponentModel;
using ProtoBuf;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
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
    }
}
