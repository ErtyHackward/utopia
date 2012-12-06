using System.ComponentModel;
using ProtoBuf;
using S33M3Resources.Structs;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    [ProtoContract]
    public partial class Cavern
    {
        [Browsable(false)]
        [ProtoMember(1)]
        public byte CubeId { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public RangeB CavernHeightSize { get; set; }

        [ProtoMember(4)]
        public RangeB SpawningHeight { get; set; }

        [ProtoMember(5)]
        public int CavernPerChunk { get; set; }

        [ProtoMember(6)]
        public double ChanceOfSpawning { get; set; }
    }
}
