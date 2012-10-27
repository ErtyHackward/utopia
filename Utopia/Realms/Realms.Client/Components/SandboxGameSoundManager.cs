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
                                    IWorldChunks worldChunk)
            : base(soundEngine, cameraManager, singleArray, dynamicEntityManager, player, chunkEntityImpactManager, worldChunk)
        {

            // steps
            RegisterStepSound(RealmConfiguration.CubeId.Snow, @"Sounds\Footsteps\footsteps_snow01.adpcm");
            //RegisterStepSound(CubeId.Snow, "Sounds\\Footsteps\\footsteps_snow02.ogg");

            RegisterStepSound(RealmConfiguration.CubeId.Grass, @"Sounds\Footsteps\footsteps_grass01.adpcm");
            //RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass02.ogg");
            //RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass03.ogg");
            //RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass04.ogg");

            RegisterStepSound(RealmConfiguration.CubeId.Sand, @"Sounds\Footsteps\footsteps_sand01.adpcm");
            RegisterStepSound(RealmConfiguration.CubeId.Sand, @"Sounds\Footsteps\footsteps_sand02.adpcm");
            RegisterStepSound(RealmConfiguration.CubeId.Sand, @"Sounds\Footsteps\footsteps_sand03.adpcm");

            RegisterStepSound(RealmConfiguration.CubeId.Dirt, @"Sounds\Footsteps\footsteps_dirt01.adpcm");
            RegisterStepSound(RealmConfiguration.CubeId.Dirt, @"Sounds\Footsteps\footsteps_dirt02.adpcm");
            RegisterStepSound(RealmConfiguration.CubeId.Dirt, @"Sounds\Footsteps\footsteps_dirt03.adpcm");
            RegisterStepSound(RealmConfiguration.CubeId.Dirt, @"Sounds\Footsteps\footsteps_dirt04.adpcm");

            RegisterStepSound(RealmConfiguration.CubeId.Stone, @"Sounds\Footsteps\footsteps_stone01.adpcm");
            RegisterStepSound(RealmConfiguration.CubeId.Stone, @"Sounds\Footsteps\footsteps_stone02.adpcm");
            RegisterStepSound(RealmConfiguration.CubeId.Stone, @"Sounds\Footsteps\footsteps_stone03.adpcm");
            RegisterStepSound(RealmConfiguration.CubeId.Stone, @"Sounds\Footsteps\footsteps_stone04.adpcm");

            RegisterStepSound(RealmConfiguration.CubeId.StillWater, @"Sounds\Footsteps\footsteps_water01.adpcm");

            // ambiance
            RegisterCubeAmbientSound(RealmConfiguration.CubeId.StillWater, @"Sounds\Ambiance\water_stream.adpcm");
            RegisterCubeAmbientSound(RealmConfiguration.CubeId.StillLava, @"Sounds\Ambiance\lava.adpcm");

            PreLoadSound(@"Sounds\Blocks\put.wav");
            PreLoadSound(@"Sounds\Blocks\take.wav");
        }

        protected override void PlayBlockPut(Vector3I blockPos)
        {
            SoundEngine.StartPlay3D(@"Sounds\Blocks\put.wav", new Vector3(blockPos.X + 0.5f, blockPos.Y + 0.5f, blockPos.Z + 0.5f));
        }

        protected override void PlayBlockTake(Vector3I blockPos)
        {
            SoundEngine.StartPlay3D(@"Sounds\Blocks\take.wav", new Vector3(blockPos.X + 0.5f, blockPos.Y + 0.5f, blockPos.Z + 0.5f));
        }
    }
}
