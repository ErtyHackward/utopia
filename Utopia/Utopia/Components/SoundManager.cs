using System;
using IrrKlang;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;

namespace Utopia.Components
{
    /// <summary>
    /// Wrapper around irrKlang library to provide sound playback
    /// Used to play all sound media
    /// </summary>
    public class SoundManager : GameComponent
    {
        private readonly CameraManager<ICameraFocused> _cameraManager;
        private readonly ISoundEngine _soundEngine;

        private string _buttonPressSound;

        /// <summary>
        /// Gets irrKlang sound engine object
        /// </summary>
        public ISoundEngine SoundEngine
        {
            get { return _soundEngine; }
        }

        public SoundManager(CameraManager<ICameraFocused> cameraManager)
        {
            if (cameraManager == null) throw new ArgumentNullException("cameraManager");
            _cameraManager = cameraManager;
            _soundEngine = new ISoundEngine();
        }

        public void SetGuiButtonSound(string filePath)
        {
            if (_buttonPressSound == null)
                ButtonControl.PressedSome += PressableControlPressedSome;

            _buttonPressSound = filePath;
        }

        void PressableControlPressedSome(object sender, EventArgs e)
        {
            _soundEngine.Play2D(_buttonPressSound);
        }

        public override void Dispose()
        {
            _soundEngine.Dispose();

            if (_buttonPressSound != null)
            {
                _buttonPressSound = null;
                ButtonControl.PressedSome -= PressableControlPressedSome;
            }
        }

        public override void Update( GameTime timeSpend)
        {
            Vector3D pos = new Vector3D((float)_cameraManager.ActiveCamera.WorldPosition.X, (float)_cameraManager.ActiveCamera.WorldPosition.Y, (float)_cameraManager.ActiveCamera.WorldPosition.Z);
            Vector3D lookAt = new Vector3D(_cameraManager.ActiveCamera.LookAt.X, _cameraManager.ActiveCamera.LookAt.Y, _cameraManager.ActiveCamera.LookAt.Z);
            _soundEngine.SetListenerPosition(pos, lookAt);
            _soundEngine.Update();
        }

    }
}
