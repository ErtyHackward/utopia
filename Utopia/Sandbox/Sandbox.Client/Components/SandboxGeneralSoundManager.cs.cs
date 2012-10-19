using S33M3CoreComponents.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Components;

namespace Sandbox.Client.Components
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
