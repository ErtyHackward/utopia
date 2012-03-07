using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Components;

namespace Sandbox.Client.Components
{
    public class SandboxSoundManager : SoundManager
    {
        public SandboxSoundManager()
        {
            SetGuiButtonSound("Sounds\\Interface\\button_press.wav");
        }
    }
}
