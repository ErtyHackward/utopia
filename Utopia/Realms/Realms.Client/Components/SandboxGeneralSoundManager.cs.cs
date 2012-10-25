using S33M3CoreComponents.Sound;
using Utopia.Components;

namespace Realms.Client.Components
{
    public class SandboxGeneralSoundManager : GeneralSoundManager
    {
        public SandboxGeneralSoundManager(ISoundEngine soundEngine)
            : base(soundEngine)
        {
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            SetGuiButtonSound(@"Sounds\Interface\button_press.wav");
            base.LoadContent(context);
        }
    }
}
