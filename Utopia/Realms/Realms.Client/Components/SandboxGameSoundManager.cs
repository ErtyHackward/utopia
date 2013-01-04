using S33M3Resources.Structs;
using Utopia.Components;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Shared.Configuration;
using S33M3CoreComponents.Sound;
using SharpDX;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.GameClocks;
using Utopia.Entities.Managers;
using Utopia.Shared.World;
using System.Linq;
using Utopia.Sounds;
using System.IO;
using Utopia.Shared.Sounds;
using System.Collections.Generic;

namespace Realms.Client.Components
{
    public class SandboxGameSoundManager : GameSoundManager
    {
        public SandboxGameSoundManager( ISoundEngine soundEngine,
                                    CameraManager<ICameraFocused> cameraManager,
                                    SingleArrayChunkContainer singleArray,
                                    IDynamicEntityManager dynamicEntityManager,
                                    IDynamicEntity player,
                                    IChunkEntityImpactManager chunkEntityImpactManager,
                                    IWorldChunks worldChunk,
                                    IClock gameClockTime,
                                    PlayerEntityManager playerEntityManager,
                                    VisualWorldParameters visualWorldParameters,
                                    IClock worlClock)
            : base(soundEngine, cameraManager, singleArray, dynamicEntityManager, player, chunkEntityImpactManager, worldChunk, gameClockTime, playerEntityManager, visualWorldParameters, worlClock)
        {

            //Buffer cube walking sound
            foreach (var cube in visualWorldParameters.WorldParameters.Configuration.GetAllCubesProfiles().Where(x => x.WalkingOverSound.Count > 0))
            {
                foreach (var walkingSound in cube.WalkingOverSound)
                {
                    RegisterStepSound(cube.Id, new SoundMetaData()
                                {
                                    Path = walkingSound.SoundFilePath,
                                    Alias = walkingSound.SoundAlias,
                                    Volume = walkingSound.DefaultVolume,
                                    Power = walkingSound.Power
                                }
                        );
                }
            }


            PreLoadSound("Put", @"Sounds\Blocks\put.wav", 0.3f, 12.0f);
            PreLoadSound("Take", @"Sounds\Blocks\take.wav", 0.3f, 12.0f);
            PreLoadSound("Hurt", @"Sounds\Events\hurt.wav", 0.3f, 16.0f);
            PreLoadSound("WaterDrop", @"Sounds\Events\waterdrop.wav", 1.0f, 16.0f);

            //Load and prefetch Mood sounds
            foreach (var moodSoundFile in Directory.GetFiles(@"Sounds\Moods", "*.adpcm.wav"))
            {
                TimeOfDaySound time;
                MoodType type;
                string[] fileMetaData = moodSoundFile.Replace(".adpcm.wav", "").Split('_');
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
                    default:
                        continue;
                }

                MoodsSoundSource soundSource = new MoodsSoundSource()
                {
                    SoundAlias = "Mood" + fileMetaData[0],
                    SoundFilePath = moodSoundFile,
                    DefaultVolume = type == MoodType.Peace ? 0.1f : 0.4f
                };

                if (time == TimeOfDaySound.FullDay)
                {
                    AddNewSound(soundSource, TimeOfDaySound.Day , type);
                    AddNewSound(soundSource, TimeOfDaySound.Night, type);
                }
                else
                {
                    AddNewSound(soundSource, time, type);
                }

                PreLoadSound(soundSource.SoundAlias, soundSource.SoundFilePath, soundSource.DefaultVolume, soundSource.Power);
            }
        }

        private void AddNewSound(MoodsSoundSource sound, TimeOfDaySound timeofDay, MoodType type)
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

        protected override void PlayBlockPut(Vector3I blockPos)
        {
            SoundEngine.StartPlay3D("Put", new Vector3(blockPos.X + 0.5f, blockPos.Y + 0.5f, blockPos.Z + 0.5f));
        }

        protected override void PlayBlockTake(Vector3I blockPos)
        {
            SoundEngine.StartPlay3D("Take", new Vector3(blockPos.X + 0.5f, blockPos.Y + 0.5f, blockPos.Z + 0.5f));
        }

    }
}
