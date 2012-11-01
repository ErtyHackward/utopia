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
using S33M3CoreComponents.Maths;
using Utopia.Worlds.Chunks;
using Utopia.Shared.World.Processors.Utopia.Biomes;
using Utopia.Worlds.GameClocks;
using Utopia.Entities.Managers;

namespace Utopia.Components
{
    /// <summary>
    /// Wrapper around irrKlang library to provide sound playback
    /// Used to play all sound media
    /// </summary>
    public class GameSoundManager : GameComponent, IDebugInfo
    {
        private struct DynamicEntitySoundTrack
        {
            public IDynamicEntity Entity;
            public Vector3D Position;
            public byte LastSound;
            public bool isLocalSound;
        }

        public struct SoundMetaData
        {
            public string Alias;
            public string Path;
            public float Volume;
            public float Power;
        }

        #region Private Variables
        private readonly CameraManager<ICameraFocused> _cameraManager;
        private IDynamicEntityManager _dynamicEntityManager;
        private readonly ISoundEngine _soundEngine;
        private SingleArrayChunkContainer _singleArray;
        private IChunkEntityImpactManager _chunkEntityImpactManager;
        private IWorldChunks _worldChunk;
        private IClock _gameClockTime;
        private PlayerEntityManager _playerEntityManager;

        private FastRandom _rnd;

        private Vector3I _lastPosition;
        private Range3I _lastRange;
        private Vector3 _listenerPosition;
        private IDynamicEntity _player;

        private readonly SortedList<string, KeyValuePair<ISoundVoice, List<Vector3>>> _sharedSounds = new SortedList<string, KeyValuePair<ISoundVoice, List<Vector3>>>();

        // collection of remembered positions of entities to detect the moment of playing next step sound
        private readonly List<DynamicEntitySoundTrack> _stepsTracker = new List<DynamicEntitySoundTrack>();
        // collection of sounds of steps
        private readonly Dictionary<byte, List<string>> _stepsSounds = new Dictionary<byte, List<string>>();

        private readonly List<KeyValuePair<int, string>> _ambientSounds = new List<KeyValuePair<int, string>>();

        private readonly List<SoundMetaData> _preLoad = new List<SoundMetaData>();
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets irrKlang sound engine object
        /// </summary>
        public ISoundEngine SoundEngine
        {
            get { return _soundEngine; }
        }

        public bool ShowDebugInfo { get; set; }
        #endregion

        public GameSoundManager(ISoundEngine soundEngine,
                                CameraManager<ICameraFocused> cameraManager,
                                SingleArrayChunkContainer singleArray,
                                IDynamicEntityManager dynamicEntityManager,
                                IDynamicEntity player,
                                IChunkEntityImpactManager chunkEntityImpactManager,
                                IWorldChunks worldChunk,
                                IClock gameClockTime,
                                PlayerEntityManager playerEntityManager)
        {
            _cameraManager = cameraManager;
            _soundEngine = soundEngine;
            _singleArray = singleArray;
            _worldChunk = worldChunk;
            _chunkEntityImpactManager = chunkEntityImpactManager;
            _gameClockTime = gameClockTime;
            _playerEntityManager = playerEntityManager;

            _dynamicEntityManager = dynamicEntityManager;
            _stepsTracker.Add(new DynamicEntitySoundTrack { Entity = player, Position = player.Position, isLocalSound = true });
            _player = player;

            //Register to Events
            _playerEntityManager.OnLanding += playerEntityManager_OnLanding;

            worldChunk.LoadComplete += worldChunk_LoadComplete;

            _dynamicEntityManager.EntityAdded += DynamicEntityManagerEntityAdded;
            _dynamicEntityManager.EntityRemoved += DynamicEntityManagerEntityRemoved;
            _chunkEntityImpactManager.BlockReplaced += _chunkEntityImpactManager_BlockReplaced;

            _rnd = new FastRandom();

            this.IsDefferedLoadContent = true; //Make LoadContent executed in thread
        }

