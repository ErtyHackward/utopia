using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using S33M3DXEngine.Main.Interfaces;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Interfaces;

namespace Utopia.Entities.Managers.Interfaces
{
    /// <summary>
    /// Keeps a collection of IDynamicEntities received from the server.
    /// Responsible to draw them, also used to check collission detection with the player
    /// Draws the player entity if player is in 3rd person mode
    /// </summary>
    public interface IVisualDynamicEntityManager : IDrawableComponent, IDynamicEntityManager
    {
        event EventHandler<DynamicEntityEventArgs> EntityAdded;

        event EventHandler<DynamicEntityEventArgs> EntityRemoved;

        void AddEntity(ICharacterEntity entity, bool withNetworkInterpolation);

        void RemoveEntity(ICharacterEntity entity);

        void RemoveEntityById(uint entityId, bool dispose=true);

        ICharacterEntity GetEntityById(uint p);


        List<VisualVoxelEntity> DynamicEntities { get; set; }

        /// <summary>
        /// Updates existing entity object (in case of visual or equipment changes)
        /// </summary>
        /// <param name="entity"></param>
        void UpdateEntity(ICharacterEntity entity);

        /// <summary>
        /// Change the voxel body from a dynamic entity
        /// </summary>
        /// <param name="entityId">The concerned dynamic entity</param>
        /// <param name="modelName">the voxel body to assign, if null, the default model body will be set</param>
        /// <param name="assignModelToEntity"></param>
        void UpdateEntityVoxelBody(uint entityId, string modelName = null, bool assignModelToEntity = true);

        void VoxelDraw(DeviceContext context, Matrix viewProjection);
    }
}
