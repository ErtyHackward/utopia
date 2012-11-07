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

            //Add specific Channel Mapping matrix, it will overide default mapping
            float[] mapping;
            //mono sound track ==> 5.1 speaker Mapping (instead of playing the mono track to left and right speaker only)
            //              Input
            // Output       Mono
            // Front Left   1.0f
            // Front Right  1.0f
            // Front Center 0.5f
            // LowFreq      0.0f
            // Side Left    0.3f
            // Side Right   0.3f
            mapping = new float[6] { 1.0f, 1.0f, 0.5f, 0.0f, 0.3f, 0.3f };
            base.SoundEngine.AddCustomChannelMapping(1, 6, mapping);

            //stereo sound track ==> 5.1 speaker Mapping (instead of playing the stereo track to left and right speaker only)
            //              Input
            // Output       Left        Right
            // Front Left   1.0f        0.0f
            // Front Right  0.0f        1.0f
            // Front Center 0.3f        0.3f
            // LowFreq      0.0f        0.0f
            // Side Left    0.8f        0.0f
            // Side Right   0.0f        0.8f
            mapping = new float[12] { 1.0f, 0.0f, 0.0f, 1.0f, 0.3f, 0.3f, 0.0f, 0.0f, 0.8f, 0.0f, 0.0f, 0.8f };
            base.SoundEngine.AddCustomChannelMapping(2, 6, mapping);
        }
    }
}
