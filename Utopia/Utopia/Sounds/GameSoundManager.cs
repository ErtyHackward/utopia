using System;
using System.Collections.Generic;
using S33M3DXEngine.Debug.Interfaces;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3Resources.Structs;
using Utopia.Entities.Managers;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Interfaces;
using Vector3D = S33M3Resources.Structs.Vector3D;
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
using Utopia.Shared.World;
using Utopia.Shared.Sounds;
using System.IO;
using System.Linq;
using Utopia.Shared.Entities.Dynamic;

namespace Utopia.Sounds
{
    /// <summary>
    /// Wrapper around irrKlang library to provide sound playback
    /// Used to play all sound media
    /// </summary>
    public class GameSoundManager : GameComponent, IDebugInfo
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
            public int Priority;
        }

        #region Private Variables
        private readonly IClock _worlClock;
        private readonly CameraManager<ICameraFocused> _cameraManager;
        private IVisualDynamicEntityManager _dynamicEntityManager;
        private readonly ISoundEngine _soundEngine;
        private SingleArrayChunkContainer _singleArray;
        private IChunkEntityImpactManager _chunkEntityImpactManager;
        private IWorldChunks _worldChunk;
        private IClock _gameClockTime;
        private readonly PlayerEntityManager _playerEntityManager;
        private VisualWorldParameters _visualWorldParameters;
        private UtopiaProcessorParams _biomesParams;
        private Dictionary<IItem, ISoundVoice> _staticEntityPlayingVoices = new Dictionary<IItem, ISoundVoice>();

        private FastRandom _rnd;

        private Vector3 _listenerPosition;


        private readonly SortedList<string, KeyValuePair<ISoundVoice, List<Vector3>>> _sharedSounds = new SortedList<string, KeyValuePair<ISoundVoice, List<Vector3>>>();

        // collection of remembered positions of entities to detect the moment of playing next step sound
        private readonly List<DynamicEntitySoundTrack> _stepsTracker = new List<DynamicEntitySoundTrack>();
        // collection of sounds of steps
        private readonly Dictionary<byte, List<SoundMetaData>> _stepsSounds = new Dictionary<byte, List<SoundMetaData>>();

        private readonly List<SoundMetaData> _preLoad = new List<SoundMetaData>();
        #endregion

        #region Public Properties
        public Dictionary<MoodSoundKey, List<IUtopiaSoundSource>> MoodsSounds { get; set; }
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
                                IVisualDynamicEntityManager dynamicEntityManager,
                                IChunkEntityImpactManager chunkEntityImpactManager,
                                IWorldChunks worldChunk,
                                IClock gameClockTime,
                                PlayerEntityManager playerEntityManager,
                                VisualWorldParameters visualWorldParameters,
                                IClock worlClock)
        {
            _cameraManager = cameraManager;
            _soundEngine = soundEngine;
            _singleArray = singleArray;
            _worldChunk = worldChunk;
            _chunkEntityImpactManager = chunkEntityImpactManager;
            _gameClockTime = gameClockTime;
            _playerEntityManager = playerEntityManager;
            _visualWorldParameters = visualWorldParameters;
            _worlClock = worlClock;
            if (visualWorldParameters.WorldParameters.Configuration is UtopiaWorldConfiguration)
            {
                _biomesParams = ((UtopiaWorldConfiguration)visualWorldParameters.WorldParameters.Configuration).ProcessorParam;
            }

            _dynamicEntityManager = dynamicEntityManager;
            _stepsTracker.Add(new DynamicEntitySoundTrack { Entity = _playerEntityManager.Player, Position = _playerEntityManager.Player.Position, isLocalSound = true });
            _playerEntityManager.PlayerEntityChanged += _playerEntityManager_PlayerEntityChanged;

            //Register to Events
            
            _dynamicEntityManager.EntityAdded += DynamicEntityManagerEntityAdded;
            _dynamicEntityManager.EntityRemoved += DynamicEntityManagerEntityRemoved;
            _chunkEntityImpactManager.BlockReplaced += _chunkEntityImpactManager_BlockReplaced;
            _chunkEntityImpactManager.StaticEntityAdd += StaticEntityAdd;
            _chunkEntityImpactManager.StaticEntityRemoved += StaticEntityRemoved;

            _rnd = new FastRandom();
            MoodsSounds = new Dictionary<MoodSoundKey, List<IUtopiaSoundSource>>();

            IsDefferedLoadContent = true; //Make LoadContent executed in thread
        }

        void _playerEntityManager_PlayerEntityChanged(object sender, PlayerEntityChangedEventArgs e)
        {
            _stepsTracker.RemoveAll(t => t.Entity == e.PreviousCharacter);

            if (e.PlayerCharacter != null)
            {
                _stepsTracker.Add(new DynamicEntitySoundTrack { 
                    Entity = _playerEntityManager.Player, 
                    Position = _playerEntityManager.Player.Position, 
                    isLocalSound = true 
                });
            }
        }

        public override void BeforeDispose()
        {
            _dynamicEntityManager.EntityAdded -= DynamicEntityManagerEntityAdded;
            _dynamicEntityManager.EntityRemoved -= DynamicEntityManagerEntityRemoved;
            _chunkEntityImpactManager.BlockReplaced -= _chunkEntityImpactManager_BlockReplaced;
            _chunkEntityImpactManager.StaticEntityAdd -= StaticEntityAdd;
            _chunkEntityImpactManager.StaticEntityRemoved -= StaticEntityRemoved;
        }

        #region Public Methods
        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            #region Load Derived classes sounds
            foreach (var data in _preLoad)
            {
                ISoundDataSource dataSource = _soundEngine.AddSoundSourceFromFile(data.Path, data.Alias, SourceCategory.FX, priority:data.Priority);
                if (dataSource != null)
                {
                    dataSource.Volume = data.Volume;
                    dataSource.Power = data.Power;
                }
            }
            #endregion

            #region Load Steps sounds
            //Buffer cube walking sound
            foreach (var cube in _visualWorldParameters.WorldParameters.Configuration.GetAllCubesProfiles().Where(x => x.WalkingOverSound.Count > 0))
            {
                foreach (var walkingSound in cube.WalkingOverSound)
                {
                    RegisterStepSound(cube.Id, new SoundMetaData()
                    {
                        Path = walkingSound.FilePath,
                        Alias = walkingSound.Alias ?? Path.GetFileNameWithoutExtension(walkingSound.FilePath),
                        Volume = walkingSound.Volume,
                        Power = walkingSound.Power
                    }
                        );
                }
            }

            foreach (var pair in _stepsSounds)
            {
                foreach (var sound in pair.Value)
                {
                    ISoundDataSource dataSource = _soundEngine.AddSoundSourceFromFile(sound.Path, sound.Alias, SourceCategory.FX, priority:100);
                    if (dataSource != null)
                    {
                        dataSource.Volume = sound.Volume;
                        dataSource.Power = sound.Power;
                    }
                }
            }

            #endregion

            #region Load biome Ambiant sound

            //Prepare Sound for biomes ========================
            if (_biomesParams != null)
            {
                foreach (var biome in _biomesParams.Biomes)
                {
                    foreach (var biomeSound in biome.AmbientSound)
                    {
                        ISoundDataSource dataSource = _soundEngine.AddSoundSourceFromFile(biomeSound.FilePath, biomeSound.Alias, SourceCategory.Music, priority: biomeSound.Priority);
                        if (dataSource != null)
                        {
                            dataSource.Volume = biomeSound.Volume;
                            dataSource.isStreamed = true;
                        }
                    }
                }
            }

            #endregion

            #region Mood Sound
            //Load and prefetch Mood sounds ========================
            foreach (var moodSoundFile in Directory.GetFiles(@"Sounds\Moods", "*_*.wma"))
            {
                TimeOfDaySound time;
                MoodType type;
                string[] fileMetaData = moodSoundFile.Replace(".wma", "").Split('_');
                if (fileMetaData.Length < 3) time = TimeOfDaySound.FullDay;
                else
                {
                    switch (fileMetaData[2].ToLower())
                    {
                        case "day":
                            time = TimeOfDaySound.Day;
                            break;
                        case "night":
                            time = TimeOfDaySound.Night;
                            break;
                        default:
                            time = TimeOfDaySound.FullDay;
                            break;
                    }
                }

                switch (fileMetaData[1].ToLower())
                {
                    case "fear":
                        type = MoodType.Fear;
                        break;
                    case "peace":
                        type = MoodType.Peace;
                        break;
                    case "dead":
                        type = MoodType.Dead;
                        break;
                    default:
                        continue;
                }

                MoodsSoundSource soundSource = new MoodsSoundSource()
                {
                    Alias = "Mood" + fileMetaData[0],
                    FilePath = moodSoundFile,
                    Volume = type == MoodType.Peace ? 0.1f : 0.6f,
                    isStreamed = true                    
                };

                if (time == TimeOfDaySound.FullDay)
                {
                    InsertMoodSound(soundSource, TimeOfDaySound.Day, type);
                    InsertMoodSound(soundSource, TimeOfDaySound.Night, type);
                }
                else
                {
                    InsertMoodSound(soundSource, time, type);
                }

                ISoundDataSource dataSource = _soundEngine.AddSoundSourceFromFile(soundSource.FilePath, soundSource.Alias, SourceCategory.Music);
                if (dataSource != null)
                {
                    dataSource.Volume = soundSource.Volume;
                    dataSource.Power = soundSource.Power;
                }
            }
            #endregion

            base.LoadContent(context);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            _listenerPosition = _cameraManager.ActiveCamera.WorldPosition.Value.AsVector3();
            //Set current camera Position
            _soundEngine.SetListenerPosition(_listenerPosition, _cameraManager.ActiveCamera.LookAt.Value);
            //Update All sounds currently playing following new player position (For 3D Sounds)
            _soundEngine.Update3DSounds();

            //Get current player chunk
            VisualChunk chunk;
            if (!_worldChunk.GetSafeChunk(MathHelper.Floor(_playerEntityManager.Player.Position.X), MathHelper.Floor(_playerEntityManager.Player.Position.Z), out chunk)) 
                return;

            //Always active background music linked to player Mood + Time
            MoodSoundProcessing(_playerEntityManager.Player);

            //Activate Ambiant sounds following player positions, biomes, times, ...
            if (_biomesParams != null)
            {
                AmbiantSoundProcessing(chunk, _playerEntityManager.Player);
            }

            //Walking step sound processing
            WalkingSoundProcessing();

            try
            {
                StaticEntitiesEmittedSoundProcessing();
            }
            catch (InvalidOperationException x)
            {
                logger.Error("Error when processing static sounds: {0}", x.Message);
            }
        }

        public string GetDebugInfo()
        {
            return "Game Sound Manager Debug ...";
        }
        #endregion

        #region Private Methods

        private void InsertMoodSound(MoodsSoundSource sound, TimeOfDaySound timeofDay, MoodType type)
        {
            MoodSoundKey key = new MoodSoundKey() { TimeOfDay = timeofDay, Type = type };
            List<IUtopiaSoundSource> soundList;
            if (this.MoodsSounds.TryGetValue(key, out soundList) == false)
            {
                soundList = new List<IUtopiaSoundSource>();
                this.MoodsSounds.Add(key, soundList);
            }
            soundList.Add(sound);
        }

        /// <summary>
        /// Setup a step sound for a cube type
        /// </summary>
        /// <param name="cubeId"></param>
        /// <param name="sound"></param>
        private void RegisterStepSound(byte cubeId, SoundMetaData sound)
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

        protected void PreLoadSound(SoundMetaData metaData)
        {
            _preLoad.Add(metaData);
        }

        protected void PreLoadSound(string alias, string path, float volume, float power, int priority)
        {
            _preLoad.Add(new SoundMetaData() { Alias = alias, Path = path, Volume = volume, Power = power, Priority = priority });
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
                var result = _singleArray.GetCube(underTheFeets);
                if (result.IsValid == false) return;
                BlockProfile cubeUnderFeet = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[result.Cube.Id]; 

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

                //Compute the distance between the previous and current position, set to
                double distance = Vector3D.Distance(entityTrack.Position, entity.Position);

                // do we need to play the step sound?
                //Trigger only if the difference between previous memorize position and current is > 1.5 meters
                //Or if the previous position was in the air
                if (distance >= 1.5f || _singleArray.CheckCube(prevUnderTheFeets, WorldConfiguration.CubeId.Air))
                {
                    byte soundIndex = 0;
                    var cubeResult = _singleArray.GetCube(entity.Position);
                    if (cubeResult.IsValid) //Valid cube retrieved
                    {
                        BlockProfile currentCube = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[cubeResult.Cube.Id];

                        //If walking on the ground, but with Feets and legs inside water block
                        if (currentCube.CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid && cubeUnderFeet.IsSolidToEntity)
                        {
                            //If my Head is not inside a Water block (Meaning = I've only the feet inside water)
                            if (_singleArray.CheckCube(entity.Position + new Vector3D(0, entity.DefaultSize.Y, 0), WorldConfiguration.CubeId.Air))
                            {
                                soundIndex = PlayWalkingSound(currentCube.Id, entityTrack);
                            }
                        }
                        else
                        {
                            //Play a foot step sound only if the block under feet is solid to entity. (No water, no air, ...)
                            if (_visualWorldParameters.WorldParameters.Configuration.BlockProfiles[cubeUnderFeet.Id].IsSolidToEntity)
                            {
                                soundIndex = PlayWalkingSound(cubeUnderFeet.Id, entityTrack);
                            }
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
                    _soundEngine.StartPlay2D(sounds[soundIndex].Alias, SourceCategory.FX);
                }
                else
                {
                    _soundEngine.StartPlay3D(sounds[soundIndex].Alias, new Vector3((float)entityTrack.Entity.Position.X, (float)entityTrack.Entity.Position.Y, (float)entityTrack.Entity.Position.Z), SourceCategory.FX);
                }
            }

            return soundIndex;
        }
        #endregion

        #region Biome ambiant Sound Processing
        private ISoundVoice _currentlyPLayingAmbiantSound;
        private Biome _previousBiomePlaying;
        private void AmbiantSoundProcessing(VisualChunk chunk, ICharacterEntity player)
        {
            ChunkColumnInfo columnInfo = chunk.BlockData.GetColumnInfo(player.Position.ToCubePosition().X - chunk.ChunkPositionBlockUnit.X, player.Position.ToCubePosition().Z - chunk.ChunkPositionBlockUnit.Y);

            bool playerAboveMaxChunkheight = (columnInfo.MaxHeight - player.Position.ToCubePosition().Y < -15);
            bool playerBelowMaxChunkheight = (columnInfo.MaxHeight - player.Position.ToCubePosition().Y > 15);

            Biome currentBiome = _biomesParams.Biomes[chunk.BlockData.ChunkMetaData.ChunkMasterBiomeType];

            //Ambiant sound are just for surface "chunk", if not stop playing them !
            if (playerAboveMaxChunkheight || playerBelowMaxChunkheight || player.HealthState == DynamicEntityHealthState.Dead)
            {
                if (_currentlyPLayingAmbiantSound != null)
                {
                    if (_currentlyPLayingAmbiantSound.IsPlaying)
                    {
                        _currentlyPLayingAmbiantSound.Stop(1000);
                        _currentlyPLayingAmbiantSound = null;                        
                    }
                    _previousBiomePlaying = null;
                }
                return;
            }

            //IF first pass or biome did change or currently player sound is finished
            if (
                _previousBiomePlaying == null || 
                currentBiome != _previousBiomePlaying || 
                (_currentlyPLayingAmbiantSound != null && _currentlyPLayingAmbiantSound.IsPlaying == false)
                )
            {
                if (_currentlyPLayingAmbiantSound != null)
                {
                    if (_currentlyPLayingAmbiantSound.IsPlaying)
                    {
                        _currentlyPLayingAmbiantSound.Stop(1000);
                        _currentlyPLayingAmbiantSound = null;
                    }
                }
                //Pickup next biome ambiant sound, and start it !
                if (currentBiome.AmbientSound.Count > 0)
                {
                    int nextAmbientSoundId = _rnd.Next(0, currentBiome.AmbientSound.Count);
                    _currentlyPLayingAmbiantSound = _soundEngine.StartPlay2D(currentBiome.AmbientSound[nextAmbientSoundId].Alias, SourceCategory.Music, false, 3000);
                    _previousBiomePlaying = currentBiome;
                }
            }
        }
        #endregion

        #region Mood Sound Processing
        private ISoundVoice _currentlyPlayingMoodSound;
        private MoodSoundKey _previousMood = null;
        private void MoodSoundProcessing(ICharacterEntity player)
        {
            MoodSoundKey currentMood = new MoodSoundKey() { TimeOfDay = GetTimeofDay(), Type = GetMoodType(player) };

            //No sound was playing, or a new one needs to be played
            if (_currentlyPlayingMoodSound == null || 
                _currentlyPlayingMoodSound.IsPlaying == false ||
                _previousMood == null ||
                _previousMood != currentMood)
            {
                StartNewMoodSound(currentMood);
                _previousMood = currentMood;
            }        
        }

        private ISoundVoice StartNewMoodSound(MoodSoundKey currentMood)
        {
            if (_currentlyPlayingMoodSound != null)
            {
                if (_currentlyPlayingMoodSound.IsPlaying)
                {
                    _currentlyPlayingMoodSound.Stop(5000);
                    _currentlyPlayingMoodSound = null;
                }
            }

            //MoodsSounds.Keys[1].
            List<IUtopiaSoundSource> soundSource;

            if (MoodsSounds.TryGetValue(currentMood, out soundSource))
            {
                IUtopiaSoundSource sound = soundSource[_rnd.Next(0, soundSource.Count)];
                _currentlyPlayingMoodSound = SoundEngine.StartPlay2D(sound.Alias, SourceCategory.Music, false, 5000);
                return _currentlyPlayingMoodSound;
            }
            else
            {
                return null;
            }
        }

        private MoodType GetMoodType(ICharacterEntity player)
        {
            if (player.HealthState == DynamicEntityHealthState.Dead)
            {
                return MoodType.Dead;
            }

            //Add drowning sound !

            //Testing "Fear" Condition !
            bool playerNearBottom = player.Position.Y <= 30;

            if (playerNearBottom)
            {
                return MoodType.Fear;
            }
            else
            {
                //Test against conditions that could change my "Mood"
                return MoodType.Peace;
            }
        }

        private TimeOfDaySound GetTimeofDay()
        {
            if (_worlClock.ClockTime.Hours <= 6 || _worlClock.ClockTime.Hours >= 19) return TimeOfDaySound.Night;
            else return TimeOfDaySound.Day;
        }

        #endregion

        #region Sound on Events


        //Handle Cube removed / added sound
        private void _chunkEntityImpactManager_BlockReplaced(object sender, LandscapeBlockReplacedEventArgs e)
        {
            BlockProfile NewBlockTypeProfile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[e.NewBlockType];
            BlockProfile PreviousBlockTypeProfile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[e.PreviousBlock.Id];

            if (e.NewBlockType == WorldConfiguration.CubeId.Air && PreviousBlockTypeProfile.CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid)
                return;
            if (NewBlockTypeProfile.CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid && e.PreviousBlock.Id == WorldConfiguration.CubeId.Air)
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
            var index = _stepsTracker.FindIndex(p => p.Entity.DynamicId == e.Entity.DynamicId);
            _stepsTracker.RemoveAt(index);
        }

        private void DynamicEntityManagerEntityAdded(object sender, Shared.Entities.Events.DynamicEntityEventArgs e)
        {
            _stepsTracker.Add(new DynamicEntitySoundTrack { Entity = e.Entity, Position = e.Entity.Position, isLocalSound = false });
        }

        protected virtual void StaticEntityRemoved(object sender, StaticEventArgs e) { }
        protected virtual void StaticEntityAdd(object sender, StaticEventArgs e) { }

        protected virtual void PlayBlockPut(Vector3I blockPos){}
        protected virtual void PlayBlockTake(Vector3I blockPos){}
        #endregion

        #region StaticEntities Processing
        private const int _qtPlayingSound = 8;
        private const int _collectRange = 32;
        private void StaticEntitiesEmittedSoundProcessing()
        {
            IItem[] collectedStaticItems = new IItem[100];
            IItem[] nearestEntities = new IItem[_qtPlayingSound];
            IItem[] entitiesSet = new IItem[_qtPlayingSound];

            int i = 0;
            //Collection surrending "Sound" entities (Max of 100) ===========================================
            foreach (VisualChunk chunk in _worldChunk.SortedChunks.Where(x => x.DistanceFromPlayer < _collectRange))
            {
                foreach (var soundStaticEntities in chunk.SoundStaticEntities)
                {
                    collectedStaticItems[i] = soundStaticEntities;
                    i++;
                    if (i >= collectedStaticItems.Length) break;
                }
                if (i >= collectedStaticItems.Length) break;
            }
            for (; i < collectedStaticItems.Length; i++) { collectedStaticItems[i] = null; }

            // Sorting array by distance from Player
            i = 0;
            foreach (var entity in collectedStaticItems.Where(x => x != null).OrderBy(x => MVector3.DistanceSquared(x.Position, _playerEntityManager.CameraWorldPosition)))
            {
                nearestEntities[i] = entity;
                i++;
                if (i >= _qtPlayingSound) break; //Maximum playing 10 closest static sound at the same time
            }
            for (; i < _qtPlayingSound; i++) { nearestEntities[i] = null; }

            //Get entities remove from playing list.
            i = 0;
            entitiesSet[0] = null;
            foreach (var entities in _staticEntityPlayingVoices.Keys.Except(nearestEntities))
            {
                entitiesSet[i] = entities;
                i++;
            }
            for (i--; i >= 0; i--)
            {
                if (entitiesSet[i] == null) break;
                if (_staticEntityPlayingVoices[entitiesSet[i]] != null)
                {
                    _staticEntityPlayingVoices[entitiesSet[i]].Stop();
                }
                _staticEntityPlayingVoices.Remove(entitiesSet[i]);
            }

            ISoundVoice voice;
            foreach (var entities in nearestEntities.Where(x => x!=null))
            {
                if (_staticEntityPlayingVoices.TryGetValue(entities, out voice) == false)
                {
                    ISoundVoice playingVoice = _soundEngine.StartPlay3D(entities.EmittedSound, entities.Position.AsVector3(), entities.EmittedSound.isLooping, entities.EmittedSound.minDeferredStart, entities.EmittedSound.maxDeferredStart);
                    _staticEntityPlayingVoices.Add(entities, playingVoice);
                }
                else
                {
                    if (voice == null)
                    {
                        ISoundVoice playingVoice = _soundEngine.StartPlay3D(entities.EmittedSound, entities.Position.AsVector3(), entities.EmittedSound.isLooping, entities.EmittedSound.minDeferredStart, entities.EmittedSound.maxDeferredStart);
                        _staticEntityPlayingVoices[entities] = playingVoice;
                    }
                }
            }

        }
        #endregion

        #endregion
    }
}
