using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Settings
{
    [ProtoContract]
    public class SoundSource
    {
        [Description("Sound file path, can be relative one"), Category("General")]
        [ProtoMember(1)]
        [TypeConverter(typeof(SoundSelector))]
        public string SoundFilePath { get; set; }

        [Description("Sound alias name"), Category("General")]
        [Browsable(false)]
        [ProtoMember(2)]
        public string SoundAlias { get; set; }

        [Description("Sound volume Coef. (1.0 = original file sound volume)"), Category("General")]
        [ProtoMember(3)]
        public float DefaultVolume { get; set; }

        [Description("The distance the sound is propagating (in meter), used only in 3D sound"), Category("General")]
        [ProtoMember(4)]
        public float Power { get; set; }

        public SoundSource()
        {
            DefaultVolume = 1.0f;
            Power = 16.0f;
        }

    }
}
