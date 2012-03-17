using System;
using System.Collections.Generic;
using System.Diagnostics;
using IrrKlang;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3DXEngine.Debug.Interfaces;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3Resources.Structs;
using Utopia.Entities.Managers;
using Utopia.Shared.Chunks;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks.ChunkLandscape;
using IrrVector3 = IrrKlang.Vector3D;
using Vector3D = S33M3Resources.Structs.Vector3D;

namespace Utopia.Components
{
    /// <summary>
    /// Wrapper around irrKlang library to provide sound playback
    /// Used to play all sound media
    /// </summary>
    public class SoundManager : GameComponent, IDebugInfo
    {
        private readonly CameraManager<ICameraFocused> _cameraManager;
        private readonly DynamicEntityManager _dynamicEntityManager;
        private readonly ISoundEngine _soundEngine;
        private SingleArrayChunkContainer _singleArray;

        private Vector3I _lastPosition;
        private Range3 _lastRange;
        private IrrVector3 _listenerPosition;

        private readonly SortedList<string, KeyValuePair<ISound, List<IrrVector3>>> _sharedSounds = new SortedList<string, KeyValuePair<ISound, List<IrrVector3>>>();
        
        private string _buttonPressSound;
        private string _debugInfo;
        private long _listenCubesTime;

        private readonly List<KeyValuePair<IDynamicEntity, Vector3D>> _stepsTracker = new List<KeyValuePair<IDynamicEntity, Vector3D>>();
        private readonly Dictionary<byte, List<string>> _stepsSounds = new Dictionary<byte, List<string>>();


        /// <summary>
        /// Gets irrKlang sound engine object
        /// </summary>
        public ISoundEngine SoundEngine
        {
            get { return _soundEngine; }
        }

        public SoundManager(CameraManager<ICameraFocused> cameraManager, DynamicEntityManager dynamicEntityManager, IDynamicEntity player)
        {
            if (cameraManager == null) throw new ArgumentNullException("cameraManager");
            if (dynamicEntityManager == null) throw new ArgumentNullException("dynamicEntityManager");
            _cameraManager = cameraManager;
            _dynamicEntityManager = dynamicEntityManager;

            _dynamicEntityManager.EntityAdded += DynamicEntityManagerEntityAdded;
            _dynamicEntityManager.EntityRemoved += DynamicEntityManagerEntityRemoved;

            _soundEngine = new ISoundEngine();

            _stepsTracker.Add(new KeyValuePair<IDynamicEntity, Vector3D>(player, player.Position));

        }

        void DynamicEntityManagerEntityRemoved(object sender, Shared.Entities.Events.DynamicEntityEventArgs e)
        {
            _stepsTracker.RemoveAt(_stepsTracker.FindIndex(p => p.Key == e.Entity));
        }

        void DynamicEntityManagerEntityAdded(object sender, Shared.Entities.Events.DynamicEntityEventArgs e)
        {
            _stepsTracker.Add(new KeyValuePair<IDynamicEntity, Vector3D>(e.Entity, e.Entity.Position));
        }

