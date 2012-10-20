using S33M3Resources.Structs;
using Utopia.Components;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.Shared.Entities.Interfaces;
using IrrKlang;
using Utopia.Shared.Chunks;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Shared.Configuration;

namespace Realms.Client.Components
{
    public class SandboxGameSoundManager : GameSoundManager
    {
        public SandboxGameSoundManager( ISoundEngine soundEngine,
                                    CameraManager<ICameraFocused> cameraManager,
                                    SingleArrayChunkContainer singleArray,
                                    IDynamicEntityManager dynamicEntityManager,
                                    IDynamicEntity player,
                                    IChunkEntityImpactManager chunkEntityImpactManager)
            : base(soundEngine, cameraManager, singleArray, dynamicEntityManager, player, chunkEntityImpactManager)
        {

            // steps

            RegisterStepSound(RealmConfiguration.CubeId.Snow, "Sounds\\Footsteps\\footsteps_snow01.ogg");
            //RegisterStepSound(CubeId.Snow, "Sounds\\Footsteps\\footsteps_snow02.ogg");

            RegisterStepSound(RealmConfiguration.CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass01.ogg");
            //RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass02.ogg");
            //RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass03.ogg");
            //RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass04.ogg");

            RegisterStepSound(RealmConfiguration.CubeId.Sand, "Sounds\\Footsteps\\footsteps_sand01.ogg");
            RegisterStepSound(RealmConfiguration.CubeId.Sand, "Sounds\\Footsteps\\footsteps_sand02.ogg");
            RegisterStepSound(RealmConfiguration.CubeId.Sand, "Sounds\\Footsteps\\footsteps_sand03.ogg");

            RegisterStepSound(RealmConfiguration.CubeId.Dirt, "Sounds\\Footsteps\\footsteps_dirt01.ogg");
            RegisterStepSound(RealmConfiguration.CubeId.Dirt, "Sounds\\Footsteps\\footsteps_dirt02.ogg");
            RegisterStepSound(RealmConfiguration.CubeId.Dirt, "Sounds\\Footsteps\\footsteps_dirt03.ogg");
            RegisterStepSound(RealmConfiguration.CubeId.Dirt, "Sounds\\Footsteps\\footsteps_dirt04.ogg");

            RegisterStepSound(RealmConfiguration.CubeId.Stone, "Sounds\\Footsteps\\footsteps_stone01.ogg");
            RegisterStepSound(RealmConfiguration.CubeId.Stone, "Sounds\\Footsteps\\footsteps_stone02.ogg");
            RegisterStepSound(RealmConfiguration.CubeId.Stone, "Sounds\\Footsteps\\footsteps_stone03.ogg");
            RegisterStepSound(RealmConfiguration.CubeId.Stone, "Sounds\\Footsteps\\footsteps_stone04.ogg");

            RegisterStepSound(RealmConfiguration.CubeId.StillWater, "Sounds\\Footsteps\\footsteps_water01.ogg");

            // ambiance
            RegisterCubeAmbientSound(RealmConfiguration.CubeId.StillWater, "Sounds\\Ambiance\\water_stream.ogg");
            RegisterCubeAmbientSound(RealmConfiguration.CubeId.StillLava, "Sounds\\Ambiance\\lava.ogg");

            PreLoadSound("Sounds\\Blocks\\put.wav");
            PreLoadSound("Sounds\\Blocks\\take.wav");
        }

        public override void PlayBlockPut(Vector3I blockPos)
        {
            var sound = SoundEngine.Play3D("Sounds\\Blocks\\put.wav", blockPos.X + 0.5f, blockPos.Y + 0.5f, blockPos.Z + 0.5f);
            sound.MaxDistance = 16;
        }

        public override void PlayBlockTake(Vector3I blockPos)
        {
            var sound = SoundEngine.Play3D("Sounds\\Blocks\\take.wav", blockPos.X + 0.5f, blockPos.Y + 0.5f, blockPos.Z + 0.5f);
            sound.MaxDistance = 16;
        }
    }
}
