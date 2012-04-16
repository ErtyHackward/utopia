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
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Settings;
using Utopia.Entities.Managers.Interfaces;

namespace Utopia.Components
{
    /// <summary>
    /// Wrapper around irrKlang library to provide sound playback
    /// Used to play all sound media
    /// </summary>
    public class GameSoundManager : GameComponent, IDebugInfo
    {
        private readonly CameraManager<ICameraFocused> _cameraManager;
        private IDynamicEntityManager _dynamicEntityManager;
        private readonly ISoundEngine _soundEngine;
        private SingleArrayChunkContainer _singleArray;

        private Vector3I _lastPosition;
        private Range3I _lastRange;
        private IrrVector3 _listenerPosition;

        private readonly SortedList<string, KeyValuePair<ISound, List<IrrVector3>>> _sharedSounds = new SortedList<string, KeyValuePair<ISound, List<IrrVector3>>>();
        
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

        public GameSoundManager(ISoundEngine soundEngine, 
                                CameraManager<ICameraFocused> cameraManager, 
                                SingleArrayChunkContainer singleArray, 
                                IDynamicEntityManager dynamicEntityManager, 
                                IDynamicEntity player)
        {
            _cameraManager = cameraManager;
            _soundEngine = soundEngine;
            _singleArray = singleArray;

            _dynamicEntityManager = dynamicEntityManager;
            _stepsTracker.Add(new KeyValuePair<IDynamicEntity, Vector3D>(player, player.Position));

            _dynamicEntityManager.EntityAdded += DynamicEntityManagerEntityAdded;
            _dynamicEntityManager.EntityRemoved += DynamicEntityManagerEntityRemoved;
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

        public override void BeforeDispose()
        {
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

                Range3I listenRange;

                listenRange.Position = _lastPosition - new Vector3I(16,16,16);

                listenRange.Size = new Vector3I(32,32,32); // 32768 block scan around player

                ListenCubes(listenRange);
                sw.Stop();
                _listenCubesTime = sw.ElapsedMilliseconds;
            }

            sw.Restart();

            PlayClosestSound();
            sw.Stop();
            _debugInfo += " cubes: " + _listenCubesTime + " ms, select closest: " + sw.ElapsedMilliseconds;

            #region check for steps sounds

            Random rnd = new Random();

            // foreach dynamic entity
            for (int i = 0; i < _stepsTracker.Count; i++)
            {
                var pair = _stepsTracker[i];
                IDynamicEntity entity = pair.Key;

                // first let's detect if the entity is in air

                Vector3D underTheFeets = entity.Position;
                underTheFeets.Y -= 0.01f;
                
                TerraCube cubeUnderFeet = _singleArray.GetCube(underTheFeets);

                // no need to play step if the entity is in air or not in walking displacement mode
                if (cubeUnderFeet.Id == CubeId.Error || 
                    cubeUnderFeet.Id == CubeId.Air || 
                    _stepsTracker[i].Key.DisplacementMode !=  Shared.Entities.EntityDisplacementModes.Walking)
                {
                    var item = new KeyValuePair<IDynamicEntity, Vector3D>(_stepsTracker[i].Key, entity.Position); //Save the position of the entity
                    _stepsTracker[i] = item;
                    continue;
                }

                // possible that entity just landed after the jump, so we need to check if the entity was in the air last time to play the landing sound
                Vector3D prevUnderTheFeets = pair.Value; //Containing the previous DynamicEntity Position
                prevUnderTheFeets.Y -= 0.01f;

                TerraCube prevCube = _singleArray.GetCube(prevUnderTheFeets);

                //Compute the distance between the previous and current position, set to
                double distance = Vector3D.Distance(pair.Value, entity.Position);

                // do we need to play the step sound?
                //Trigger only if the difference between previous memorize position and current is > 2.0 meters
                //Or if the previous position was in the air
                if (distance >= 1.5f || prevCube.Id == CubeId.Air)
                {
                    TerraCube currentCube = _singleArray.GetCube(entity.Position);
                        
                    //If walking on the ground, but with Feets and legs inside water block
                    if (currentCube.Id == CubeId.Water && cubeUnderFeet.Id != CubeId.Water)
                    {
                        //If my Head is not inside a Water block (Meaning = I've only the feet inside water)
                        TerraCube headCube = _singleArray.GetCube(entity.Position + new Vector3I(0, entity.Size.Y, 0));
                        if (headCube.Id == CubeId.Air)
                        {
                            List<string> sounds;
                            if (_stepsSounds.TryGetValue(currentCube.Id, out sounds))
                            {
                                _soundEngine.Play3D(sounds[0], (float)entity.Position.X, (float)entity.Position.Y, (float)entity.Position.Z);
                            }
                        }
                        else
                        {
                            if (headCube.Id == CubeId.Water)
                            {
                                //Play Sound ??
                                //Entity having its head, and feet inside water, but "walking" on the ground.
                            }
                        }
                    }
                    else
                    {
                        //Play a foot step sound only if the block under feet is solid to entity. (No water, no air, ...)
                        if (GameSystemSettings.Current.Settings.CubesProfile[cubeUnderFeet.Id].IsSolidToEntity)
                        {
                            List<string> sounds;
                            if (_stepsSounds.TryGetValue(cubeUnderFeet.Id, out sounds))
                            {
                                _soundEngine.Play3D(sounds[rnd.Next(0, sounds.Count)], (float)entity.Position.X, (float)entity.Position.Y, (float)entity.Position.Z);
                            }
                        }
                    }
                    
                    //Save the Entity last position.
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
        public void ListenCubes(Range3I range)
        {
            if (_singleArray == null) return;
            // remove sounds that are far away from the sound collection that are out of current player range
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
            

            int cubeIndex;
            // add new sounds
            foreach (var position in range.AllExclude(_lastRange))
            {
                //Get the block index, checking for World limit on the Y side
                if (_singleArray.IndexSafe(position.X, position.Y, position.Z, out cubeIndex))
                {
                    if (_singleArray.Cubes[cubeIndex].Id == CubeId.Water)
                    {
                        var soundPosition = new IrrVector3(position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f);

                        //Add the 3d position of a sound instance in the collection
                        if (_sharedSounds.ContainsKey("Sounds\\Ambiance\\water_stream.ogg"))
                            _sharedSounds["Sounds\\Ambiance\\water_stream.ogg"].Value.Add(soundPosition);
                        else
                        {
                            //Add new sound type collection
                            var sound = _soundEngine.Play3D("Sounds\\Ambiance\\water_stream.ogg", soundPosition, true, false, StreamMode.AutoDetect);
                            _sharedSounds.Add("Sounds\\Ambiance\\water_stream.ogg", new KeyValuePair<ISound, List<IrrVector3>>(sound, new List<IrrVector3> { soundPosition }));
                        }
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
