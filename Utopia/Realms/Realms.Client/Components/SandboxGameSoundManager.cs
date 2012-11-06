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
                                    VisualWorldParameters visualWorldParameters)
            : base(soundEngine, cameraManager, singleArray, dynamicEntityManager, player, chunkEntityImpactManager, worldChunk, gameClockTime, playerEntityManager, visualWorldParameters)
        {

            //Buffer cube walking sound
            foreach (var cube in visualWorldParameters.WorldParameters.Configuration.CubeProfiles.Where(x => x.WalkingOverSound.Count > 0))
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

            //// steps
            //RegisterStepSound(WorldConfiguration.CubeId.Snow, @"Sounds\Footsteps\footsteps_snow01.adpcm.wav");
            ////RegisterStepSound(CubeId.Snow, "Sounds\\Footsteps\\footsteps_snow02.ogg");

            //RegisterStepSound(WorldConfiguration.CubeId.Grass, @"Sounds\Footsteps\footsteps_grass01.adpcm.wav");
            ////RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass02.ogg");
            ////RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass03.ogg");
            ////RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass04.ogg");

            //RegisterStepSound(WorldConfiguration.CubeId.Sand, @"Sounds\Footsteps\footsteps_sand01.adpcm.wav");
            //RegisterStepSound(WorldConfiguration.CubeId.Sand, @"Sounds\Footsteps\footsteps_sand02.adpcm.wav");
            //RegisterStepSound(WorldConfiguration.CubeId.Sand, @"Sounds\Footsteps\footsteps_sand03.adpcm.wav");

            //RegisterStepSound(WorldConfiguration.CubeId.Dirt, @"Sounds\Footsteps\footsteps_dirt01.adpcm.wav");
            //RegisterStepSound(WorldConfiguration.CubeId.Dirt, @"Sounds\Footsteps\footsteps_dirt02.adpcm.wav");
            //RegisterStepSound(WorldConfiguration.CubeId.Dirt, @"Sounds\Footsteps\footsteps_dirt03.adpcm.wav");
            //RegisterStepSound(WorldConfiguration.CubeId.Dirt, @"Sounds\Footsteps\footsteps_dirt04.adpcm.wav");

            //RegisterStepSound(WorldConfiguration.CubeId.Stone, @"Sounds\Footsteps\footsteps_stone01.adpcm.wav");
            //RegisterStepSound(WorldConfiguration.CubeId.Stone, @"Sounds\Footsteps\footsteps_stone02.adpcm.wav");
            //RegisterStepSound(WorldConfiguration.CubeId.Stone, @"Sounds\Footsteps\footsteps_stone03.adpcm.wav");
            //RegisterStepSound(WorldConfiguration.CubeId.Stone, @"Sounds\Footsteps\footsteps_stone04.adpcm.wav");

            //RegisterStepSound(WorldConfiguration.CubeId.StillWater, @"Sounds\Footsteps\footsteps_water01.adpcm.wav");

            PreLoadSound("Put", @"Sounds\Blocks\put.wav", 0.3f, 12.0f);
            PreLoadSound("Take", @"Sounds\Blocks\take.wav", 0.3f, 12.0f);
            PreLoadSound("Hurt", @"Sounds\Events\hurt.wav", 0.3f, 16.0f);
            PreLoadSound("WaterDrop", @"Sounds\Events\waterdrop.wav", 1.0f, 16.0f);
            PreLoadSound("Peaceful", @"Sounds\Moods\peaceful.adpcm.wav", 0.1f, 16.0f);
            PreLoadSound("Cavern", @"Sounds\Ambiance\cavern.adpcm.wav", 1.0f, 16.0f);
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
