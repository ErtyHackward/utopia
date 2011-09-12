using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Action;
using Utopia.InputManager;
using Utopia.Shared.Chunks;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks.Entities.Concrete;
using S33M3Engines.D3D;
using S33M3Engines.Cameras;
using S33M3Engines.WorldFocus;

namespace Utopia.Entities
{
    public abstract class VisualCharacterEntity : VisualDynamicEntity
    {
        #region Private Variables
        #endregion

        #region Public Variables/Properties
        public readonly CharacterEntity CharacterEntity;
        #endregion

        public VisualCharacterEntity(D3DEngine engine,
                                     CameraManager cameraManager,
                                     WorldFocusManager worldFocusManager,
                                     ActionsManager actions,
                                     InputsManager inputsManager,
                                     SingleArrayChunkContainer cubesHolder,
                                     VoxelMeshFactory voxelMeshFactory,
                                     VoxelEntity voxelEntity,
                                     CharacterEntity entity)
            : base(engine, cameraManager,worldFocusManager, entity, actions, inputsManager, cubesHolder, voxelMeshFactory, voxelEntity)
        {
            CharacterEntity = entity;
        }

        #region Private Methods
        #endregion

        #region Public Methods
        public override void Draw()
        {
            base.Draw();
        }

        public override void Update(ref GameTime timeSpent)
        {
            base.Update(ref timeSpent);
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            base.Interpolation(ref interpolationHd, ref interpolationLd);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }
}
