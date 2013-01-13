using System;
using S33M3DXEngine.Main;
using S33M3DXEngine.Debug.Interfaces;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Sound;

namespace Utopia.Sounds
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
            this.IsSystemComponent = true;
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            _soundEngine.AddSoundSourceFromFile(_buttonPressSound, "ButtonPressed", SourceCategory.FX).SoundVolume = 0.3f;
        }

        public void SetGuiButtonSound(string filePath)
        {
            if (_buttonPressSound == null) ButtonControl.PressedSome += PressableControlPressedSome;
            _buttonPressSound = filePath;
        }

        private void PressableControlPressedSome(object sender, EventArgs e)
        {
            _soundEngine.StartPlay2D("ButtonPressed", SourceCategory.FX);
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
