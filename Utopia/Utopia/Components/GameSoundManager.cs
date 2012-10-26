using System;
using System.Collections.Generic;
using System.Diagnostics;
using S33M3DXEngine.Debug.Interfaces;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using Vector3D = S33M3Resources.Structs.Vector3D;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Settings;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Shared.Configuration;
using S33M3CoreComponents.Sound;
using SharpDX;

namespace Utopia.Components
{
    /// <summary>
    /// Wrapper around irrKlang library to provide sound playback
    /// Used to play all sound media
    /// </summary>
    public class GameSoundManager : GameComponent, IDebugInfo
    {
        private struct Track
        {
            public IDynamicEntity Entity;
            public Vector3D Position;
            public byte LastSound;
            public bool isLocalSound;
        }

        #region Private Variables
        private readonly CameraManager<ICameraFocused> _cameraManager;
        private IDynamicEntityManager _dynamicEntityManager;
        private readonly ISoundEngine _soundEngine;
        private SingleArrayChunkContainer _singleArray;
        private IChunkEntityImpactManager _chunkEntityImpactManager;

        private Vector3I _lastPosition;
        private Range3I _lastRange;
        private Vector3 _listenerPosition;

        private readonly SortedList<string, KeyValuePair<ISoundVoice, List<Vector3>>> _sharedSounds = new SortedList<string, KeyValuePair<ISoundVoice, List<Vector3>>>();

        private string _debugInfo;
        private long _listenCubesTime;

        // collection of remembered positions of entities to detect the moment of playing next step sound
        private readonly List<Track> _stepsTracker = new List<Track>();
        // collection of sounds of steps
        private readonly Dictionary<byte, List<string>> _stepsSounds = new Dictionary<byte, List<string>>();

        private readonly List<KeyValuePair<int, string>> _ambientSounds = new List<KeyValuePair<int, string>>();

        private readonly List<string> _preLoad = new List<string>();
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets irrKlang sound engine object
        /// </summary>
        public ISoundEngine SoundEngine
        {
            get { return _soundEngine; }
        }
        #endregion

        public GameSoundManager(ISoundEngine soundEngine,
                                CameraManager<ICameraFocused> cameraManager,
                                SingleArrayChunkContainer singleArray,
                                IDynamicEntityManager dynamicEntityManager,
                                IDynamicEntity player,
                                IChunkEntityImpactManager chunkEntityImpactManager)
        {
            _cameraManager = cameraManager;
            _soundEngine = soundEngine;
            _singleArray = singleArray;
            _chunkEntityImpactManager = chunkEntityImpactManager;

            _dynamicEntityManager = dynamicEntityManager;
            _stepsTracker.Add(new Track { Entity = player, Position = player.Position, isLocalSound = true });

            _dynamicEntityManager.EntityAdded += DynamicEntityManagerEntityAdded;
            _dynamicEntityManager.EntityRemoved += DynamicEntityManagerEntityRemoved;
            _chunkEntityImpactManager.BlockReplaced += _chunkEntityImpactManager_BlockReplaced;
        }

        public override void BeforeDispose()
        {
            _dynamicEntityManager.EntityAdded -= DynamicEntityManagerEntityAdded;
            _dynamicEntityManager.EntityRemoved -= DynamicEntityManagerEntityRemoved;
            _chunkEntityImpactManager.BlockReplaced -= _chunkEntityImpactManager_BlockReplaced;
        }

        #region Public Methods
        /// <summary>
        /// Setup a step sound for a cube type
        /// </summary>
        /// <param name="cubeId"></param>
        /// <param name="sound"></param>
        public void RegisterStepSound(byte cubeId, string sound)
        {
            if (_stepsSounds.ContainsKey(cubeId))
            {
                _stepsSounds[cubeId].Add(sound);
            }
            else
            {
                _stepsSounds.Add(cubeId, new List<string> { sound });
            }
        }

        /// <summary>
        /// Setup an ambient sound for a cube type. Dont add many sounds, it hurts the perfomance.
        /// </summary>
        /// <param name="cubeId"></param>
        /// <param name="sound"></param>
        public void RegisterCubeAmbientSound(byte cubeId, string sound)
        {
            var index = _ambientSounds.FindIndex(p => p.Key == cubeId);
            if (index != -1)
                throw new InvalidOperationException("Only one ambient sound is allowed per cube type");

            _ambientSounds.Add(new KeyValuePair<int, string>(cubeId, sound));
        }

        public void PreLoadSound(string path)
        {
            _preLoad.Add(path);
        }

        //? ?????? Async load possible ????
        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            // load all sounds
            foreach (var pair in _ambientSounds)
            {
                ISoundDataSource dataSource = _soundEngine.AddSoundSourceFromFile(pair.Value, pair.Value);
                dataSource.SoundVolume = 0.5f;
                dataSource.SoundPower = 32.0f;
            }

