using System;
using IrrKlang;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using SharpDX;

namespace Utopia.Components
{
    /// <summary>
    /// Wrapper around irrKlang library to provide sound playback
    /// Used to play all sound media
    /// </summary>
    public class SoundManager : GameComponent
    {
        private readonly CameraManager _cameraManager;
        private readonly ISoundEngine _soundEngine;

        private string _buttonPressSound;

        /// <summary>
        /// Gets irrKlang sound engine object
        /// </summary>
        public ISoundEngine SoundEngine
        {
            get { return _soundEngine; }
        }

        public SoundManager(CameraManager cameraManager)
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

        public override void Update(ref GameTime timeSpent)
        {
            var pos = new Vector3D((float)_cameraManager.ActiveCamera.WorldPosition.X, (float)_cameraManager.ActiveCamera.WorldPosition.Y, (float)_cameraManager.ActiveCamera.WorldPosition.Z);
            var rotationMatrix = Matrix.RotationQuaternion(_cameraManager.ActiveCamera.Orientation);
            var lookAt = new Vector3D(rotationMatrix.M13, rotationMatrix.M23, rotationMatrix.M33);
            
            _soundEngine.SetListenerPosition(pos, lookAt);
            _soundEngine.Update();
        }

    }
}
