using System;
using System.Collections.Generic;
using IrrKlang;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Cubes;
using Utopia.Shared.Structs;
using Vector3D = IrrKlang.Vector3D;

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
        private SingleArrayChunkContainer _singleArray;

        private Vector3I _lastPosition;
        private Range3 _lastRange;

        private readonly Dictionary<Vector3I, ISound> _activeSounds = new Dictionary<Vector3I, ISound>();
        
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

        public void LateInitialization(SingleArrayChunkContainer singleArray)
        {
            if (singleArray == null) throw new ArgumentNullException("singleArray");
            _singleArray = singleArray;
        }

        public void SetGuiButtonSound(string filePath)
        {
            if (_buttonPressSound == null)
                ButtonControl.PressedSome += PressableControlPressedSome;

            _buttonPressSound = filePath;
        }

        private void PressableControlPressedSome(object sender, EventArgs e)
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

        public override void Update(GameTime timeSpent)
        {
            var pos = new Vector3D((float) _cameraManager.ActiveCamera.WorldPosition.X,
                                   (float) _cameraManager.ActiveCamera.WorldPosition.Y,
                                   (float) _cameraManager.ActiveCamera.WorldPosition.Z);
            var lookAt = new Vector3D(_cameraManager.ActiveCamera.LookAt.X, _cameraManager.ActiveCamera.LookAt.Y,
                                      _cameraManager.ActiveCamera.LookAt.Z);
            _soundEngine.SetListenerPosition(pos, lookAt);
            _soundEngine.Update();

            // update all sounds
            if ((Vector3I)_cameraManager.ActiveCamera.WorldPosition != _lastPosition)
            {
                _lastPosition = (Vector3I)_cameraManager.ActiveCamera.WorldPosition;

                Range3 listenRange;

                listenRange.Position = _lastPosition - new Vector3I(8,8,8);

                listenRange.Size = new Vector3I(16,16,16);

                ListenCubes(listenRange);
            }
        }

        /// <summary>
        /// Update cubes that emit sounds
        /// </summary>
        /// <param name="range"></param>
        public void ListenCubes(Range3 range)
        {
            // remove sounds that are far away
            foreach (var position in _lastRange.AllExclude(range))
            {
                ISound sound;
                if (_activeSounds.TryGetValue(position, out sound))
                {
                    sound.Stop();
                    _activeSounds.Remove(position);
                }
            }

            // add new sounds
            foreach (var position in range.AllExclude(_lastRange))
            {
                if (_singleArray.GetCube(position).Id == CubeId.Water)
                {
                    var sound = _soundEngine.Play3D("Sounds\\Ambiance\\water_stream.ogg", position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f, true);
                    _activeSounds.Add(position, sound);
                }
            }

            _lastRange = range;
        }

    }
}