        public void AddStepSound(byte blockId, string sound)
        {
            if (_stepsSounds.ContainsKey(blockId))
            {
                _stepsSounds[blockId].Add(sound);
            }
            else
            {
                _stepsSounds.Add(blockId, new List<string> { sound });
            }
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
            _listenerPosition = new IrrVector3((float)_cameraManager.ActiveCamera.WorldPosition.X,
                                   (float) _cameraManager.ActiveCamera.WorldPosition.Y,
                                   (float) _cameraManager.ActiveCamera.WorldPosition.Z);
            var lookAt = new IrrVector3(_cameraManager.ActiveCamera.LookAt.X, _cameraManager.ActiveCamera.LookAt.Y,
                                      -_cameraManager.ActiveCamera.LookAt.Z);

            var sw = Stopwatch.StartNew();

            _soundEngine.SetListenerPosition(_listenerPosition, lookAt);
            _soundEngine.Update();
            sw.Stop();

            _debugInfo = "Sounds playing: " + _sharedSounds.Count + ", Update " + sw.ElapsedMilliseconds + " ms, ";

            
            // update all cubes sounds
            if ((Vector3I)_cameraManager.ActiveCamera.WorldPosition != _lastPosition)
            {
                sw.Restart();
                _lastPosition = (Vector3I)_cameraManager.ActiveCamera.WorldPosition;

                Range3 listenRange;

                listenRange.Position = _lastPosition - new Vector3I(16,16,16);

                listenRange.Size = new Vector3I(32,32,32);

                ListenCubes(listenRange);
                sw.Stop();
                _listenCubesTime = sw.ElapsedMilliseconds;
            }

            sw.Restart();

            PlayClosestSound();
            sw.Stop();
            _debugInfo += " cubes:" + _listenCubesTime + " ms, select closest: " + sw.ElapsedMilliseconds;

            #region check for steps sounds

            var r = new Random();

            // foreach dynamic entity
            for (int i = 0; i < _stepsTracker.Count; i++)
            {
                var pair = _stepsTracker[i];
                var entity = pair.Key;

                // first let's detect if the entity is in air

                var underTheFeets = entity.Position;
                underTheFeets.Y -= 0.01f;
                
                var downCube = _singleArray.GetCube(underTheFeets);

                // no need to play step if the entity is in air
                if (downCube.Id == CubeId.Air)
                {
                    var item = new KeyValuePair<IDynamicEntity, Vector3D>(_stepsTracker[i].Key, entity.Position);
                    _stepsTracker[i] = item;
                    continue;
                }

                // possible that entity just landed after the jump, so we need to check if the entity was in the air last time to play the landing sound
                var prevUnderTheFeets = pair.Value;
                prevUnderTheFeets.Y -= 0.01f;
                var prevCube = _singleArray.GetCube(prevUnderTheFeets);

                double distance;

                distance = prevCube.Id == CubeId.Air ? 2.0f : Vector3D.Distance(pair.Value, entity.Position);

                // do we need to play the step sound?
                if (distance >= 2.0f)
                {
                    
                    var currentCube = _singleArray.GetCube(entity.Position);
                        
                    if (currentCube.Id == CubeId.Water && downCube.Id != CubeId.Water) 
                    {
                        var upCube = _singleArray.GetCube(entity.Position + new Vector3I(0, 1, 0));
                        if (upCube.Id == CubeId.Air)
                        {
                            List<string> sounds;
                            if (_stepsSounds.TryGetValue(currentCube.Id, out sounds))
                            {
                                //r.Next(0, sounds.Count)
                                _soundEngine.Play3D(sounds[0], (float) entity.Position.X, (float) entity.Position.Y, (float) entity.Position.Z);
                            }
                        }
                    }
                    else if (downCube.Id != CubeId.Air)
                    {
                        List<string> sounds;
                        if (_stepsSounds.TryGetValue(downCube.Id, out sounds))
                        {
                            _soundEngine.Play3D(sounds[r.Next(0, sounds.Count)], (float)entity.Position.X, (float)entity.Position.Y, (float)entity.Position.Z);
                        }
                    }
                    

                    var item = new KeyValuePair<IDynamicEntity, Vector3D>(_stepsTracker[i].Key, entity.Position);
                    _stepsTracker[i] = item;
                }
            }
            #endregion
        }

        /// <summary>
        /// Update cubes that emit sounds
        /// </summary>
        /// <param name="range"></param>
        public void ListenCubes(Range3 range)
        {
            // remove sounds that are far away
            
            for (int i = _sharedSounds.Count - 1; i >= 0; i--)
            {
                for (int j = _sharedSounds.Values[i].Value.Count - 1; j >= 0; j--)
                {
                    var pos = _sharedSounds.Values[i].Value[j];
                    var position = new Vector3I((int)pos.X, (int)pos.Y, (int)pos.Z);
                    if (!range.Contains(position))
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
            

            // add new sounds
            foreach (var position in range.AllExclude(_lastRange))
            {
                if (_singleArray.GetCube(position).Id == CubeId.Water)
                {
                    var soundPosition = new IrrVector3(position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f);
                    
                    if (_sharedSounds.ContainsKey("Sounds\\Ambiance\\water_stream.ogg"))
                        _sharedSounds["Sounds\\Ambiance\\water_stream.ogg"].Value.Add(soundPosition);
                    else
                    {
                        var sound = _soundEngine.Play3D("Sounds\\Ambiance\\water_stream.ogg", soundPosition, true, false, StreamMode.AutoDetect);
                        _sharedSounds.Add("Sounds\\Ambiance\\water_stream.ogg", new KeyValuePair<ISound, List<IrrVector3>>(sound, new List<IrrVector3> { soundPosition }));
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
