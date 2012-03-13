using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Components;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;

namespace Sandbox.Client.Components
{
    public class SandboxSoundManager : SoundManager
    {
        public SandboxSoundManager(CameraManager<ICameraFocused> cameraManager)
            :base(cameraManager)
        {
            SetGuiButtonSound("Sounds\\Interface\\button_press.wav");
        }
    }
}