        public override void BeforeDispose()
        {
            _playerEntityManager.OnLanding -= playerEntityManager_OnLanding;
            _worldChunk.LoadComplete -= worldChunk_LoadComplete;
            _dynamicEntityManager.EntityAdded -= DynamicEntityManagerEntityAdded;
            _dynamicEntityManager.EntityRemoved -= DynamicEntityManagerEntityRemoved;
            _chunkEntityImpactManager.BlockReplaced -= _chunkEntityImpactManager_BlockReplaced;
        }

        #region Public Methods
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

            foreach (var data in _preLoad)
            {
                ISoundDataSource dataSource = _soundEngine.AddSoundSourceFromFile(data.Path, data.Alias);
                dataSource.SoundVolume = data.Volume;
                dataSource.SoundPower = data.Power;
            }

            //Prepare Sound for biomes
            foreach (var biome in Utopia.Shared.Configuration.RealmConfiguration.Biomes)
            {
                foreach (var biomeSound in biome.AmbientSound)
                {
                    ISoundDataSource dataSource = _soundEngine.AddSoundSourceFromFile(biomeSound.SoundFilePath, biomeSound.SoundAlias);
                    dataSource.SoundVolume = biomeSound.DefaultVolume;
                }
            }


            base.LoadContent(context);
        }

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

        public void PreLoadSound(SoundMetaData metaData)
        {
            _preLoad.Add(metaData);
        }

        public void PreLoadSound(string alias, string path, float volume, float power)
        {
            _preLoad.Add(new SoundMetaData() { Alias = alias, Path = path, Volume = volume, Power = power });
        }

        public override void Update(GameTime timeSpent)
        {
            _listenerPosition = _cameraManager.ActiveCamera.WorldPosition.Value.AsVector3();
            //Set current camera Position
            _soundEngine.SetListenerPosition(_listenerPosition, _cameraManager.ActiveCamera.LookAt.Value);
            //Update All sounds currently playing following new player position (For 3D Sounds)
            _soundEngine.Update3DSounds();
            AmbiantSoundProcessing();
            WalkingSoundProcessing();
        }

        public string GetDebugInfo()
        {
            return "Game Sound Manager Debug ...";
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
            _stepsTracker.Add(new DynamicEntitySoundTrack { Entity = e.Entity, Position = e.Entity.Position, isLocalSound = false });
        }

        protected virtual void PlayBlockPut(Vector3I blockPos)
        {
        }
        protected virtual void PlayBlockTake(Vector3I blockPos)
        {
        }

