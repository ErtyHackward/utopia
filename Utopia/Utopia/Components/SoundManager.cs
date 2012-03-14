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
        private Vector3D _listenerPosition;

        private readonly SortedList<string, KeyValuePair<ISound, List<Vector3D>>> _sharedSounds = new SortedList<string, KeyValuePair<ISound, List<Vector3D>>>();
        
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
            _listenerPosition = new Vector3D((float)_cameraManager.ActiveCamera.WorldPosition.X,
                                   (float) _cameraManager.ActiveCamera.WorldPosition.Y,
                                   (float) _cameraManager.ActiveCamera.WorldPosition.Z);
            var lookAt = new Vector3D(_cameraManager.ActiveCamera.LookAt.X, _cameraManager.ActiveCamera.LookAt.Y,
                                      _cameraManager.ActiveCamera.LookAt.Z);
            _soundEngine.SetListenerPosition(_listenerPosition, lookAt);
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

            PlayClosestSound();
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
                Vector3D soundPos;
                soundPos.X = position.X + 0.5f;
                soundPos.Y = position.Y + 0.5f;
                soundPos.Z = position.Z + 0.5f;

                for (int i = _sharedSounds.Count - 1; i >= 0; i--)
                {
                    for (int j = _sharedSounds.Values[i].Value.Count - 1; j >= 0; j--)
                    {
                        var pos = _sharedSounds.Values[i].Value[j];
                        if (pos == soundPos)
                        {
                            _sharedSounds.Values[i].Value.RemoveAt(j);
                        }
                    }

                    if (_sharedSounds.Values[i].Value.Count == 0)
                    {
                        _sharedSounds.Values[i].Key.Stop();
                        _sharedSounds.RemoveAt(i);
                    }
                }
            }

            // add new sounds
            foreach (var position in range.AllExclude(_lastRange))
            {
                if (_singleArray.GetCube(position).Id == CubeId.Water)
                {
                    var soundPosition = new Vector3D(position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f);
                    
                    if (_sharedSounds.ContainsKey("Sounds\\Ambiance\\water_stream.ogg"))
                        _sharedSounds["Sounds\\Ambiance\\water_stream.ogg"].Value.Add(soundPosition);
                    else
                    {
                        var sound = _soundEngine.Play3D("Sounds\\Ambiance\\water_stream.ogg", soundPosition, true, false, StreamMode.AutoDetect);
                        _sharedSounds.Add("Sounds\\Ambiance\\water_stream.ogg", new KeyValuePair<ISound, List<Vector3D>>(sound, new List<Vector3D>{ soundPosition }));
                    }
                }
            }

            _lastRange = range;
        }

        /// <summary>
        /// Select the closest sound position
        /// </summary>
        public void PlayClosestSound()
        {
            foreach (var pair in _sharedSounds)
            {
                // only one position available
                if (pair.Value.Value.Count == 1) continue;

                // choose the closest
                var distance = _listenerPosition.GetDistanceFrom(pair.Value.Value[0]);
                var position = pair.Value.Value[0];

                for (int i = 1; i < pair.Value.Value.Count; i++)
                {
                    var d = _listenerPosition.GetDistanceFrom(pair.Value.Value[i]);
                    if (d < distance)
                    {
                        position = pair.Value.Value[i];
                        distance = d;
                    }
                }

                pair.Value.Key.Position = position;
            }
        }




    }
}
