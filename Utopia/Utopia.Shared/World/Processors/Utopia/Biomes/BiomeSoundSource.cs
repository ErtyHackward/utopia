using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Settings;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    [ProtoContract]
    public class BiomeSoundSource : SoundSource
    {
        public enum TimeOfDaySound
        {
            FullDay = 0,
            Day = 1,
            Night = 2
        }

        [Description("Time of day when the ambient sound can be played"), Category("General")]
        [ProtoMember(1)]
        public TimeOfDaySound TimeOfDay { get; set; }
    }
}