        #region Walking Sound Processing
        private void WalkingSoundProcessing()
        {
            // foreach dynamic entity
            for (int i = 0; i < _stepsTracker.Count; i++)
            {
                DynamicEntitySoundTrack entityTrack = _stepsTracker[i];
                IDynamicEntity entity = entityTrack.Entity;

                // first let's detect if the entity is in air

                Vector3D underTheFeets = entity.Position;
                underTheFeets.Y -= 0.01f;

                TerraCube cubeUnderFeet = _singleArray.GetCube(underTheFeets);

                // no need to play step if the entity is in air or not in walking displacement mode
                if (cubeUnderFeet.Id == RealmConfiguration.CubeId.Air ||
                    _stepsTracker[i].Entity.DisplacementMode != Shared.Entities.EntityDisplacementModes.Walking)
                {
                    var item = new DynamicEntitySoundTrack { Entity = _stepsTracker[i].Entity, Position = entity.Position, isLocalSound = _stepsTracker[i].isLocalSound }; //Save the position of the entity
                    _stepsTracker[i] = item;
                    continue;
                }

                // possible that entity just landed after the jump, so we need to check if the entity was in the air last time to play the landing sound
                Vector3D prevUnderTheFeets = entityTrack.Position; //Containing the previous DynamicEntity Position
                prevUnderTheFeets.Y -= 0.01f;

                TerraCube prevCube = _singleArray.GetCube(prevUnderTheFeets);

                //Compute the distance between the previous and current position, set to
                double distance = Vector3D.Distance(entityTrack.Position, entity.Position);

                // do we need to play the step sound?
                //Trigger only if the difference between previous memorize position and current is > 1.5 meters
                //Or if the previous position was in the air
                if (distance >= 1.5f || prevCube.Id == RealmConfiguration.CubeId.Air)
                {
                    byte soundIndex = 0;
                    TerraCube currentCube = _singleArray.GetCube(entity.Position);

                    //If walking on the ground, but with Feets and legs inside water block
                    if (currentCube.Id == RealmConfiguration.CubeId.StillWater && cubeUnderFeet.Id != RealmConfiguration.CubeId.StillWater)
                    {
                        //If my Head is not inside a Water block (Meaning = I've only the feet inside water)
                        TerraCube headCube = _singleArray.GetCube(entity.Position + new Vector3I(0, entity.DefaultSize.Y, 0));
                        if (headCube.Id == RealmConfiguration.CubeId.Air)
                        {
                            soundIndex = PlayWalkingSound(currentCube.Id, entityTrack);
                        }
                    }
                    else
                    {
                        //Play a foot step sound only if the block under feet is solid to entity. (No water, no air, ...)
                        if (RealmConfiguration.CubeProfiles[cubeUnderFeet.Id].IsSolidToEntity)
                        {
                            soundIndex = PlayWalkingSound(cubeUnderFeet.Id, entityTrack);
                        }
                    }

                    //Save the Entity last position.
                    var item = new DynamicEntitySoundTrack { Entity = _stepsTracker[i].Entity, Position = entity.Position, LastSound = soundIndex, isLocalSound = _stepsTracker[i].isLocalSound };
                    _stepsTracker[i] = item;
                }
            }
        }
        private byte PlayWalkingSound(byte cubeId, DynamicEntitySoundTrack entityTrack)
        {
            List<string> sounds;
            byte soundIndex = 0;
            // play a water sound
            if (_stepsSounds.TryGetValue(cubeId, out sounds))
            {
                // choose another sound to avoid playing the same sound one after another
                while (sounds.Count > 1 && entityTrack.LastSound == soundIndex)
                {
                    soundIndex = (byte)_rnd.Next(0, sounds.Count);
                }

                if (entityTrack.isLocalSound)
                {
                    _soundEngine.StartPlay2D(sounds[soundIndex]);
                }
                else
                {
                    _soundEngine.StartPlay3D(sounds[soundIndex], new Vector3((float)entityTrack.Entity.Position.X, (float)entityTrack.Entity.Position.Y, (float)entityTrack.Entity.Position.Z));
                }
            }

            return soundIndex;
        }
        #endregion

