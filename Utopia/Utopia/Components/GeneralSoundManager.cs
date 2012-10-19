using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3DXEngine.Debug.Interfaces;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Sound;

namespace Utopia.Components
{
    /// <summary>
    /// Class to manage, sound not linked to game (Like gui button click, ....)
    /// </summary>
    public class GeneralSoundManager : GameComponent, IDebugInfo
    {
        private readonly ISoundEngine _soundEngine;

        private string _buttonPressSound;
        private string _debugInfo = null;

        /// <summary>
        /// Gets irrKlang sound engine object
        /// </summary>
        public ISoundEngine SoundEngine
        {
            get { return _soundEngine; }
        }

        public GeneralSoundManager(ISoundEngine soundEngine)
        {
            _soundEngine = soundEngine;
        }

        public void SetGuiButtonSound(string filePath)
        {
            if (_buttonPressSound == null)
                ButtonControl.PressedSome += PressableControlPressedSome;

            _buttonPressSound = filePath;
        }

        private void PressableControlPressedSome(object sender, EventArgs e)
        {
            _soundEngine.PlaySound(_buttonPressSound, 1.0f);
        }

        public override void BeforeDispose()
        {
            if (_buttonPressSound != null)
            {
                _buttonPressSound = null;
                ButtonControl.PressedSome -= PressableControlPressedSome;
            }
        }

        public bool ShowDebugInfo
        {
            get;
            set;
        }

        public string GetDebugInfo()
        {
            return _debugInfo;
        }
    }
}
