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
using Utopia.Shared.World;

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
        private VisualWorldParameters _visualWorldParameters;
        private UtopiaProcessorParams _biomesParams;

        private FastRandom _rnd;

        private Vector3 _listenerPosition;
        private IDynamicEntity _player;

        private readonly SortedList<string, KeyValuePair<ISoundVoice, List<Vector3>>> _sharedSounds = new SortedList<string, KeyValuePair<ISoundVoice, List<Vector3>>>();

        // collection of remembered positions of entities to detect the moment of playing next step sound
        private readonly List<DynamicEntitySoundTrack> _stepsTracker = new List<DynamicEntitySoundTrack>();
        // collection of sounds of steps
        private readonly Dictionary<byte, List<SoundMetaData>> _stepsSounds = new Dictionary<byte, List<SoundMetaData>>();

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
                                PlayerEntityManager playerEntityManager,
                                VisualWorldParameters visualWorldParameters)
        {
            _cameraManager = cameraManager;
            _soundEngine = soundEngine;
            _singleArray = singleArray;
            _worldChunk = worldChunk;
            _chunkEntityImpactManager = chunkEntityImpactManager;
            _gameClockTime = gameClockTime;
            _playerEntityManager = playerEntityManager;
            _visualWorldParameters = visualWorldParameters;
            if (visualWorldParameters.WorldParameters.Configuration is WorldConfiguration<UtopiaProcessorParams>)
            {
                _biomesParams = ((WorldConfiguration<UtopiaProcessorParams>)visualWorldParameters.WorldParameters.Configuration).ProcessorParam;
            }

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
            foreach (var pair in _stepsSounds)
            {
                foreach (var sound in pair.Value)
                {
                    ISoundDataSource dataSource = _soundEngine.AddSoundSourceFromFile(sound.Path, sound.Alias);
                    dataSource.SoundVolume = sound.Volume;
                    dataSource.SoundPower = sound.Power;
                }
            }

            foreach (var data in _preLoad)
            {
                ISoundDataSource dataSource = _soundEngine.AddSoundSourceFromFile(data.Path, data.Alias);
                dataSource.SoundVolume = data.Volume;
                dataSource.SoundPower = data.Power;
            }

            //Prepare Sound for biomes
            if (_biomesParams != null)
            {
                foreach (var biome in _biomesParams.Biomes)
                {
                    foreach (var biomeSound in biome.AmbientSound)
                    {
                        ISoundDataSource dataSource = _soundEngine.AddSoundSourceFromFile(biomeSound.SoundFilePath, biomeSound.SoundAlias);
                        dataSource.SoundVolume = biomeSound.DefaultVolume;
                    }
                }
            }


            base.LoadContent(context);
        }

        /// <summary>
        /// Setup a step sound for a cube type
        /// </summary>
        /// <param name="cubeId"></param>
        /// <param name="sound"></param>
        public void RegisterStepSound(byte cubeId, SoundMetaData sound)
        {
            if (_stepsSounds.ContainsKey(cubeId))
            {
                _stepsSounds[cubeId].Add(sound);
            }
            else
            {
                _stepsSounds.Add(cubeId, new List<SoundMetaData> { sound });
            }
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
            if (_biomesParams != null)
            {
                AmbiantSoundProcessing();
            }
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
            CubeProfile NewBlockTypeProfile = _visualWorldParameters.WorldParameters.Configuration.CubeProfiles[e.NewBlockType];
            CubeProfile PreviousBlockTypeProfile = _visualWorldParameters.WorldParameters.Configuration.CubeProfiles[e.PreviousBlock];

            if (e.NewBlockType == WorldConfiguration.CubeId.Air && PreviousBlockTypeProfile.CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid)
                return;
            if (NewBlockTypeProfile.CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid && e.PreviousBlock == WorldConfiguration.CubeId.Air)
                return;
            if (NewBlockTypeProfile.CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid && PreviousBlockTypeProfile.CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid)
                return;

            if (e.NewBlockType == WorldConfiguration.CubeId.Air)
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

                CubeProfile cubeUnderFeet = _visualWorldParameters.WorldParameters.Configuration.CubeProfiles[_singleArray.GetCube(underTheFeets).Id]; 

                // no need to play step if the entity is in air or not in walking displacement mode
                if (cubeUnderFeet.Id == WorldConfiguration.CubeId.Air ||
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
                if (distance >= 1.5f || prevCube.Id == WorldConfiguration.CubeId.Air)
                {
                    byte soundIndex = 0;
                    CubeProfile currentCube = _visualWorldParameters.WorldParameters.Configuration.CubeProfiles[_singleArray.GetCube(entity.Position).Id];

                    //If walking on the ground, but with Feets and legs inside water block
                    if (currentCube.CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid && cubeUnderFeet.IsSolidToEntity)
                    {
                        //If my Head is not inside a Water block (Meaning = I've only the feet inside water)
                        TerraCube headCube = _singleArray.GetCube(entity.Position + new Vector3I(0, entity.DefaultSize.Y, 0));
                        if (headCube.Id == WorldConfiguration.CubeId.Air)
                        {
                            soundIndex = PlayWalkingSound(currentCube.Id, entityTrack);
                        }
                    }
                    else
                    {
                        //Play a foot step sound only if the block under feet is solid to entity. (No water, no air, ...)
                        if (_visualWorldParameters.WorldParameters.Configuration.CubeProfiles[cubeUnderFeet.Id].IsSolidToEntity)
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
            List<SoundMetaData> sounds;
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
                    _soundEngine.StartPlay2D(sounds[soundIndex].Alias);
                }
                else
                {
                    _soundEngine.StartPlay3D(sounds[soundIndex].Alias, new Vector3((float)entityTrack.Entity.Position.X, (float)entityTrack.Entity.Position.Y, (float)entityTrack.Entity.Position.Z));
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
            Biome chunkBiome = _biomesParams.Biomes[chunk.BlockData.ChunkMetaData.ChunkMasterBiomeType];

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

        #endregion
    }
}
