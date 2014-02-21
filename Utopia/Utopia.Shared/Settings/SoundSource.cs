using System.ComponentModel;
using ProtoBuf;
using S33M3CoreComponents.Sound;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Settings
{
    [ProtoContract]
    public class SoundSource : ISoundDataSourceBase
    {
        [Description("Sound file path, can be relative one"), Category("General")]
        [ProtoMember(1)]
        [TypeConverter(typeof(SoundList))]
        public string FilePath { get; set; }

        [Description("Sound alias name"), Category("General")]
        [Browsable(false)]
        [ProtoMember(2)]
        public string Alias { get; set; }

        [Description("Sound volume Coef. (1.0 = original file sound volume)"), Category("General")]
        [ProtoMember(3)]
        public float Volume { get; set; }

        [Description("The distance the sound is propagating (in meter), used only in 3D sound"), Category("General")]
        [ProtoMember(4)]
        public float Power { get; set; }

        [Description("Sound category, each category has a maximum pool of 32 sounds that can play at the same time"), Category("General")]
        [Browsable(true)]
        [ProtoMember(5)]
        public SourceCategory Category { get; set; }

        [Description("For using with large sound file, will reduce RAM needed"), Category("General")]
        [Browsable(true)]
        [ProtoMember(6)]
        public bool isStreamed  { get; set; }

        [Description("Sound playing priority (Higher = Most chance to have it played in situation where lot of sound are played at the same time"), Category("General")]
        [Browsable(true)]
        [ProtoMember(7)]
        public int Priority { get; set; }

        public SoundSource()
        {
            Volume = 1.0f;
            Power = 16.0f;
            Category = SourceCategory.FX;
            isStreamed = false;
            Priority = 0;
        }
    }
}
