using S33M3Resources.Structs;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using S33M3CoreComponents.Sound;
using SharpDX;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.GameClocks;
using Utopia.Entities.Managers;
using Utopia.Shared.World;
using Utopia.Sounds;
using Utopia.Shared.Structs;

namespace Realms.Client.Components
{
    public class RealmGameSoundManager : GameSoundManager
    {
        public RealmGameSoundManager( ISoundEngine soundEngine,
                                    CameraManager<ICameraFocused> cameraManager,
                                    SingleArrayChunkContainer singleArray,
                                    IVisualDynamicEntityManager dynamicEntityManager,
                                    IChunkEntityImpactManager chunkEntityImpactManager,
                                    IWorldChunks worldChunk,
                                    IClock gameClockTime,
                                    PlayerEntityManager playerEntityManager,
                                    VisualWorldParameters visualWorldParameters,
                                    IClock worlClock)
            : base(soundEngine, cameraManager, singleArray, dynamicEntityManager, chunkEntityImpactManager, worldChunk, gameClockTime, playerEntityManager, visualWorldParameters, worlClock)
        {
            PreLoadSound("Hurt", @"Sounds\Events\hurt.adpcm.wav", 0.3f, 16.0f, 100);
            PreLoadSound("Dying", @"Sounds\Events\dying.adpcm.wav", 0.5f, 16.0f, 1000);
        }
    }
}
