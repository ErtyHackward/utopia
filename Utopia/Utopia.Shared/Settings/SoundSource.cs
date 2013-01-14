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

        [Category("General")]
        [Browsable(false)]
        [ProtoMember(5)]
        public SourceCategory Category { get; set; }

        [Category("General")]
        [Browsable(false)]
        [ProtoMember(6)]
        public bool isStreamed  { get; set; }


        public SoundSource()
        {
            Volume = 1.0f;
            Power = 16.0f;
            Category = SourceCategory.FX;
            isStreamed = false;
        }
    }
}
