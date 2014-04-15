using System.ComponentModel;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Settings;
using Utopia.Shared.Tools;
using Utopia.Shared.Entities.Concrete;
using System.Drawing.Design;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    [ProtoContract]
    public partial class CubeVein
    {
        [ProtoMember(1)]
        [Editor(typeof(BlueprintTypeEditor<BlockProfile>), typeof(UITypeEditor))]
        [TypeConverter(typeof(BlueprintTextHintConverter))]
        public byte Cube { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public int VeinSize { get; set; }

        [ProtoMember(4)]
        public RangeB SpawningHeight { get; set; }

        [ProtoMember(5)]
        public int VeinPerChunk { get; set; }

        [Browsable(false)]
        [ProtoMember(6)]
        public double ChanceOfSpawning { get; set; }
    }
}
