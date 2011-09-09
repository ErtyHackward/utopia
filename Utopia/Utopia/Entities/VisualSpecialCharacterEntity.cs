using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines;
using SharpDX;
using Utopia.Action;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.InputManager;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks.Entities.Concrete;
using S33M3Engines.D3D;

namespace Utopia.Entities
{
    public abstract class VisualSpecialCharacterEntity : VisualCharacterEntity
    {
        #region Private Variables
        #endregion

        #region Public Variables/Properties
        public readonly SpecialCharacterEntity SpecialCharacterEntity;
        #endregion

        public VisualSpecialCharacterEntity(D3DEngine engine,
                                           Vector3 size,
                                           IDynamicEntity dynamicEntity,
                                           ActionsManager actions,
                                           InputsManager inputsManager,
                                           SingleArrayChunkContainer cubesHolder,
                                           VoxelMeshFactory voxelMeshFactory,
                                           VoxelEntity voxelEntity,
                                           SpecialCharacterEntity entity)
            : base(engine, size, dynamicEntity, actions, inputsManager, cubesHolder, voxelMeshFactory, voxelEntity, entity)
        {
            SpecialCharacterEntity = entity;
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
