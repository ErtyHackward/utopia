using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Components;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.Entities.Managers;
using Utopia.Shared.Chunks;

namespace Sandbox.Client.Components
{
    public class SandboxSoundManager : SoundManager
    {
        public SandboxSoundManager(CameraManager<ICameraFocused> cameraManager, DynamicEntityManager dynamicEntityManager)
            : base(cameraManager, dynamicEntityManager)
        {
            SetGuiButtonSound("Sounds\\Interface\\button_press.wav");
        }
    }
}
