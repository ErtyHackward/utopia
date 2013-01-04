using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Settings;
using Utopia.Shared.Sounds;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    [ProtoContract]
    public class BiomeSoundSource : SoundSource, IUtopiaSoundSource
    {
        [Description("Time of day when the ambient sound can be played"), Category("General")]
        [ProtoMember(1)]
        public TimeOfDaySound TimeOfDay { get; set; }
    }
}
