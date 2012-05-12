using IrrKlang;
using Utopia.Components;

namespace Realms.Client.Components
{
    public class SandboxGeneralSoundManager : GeneralSoundManager
    {
        public SandboxGeneralSoundManager(ISoundEngine soundEngine)
            : base(soundEngine)
        {
            SetGuiButtonSound("Sounds\\Interface\\button_press.wav");
        }

    }
}
