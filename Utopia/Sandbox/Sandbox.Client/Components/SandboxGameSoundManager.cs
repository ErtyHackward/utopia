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
using Utopia.Shared.Structs;

namespace Sandbox.Client.Components
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
            PreLoadSound("Put", @"Sounds\Blocks\put.adpcm.wav", 0.3f, 12.0f);
            PreLoadSound("Take", @"Sounds\Blocks\take.adpcm.wav", 0.3f, 12.0f);
            PreLoadSound("Hurt", @"Sounds\Events\hurt.adpcm.wav", 0.3f, 16.0f);
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

        protected override void playerEntityManager_OnLanding(double fallHeight, TerraCubeWithPosition landedCube)
        {
            if (fallHeight > 3 && fallHeight <= 10)
            {
                SoundEngine.StartPlay2D("Hurt", 0.3f);
            }
            else
            {
                if (fallHeight > 10)
                {
                    SoundEngine.StartPlay2D("Hurt", 1.0f);
                }
            }
        }

        protected override void StaticEntityAdd(object sender, StaticEventArgs e)
        {
            if (e.Entity is IItem)
            {
                var item = e.Entity as IItem;
                var putSound = item.PutSound;
                if (!string.IsNullOrEmpty(putSound))
                {
                    SoundEngine.StartPlay3D(putSound, putSound, e.Entity.Position.AsVector3());
                }
            }
        }

        protected override void StaticEntityRemoved(object sender, StaticEventArgs e)
        {
            if(e.Entity != null) SoundEngine.StartPlay3D("Take", e.Entity.Position.AsVector3());
        }

        #endregion

    }
}
