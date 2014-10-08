using System.ComponentModel;
using ProtoBuf;
using S33M3CoreComponents.Sound;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Sound;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Plays some music, can be switched off
    /// </summary>
    [ProtoContract]
    [Description("Plays some music, can be switched off")]
    public class MusicPlayer : OrientedBlockItem, IUsableEntity
    {
        [ProtoMember(1)]
        public bool Enabled { get; set; }

        [Category("Sound")]
        [Description("Sound played when entity is used")]
        [TypeConverter(typeof(FullSoundSelector))]
        [ProtoMember(2)]
        public StaticEntitySoundSource MusicTrack { get; set; }

        ISoundVoice _voice;

        public void Use()
        {
            Enabled = !Enabled;

            if (MusicTrack != null && SoundEngine != null)
            {
                if (Enabled)
                {
                    _voice = SoundEngine.StartPlay3D(MusicTrack, Position.AsVector3(), true);
                }
                else
                {
                    if (_voice != null)
                    {
                        _voice.Stop();
                        _voice = null;
                    }
                }
            }
        }
    }
}
