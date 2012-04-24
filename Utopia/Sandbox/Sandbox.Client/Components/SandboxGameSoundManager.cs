using Utopia.Components;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.Entities.Managers;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities.Interfaces;
using IrrKlang;
using Utopia.Shared.Chunks;
using Utopia.Entities.Managers.Interfaces;

namespace Sandbox.Client.Components
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

            RegisterStepSound(CubeId.Water, "Sounds\\Footsteps\\footsteps_water01.ogg");

            // ambiance
            RegisterCubeAmbientSound(CubeId.Water, "Sounds\\Ambiance\\water_stream.ogg");

        }
    }
}