        #region Ambiant Sound Processing
        private Vector3I _playerWorldCubePosition;
        private ISoundVoice _currentlyPLayingAmbiantSound;
        private void AmbiantSoundProcessing()
        {
            //Do nothing if player did not move ! => Check how to do it ...

            //Get Player Cube Position
            Vector3I newWorldCubePosition = (Vector3I)_player.Position;
            if (newWorldCubePosition == _playerWorldCubePosition) return; //Player did not move

            //Get current player chunk
            VisualChunk chunk = _worldChunk.GetChunk(ref newWorldCubePosition);

            //Get biome info from the "chunk" MasterBiome.
            //Masterbiome being the biome associated to the chunk, based on average of all column's chunk biome.
            Biome chunkBiome = Utopia.Shared.Configuration.RealmConfiguration.Biomes[chunk.BlockData.ChunkMetaData.ChunkMasterBiomeType];

            ChunkColumnInfo columnInfo = chunk.BlockData.GetColumnInfo(newWorldCubePosition.X - chunk.ChunkPositionBlockUnit.X, newWorldCubePosition.Z - chunk.ChunkPositionBlockUnit.Y);

            //Player position VS Chunk Max Height.
            int maxchunkheight = chunk.BlockData.ChunkMetaData.ChunkMaxHeightBuilt;

            bool playerAboveMaxChunkheight = (columnInfo.MaxGroundHeight - newWorldCubePosition.Y < -15);
            bool playerBelowMaxChunkheight = (columnInfo.MaxGroundHeight - newWorldCubePosition.Y > 15);
            bool playerNearBottom = newWorldCubePosition.Y <= 30;

            //The biome doesn't have associated sound with it, or below Up/down thresholds
            if (chunkBiome.AmbientSound.Count == 0 || playerAboveMaxChunkheight || playerBelowMaxChunkheight || playerNearBottom)
            {
                //Stop ambiant sound if still currently playing
                if (_currentlyPLayingAmbiantSound != null)
                {
                    if (playerNearBottom && _currentlyPLayingAmbiantSound.PlayingDataSource.SoundAlias == "Cavern") return;

                    _currentlyPLayingAmbiantSound.Stop(1000);
                    _currentlyPLayingAmbiantSound = null;

                    if (playerNearBottom)
                    {
                        //Play special "UnderGround" ambiant sound here.
                        // ==> I'm "below Surface cube"
                        _currentlyPLayingAmbiantSound = _soundEngine.StartPlay2D("Cavern", true, 3000);
                    }
                }
            }
            else
            {
                //Select randomly a chunk ambiant sound to play.
                int nextAmbientSoundId = _rnd.Next(0, chunkBiome.AmbientSound.Count);

                if (_currentlyPLayingAmbiantSound == null || chunkBiome.AmbientSound[nextAmbientSoundId].SoundAlias != _currentlyPLayingAmbiantSound.PlayingDataSource.SoundAlias)
                {
                    if (_currentlyPLayingAmbiantSound != null)
                    {
                        _currentlyPLayingAmbiantSound.Stop(1000);
                        _currentlyPLayingAmbiantSound = null;
                    }
                    _currentlyPLayingAmbiantSound = _soundEngine.StartPlay2D(chunkBiome.AmbientSound[nextAmbientSoundId].SoundAlias, true, 3000);
                }
            }

            _playerWorldCubePosition = newWorldCubePosition;

        }
        #endregion

        #region Sound on Events
        void playerEntityManager_OnLanding(double fallHeight, TerraCubeWithPosition landedCube)
        {
            if (fallHeight > 3 && fallHeight <= 10)
            {
                SoundEngine.StartPlay2D("Hurt", 0.3f);
            }
            else
            {
                if (fallHeight > 10)
                {
                    SoundEngine.StartPlay2D("Hurt", 1.0f);
                }
            }
        }

        void worldChunk_LoadComplete(object sender, EventArgs e)
        {
            //Start playing main "Moods" music
            SoundEngine.StartPlay2D("Peaceful", true, 5000);
        }
        #endregion

        #region OLD and Slow Ambiant Sound Processing
        private void AmbiantSoundProcessingOLD()
        {
            // update all cubes sounds if Camera did move a little !
            if ((Vector3I)_cameraManager.ActiveCamera.WorldPosition.Value != _lastPosition)
            {
                _lastPosition = (Vector3I)_cameraManager.ActiveCamera.WorldPosition.Value;

                Range3I listenRange;

                listenRange.Position = _lastPosition - new Vector3I(16, 16, 16);

                listenRange.Size = new Vector3I(32, 32, 32); // 32768 block scan around player

                ListenCubes(listenRange);
            }

            PlayClosestSound();
        }

        /// <summary>
        /// Update cubes that emit sounds
        /// </summary>
        /// <param name="range"></param>
        private void ListenCubes(Range3I range)
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
        private void PlayClosestSound()
        {
            foreach (var pair in _sharedSounds)
            {
                // only one position available
                if (pair.Value.Value.Count == 1) continue;

                // choose the closest
                var distance = Vector3.Distance(_listenerPosition, pair.Value.Value[0]);
                var position = pair.Value.Value[0];

                for (int i = 1; i < pair.Value.Value.Count; i++)
                {
                    var d = Vector3.Distance(_listenerPosition, pair.Value.Value[i]);
                    if (d < distance)
                    {
                        position = pair.Value.Value[i];
                        distance = d;
                    }
                }

                pair.Value.Key.Position = position;
            }
        }
        #endregion

        #endregion
    }
}
