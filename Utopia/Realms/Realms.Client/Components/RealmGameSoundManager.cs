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
            PreLoadSound("Put", @"Sounds\Blocks\put.adpcm.wav", 0.3f, 12.0f, 50);
            PreLoadSound("Take", @"Sounds\Blocks\take.adpcm.wav", 0.3f, 12.0f, 50);
            PreLoadSound("Hurt", @"Sounds\Events\hurt.adpcm.wav", 0.3f, 16.0f, 100);
            PreLoadSound("Dying", @"Sounds\Events\dying.adpcm.wav", 0.5f, 16.0f, 1000);

            if (playerEntityManager is PlayerEntityManager)
            {
                var playerManager = playerEntityManager as PlayerEntityManager;
            }
        }

        #region Sound on Events

        protected override void PlayBlockPut(Vector3I blockPos)
        {
            SoundEngine.StartPlay3D("Put", new Vector3(blockPos.X + 0.5f, blockPos.Y + 0.5f, blockPos.Z + 0.5f));
        }

        protected override void PlayBlockTake(Vector3I blockPos)
        {
            SoundEngine.StartPlay3D("Take", new Vector3(blockPos.X + 0.5f, blockPos.Y + 0.5f, blockPos.Z + 0.5f));
        }

        protected override void StaticEntityAdd(object sender, StaticEventArgs e)
        {
            if (e.Entity is IItem)
            {
                var item = e.Entity as IItem;
                var putSound = item.PutSound;
                if (putSound != null)
                {
                    SoundEngine.StartPlay3D(putSound, e.Entity.Position.AsVector3());
                }
            }
        }

        protected override void StaticEntityRemoved(object sender, StaticEventArgs e)
        {
            if (e.Entity != null) SoundEngine.StartPlay3D("Take", e.Entity.Position.AsVector3());
        }

        #endregion

    }
}
