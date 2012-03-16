using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Components;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.Entities.Managers;
using Utopia.Shared.Chunks;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities.Interfaces;

namespace Sandbox.Client.Components
{
    public class SandboxSoundManager : SoundManager
    {
        public SandboxSoundManager(CameraManager<ICameraFocused> cameraManager, DynamicEntityManager dynamicEntityManager, IDynamicEntity player)
            : base(cameraManager, dynamicEntityManager, player)
        {
            SetGuiButtonSound("Sounds\\Interface\\button_press.wav");

            AddStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass01.ogg");
            AddStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass02.ogg");
            AddStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass03.ogg");
            AddStepSound(CubeId.Grass, "Sounds\\Footsteps\\footsteps_grass04.ogg");

            AddStepSound(CubeId.Sand, "Sounds\\Footsteps\\footsteps_sand01.ogg");
            AddStepSound(CubeId.Sand, "Sounds\\Footsteps\\footsteps_sand02.ogg");
            AddStepSound(CubeId.Sand, "Sounds\\Footsteps\\footsteps_sand03.ogg");

            AddStepSound(CubeId.Dirt, "Sounds\\Footsteps\\footsteps_dirt01.ogg");
            AddStepSound(CubeId.Dirt, "Sounds\\Footsteps\\footsteps_dirt02.ogg");
            AddStepSound(CubeId.Dirt, "Sounds\\Footsteps\\footsteps_dirt03.ogg");
            AddStepSound(CubeId.Dirt, "Sounds\\Footsteps\\footsteps_dirt04.ogg");

            AddStepSound(CubeId.Stone, "Sounds\\Footsteps\\footsteps_stone01.ogg");
            AddStepSound(CubeId.Stone, "Sounds\\Footsteps\\footsteps_stone02.ogg");
            AddStepSound(CubeId.Stone, "Sounds\\Footsteps\\footsteps_stone03.ogg");
            AddStepSound(CubeId.Stone, "Sounds\\Footsteps\\footsteps_stone04.ogg");

            AddStepSound(CubeId.Water, "Sounds\\Footsteps\\footsteps_water01.ogg");
            AddStepSound(CubeId.Water, "Sounds\\Footsteps\\footsteps_water02.ogg");
            AddStepSound(CubeId.Water, "Sounds\\Footsteps\\footsteps_water03.ogg");
            AddStepSound(CubeId.Water, "Sounds\\Footsteps\\footsteps_water04.ogg");

        }
    }
}
