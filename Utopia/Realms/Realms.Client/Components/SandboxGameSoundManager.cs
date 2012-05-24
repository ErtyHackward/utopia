using S33M3Resources.Structs;
using Utopia.Components;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities.Interfaces;
using IrrKlang;
using Utopia.Shared.Chunks;
using Utopia.Entities.Managers.Interfaces;

namespace Realms.Client.Components
{
    public class SandboxGameSoundManager : GameSoundManager
    {
        public SandboxGameSoundManager( ISoundEngine soundEngine,
                                    CameraManager<ICameraFocused> cameraManager,
                                    SingleArrayChunkContainer singleArray,
                                    IDynamicEntityManager dynamicEntityManager,
                                    IDynamicEntity player)
            : base(soundEngine, cameraManager, singleArray, dynamicEntityManager, player)
        {

            // steps
            RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass01.ogg");
            //RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass02.ogg");
            //RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass03.ogg");
            //RegisterStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass04.ogg");

            RegisterStepSound(CubeId.Sand, "Sounds\\Footsteps\\footsteps_sand01.ogg");
            RegisterStepSound(CubeId.Sand, "Sounds\\Footsteps\\footsteps_sand02.ogg");
            RegisterStepSound(CubeId.Sand, "Sounds\\Footsteps\\footsteps_sand03.ogg");

            RegisterStepSound(CubeId.Dirt, "Sounds\\Footsteps\\footsteps_dirt01.ogg");
            RegisterStepSound(CubeId.Dirt, "Sounds\\Footsteps\\footsteps_dirt02.ogg");
            RegisterStepSound(CubeId.Dirt, "Sounds\\Footsteps\\footsteps_dirt03.ogg");
            RegisterStepSound(CubeId.Dirt, "Sounds\\Footsteps\\footsteps_dirt04.ogg");

            RegisterStepSound(CubeId.Stone, "Sounds\\Footsteps\\footsteps_stone01.ogg");
            RegisterStepSound(CubeId.Stone, "Sounds\\Footsteps\\footsteps_stone02.ogg");
            RegisterStepSound(CubeId.Stone, "Sounds\\Footsteps\\footsteps_stone03.ogg");
            RegisterStepSound(CubeId.Stone, "Sounds\\Footsteps\\footsteps_stone04.ogg");

            RegisterStepSound(CubeId.StillWater, "Sounds\\Footsteps\\footsteps_water01.ogg");

            // ambiance
            RegisterCubeAmbientSound(CubeId.StillWater, "Sounds\\Ambiance\\water_stream.ogg");

            PreLoadSound("Sounds\\Blocks\\put.wav");
            PreLoadSound("Sounds\\Blocks\\take.wav");

        }

        public void PlayBlockPut(Vector3I blockPos)
        {
            var sound = SoundEngine.Play3D("Sounds\\Blocks\\put.wav", blockPos.X + 0.5f, blockPos.Y + 0.5f, blockPos.Z + 0.5f);
            sound.MaxDistance = 16;
        }

        public void PlayBlockTake(Vector3I blockPos)
        {
            var sound = SoundEngine.Play3D("Sounds\\Blocks\\take.wav", blockPos.X + 0.5f, blockPos.Y + 0.5f, blockPos.Z + 0.5f);
            sound.MaxDistance = 16;
        }
    }
}