            foreach (var pair in _stepsSounds)
            {
                foreach (var path in pair.Value)
                {
                    ISoundDataSource dataSource = _soundEngine.AddSoundSourceFromFile(path, path);
                    dataSource.SoundVolume = 0.1f;
                }
            }

             foreach (var path in _preLoad)
            {
                ISoundDataSource dataSource = _soundEngine.AddSoundSourceFromFile(path, path);
                dataSource.SoundVolume = 0.3f;
                dataSource.SoundPower = 12.0f;
            }

            base.LoadContent(context);
        }

        public override void Update(GameTime timeSpent)
        {
            _listenerPosition = _cameraManager.ActiveCamera.WorldPosition.Value.AsVector3();

            var sw = Stopwatch.StartNew();

            _soundEngine.SetListenerPosition(_listenerPosition, _cameraManager.ActiveCamera.LookAt.Value);
            _soundEngine.Update3DSounds();
            sw.Stop();

            _debugInfo = "Sounds playing: " + _sharedSounds.Count + ", Update " + sw.ElapsedMilliseconds + " ms, ";

            // update all cubes sounds if Camera move !
            if ((Vector3I)_cameraManager.ActiveCamera.WorldPosition.Value != _lastPosition)
            {
                sw.Restart();
                _lastPosition = (Vector3I)_cameraManager.ActiveCamera.WorldPosition.Value;

                Range3I listenRange;

                listenRange.Position = _lastPosition - new Vector3I(16, 16, 16);

                listenRange.Size = new Vector3I(32, 32, 32); // 32768 block scan around player

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
                IDynamicEntity entity = pair.Entity;

                // first let's detect if the entity is in air

                Vector3D underTheFeets = entity.Position;
                underTheFeets.Y -= 0.01f;

                TerraCube cubeUnderFeet = _singleArray.GetCube(underTheFeets);

                // no need to play step if the entity is in air or not in walking displacement mode
                if (cubeUnderFeet.Id == RealmConfiguration.CubeId.Error ||
                    cubeUnderFeet.Id == RealmConfiguration.CubeId.Air ||
                    _stepsTracker[i].Entity.DisplacementMode != Shared.Entities.EntityDisplacementModes.Walking)
                {
                    var item = new Track { Entity = _stepsTracker[i].Entity, Position = entity.Position, isLocalSound = _stepsTracker[i].isLocalSound }; //Save the position of the entity
                    _stepsTracker[i] = item;
                    continue;
                }

                // possible that entity just landed after the jump, so we need to check if the entity was in the air last time to play the landing sound
                Vector3D prevUnderTheFeets = pair.Position; //Containing the previous DynamicEntity Position
                prevUnderTheFeets.Y -= 0.01f;

                TerraCube prevCube = _singleArray.GetCube(prevUnderTheFeets);

                //Compute the distance between the previous and current position, set to
                double distance = Vector3D.Distance(pair.Position, entity.Position);

                // do we need to play the step sound?
                //Trigger only if the difference between previous memorize position and current is > 2.0 meters
                //Or if the previous position was in the air
                if (distance >= 1.5f || prevCube.Id == RealmConfiguration.CubeId.Air)
                {
                    TerraCube currentCube = _singleArray.GetCube(entity.Position);

                    var soundIndex = pair.LastSound;

                    //If walking on the ground, but with Feets and legs inside water block
                    if (currentCube.Id == RealmConfiguration.CubeId.StillWater && cubeUnderFeet.Id != RealmConfiguration.CubeId.StillWater)
                    {
                        //If my Head is not inside a Water block (Meaning = I've only the feet inside water)
                        TerraCube headCube = _singleArray.GetCube(entity.Position + new Vector3I(0, entity.DefaultSize.Y, 0));
                        if (headCube.Id == RealmConfiguration.CubeId.Air)
                        {
                            List<string> sounds;
                            // play a water sound
                            if (_stepsSounds.TryGetValue(currentCube.Id, out sounds))
                            {
                                if (_stepsTracker[i].isLocalSound)
                                {
                                    _soundEngine.StartPlay2D(sounds[0]);
                                }
                                else
                                {
                                    //_soundEngine.Play3D(sounds[0], (float)entity.Position.X, (float)entity.Position.Y, (float)entity.Position.Z);
                                }
                            }
                        }
                        else
                        {
                            if (headCube.Id == RealmConfiguration.CubeId.StillWater)
                            {
                                //Play Sound ??
                                //Entity having its head, and feet inside water, but "walking" on the ground.
                            }
                        }
                    }
                    else
                    {
                        //Play a foot step sound only if the block under feet is solid to entity. (No water, no air, ...)
                        if (RealmConfiguration.CubeProfiles[cubeUnderFeet.Id].IsSolidToEntity)
                        {
                            List<string> sounds;
                            if (_stepsSounds.TryGetValue(cubeUnderFeet.Id, out sounds))
                            {
                                var prevSound = soundIndex;
                                soundIndex = 0;
                                // choose another sound to avoid playing the same sound one after another
                                while (sounds.Count > 1 && prevSound == soundIndex)
                                    soundIndex = (byte)rnd.Next(0, sounds.Count);

                                if (_stepsTracker[i].isLocalSound)
                                {
                                    _soundEngine.StartPlay2D(sounds[soundIndex]);
                                }
                                else
                                {
                                    //_soundEngine.Play3D(sounds[soundIndex], (float)entity.Position.X, (float)entity.Position.Y, (float)entity.Position.Z);
                                }

                            }
                        }
                    }

                    //Save the Entity last position.
                    var item = new Track { Entity = _stepsTracker[i].Entity, Position = entity.Position, LastSound = soundIndex, isLocalSound = _stepsTracker[i].isLocalSound };
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
            if (_singleArray == null || _ambientSounds.Count == 0) return;

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
                // Get the block index, checking for World limit on the Y side
                if (_singleArray.IndexSafe(position.X, position.Y, position.Z, out cubeIndex))
                {
                    var index = _ambientSounds.FindIndex(p => p.Key == _singleArray.Cubes[cubeIndex].Id);
                    if (index != -1)
                    {
                        var soundPath = _ambientSounds[index].Value;
                        // put the ambient sound right in the center of a cube
                        var soundPosition = new Vector3(position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f);

                        // Add the 3d position of a sound instance in the collection
                        if (_sharedSounds.ContainsKey(soundPath))
                            _sharedSounds[soundPath].Value.Add(soundPosition);
                        else
                        {
                            // Add new sound type collection
                            var sound = _soundEngine.StartPlay3D(soundPath, soundPosition, true);
                            //var sound = _soundEngine.StartPlay2D(soundPath, true);
                            _sharedSounds.Add(soundPath, new KeyValuePair<ISoundVoice, List<Vector3>>(sound, new List<Vector3> { soundPosition }));
                        }
                    }
                }
            }

            _lastRange = range;
        }

        /// <summary>
        /// Select the closest ambient sound position
        /// </summary>
        public void PlayClosestSound()
        {
            foreach (var pair in _sharedSounds)
            {
                // only one position available
                if (pair.Value.Value.Count == 1) continue;

                // choose the closest
                var distance = Vector3.Distance(_listenerPosition,pair.Value.Value[0]);
                var position = pair.Value.Value[0];

                for (int i = 1; i < pair.Value.Value.Count; i++)
                {
                    var d = Vector3.Distance(_listenerPosition,pair.Value.Value[i]);
                    if (d < distance)
                    {
                        position = pair.Value.Value[i];
                        distance = d;
                    }
                }

                pair.Value.Key.Position = position;
            }
        }


        public virtual void PlayBlockPut(Vector3I blockPos)
        {
        }

        public virtual void PlayBlockTake(Vector3I blockPos)
        {
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
        #endregion

        #region Private Methods
        //Handle Cube removed / added sound
        private void _chunkEntityImpactManager_BlockReplaced(object sender, LandscapeBlockReplacedEventArgs e)
        {
            if (e.NewBlockType == RealmConfiguration.CubeId.Air && e.PreviousBlock == RealmConfiguration.CubeId.DynamicWater)
                return;
            if (e.NewBlockType == RealmConfiguration.CubeId.DynamicWater && e.PreviousBlock == RealmConfiguration.CubeId.Air)
                return;
            if (e.NewBlockType == RealmConfiguration.CubeId.DynamicWater && e.PreviousBlock == RealmConfiguration.CubeId.DynamicWater)
                return;

            if (e.NewBlockType == RealmConfiguration.CubeId.Air)
                PlayBlockTake(e.Position);
            else
                PlayBlockPut(e.Position);
        }

        private void DynamicEntityManagerEntityRemoved(object sender, Shared.Entities.Events.DynamicEntityEventArgs e)
        {
            _stepsTracker.RemoveAt(_stepsTracker.FindIndex(p => p.Entity == e.Entity));
        }

        private void DynamicEntityManagerEntityAdded(object sender, Shared.Entities.Events.DynamicEntityEventArgs e)
        {
            _stepsTracker.Add(new Track { Entity = e.Entity, Position = e.Entity.Position, isLocalSound = false });
        }
        #endregion
    }
}
