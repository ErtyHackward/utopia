using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Entities.Voxel;
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

        void AddEntity(IDynamicEntity entity, bool withNetworkInterpolation);

        void RemoveEntity(IDynamicEntity entity);

        void RemoveEntityById(uint entityId, bool dispose=true);

        IDynamicEntity GetEntityById(uint p);

    
        List<IVisualVoxelEntityContainer> DynamicEntities { get; set; }

        /// <summary>
        /// Gets or sets current player entity to display
        /// Set to null in first person mode
        /// </summary>
        IDynamicEntity PlayerEntity { get; set; }

        /// <summary>
        /// Updates existing entity object (in case of visual or equipment changes)
        /// </summary>
        /// <param name="entity"></param>
        void UpdateEntity(IDynamicEntity entity);

        void UpdateEntityVoxelBody(uint entityId, string ModelName = null, bool assignModelToEntity = true);

        void VoxelDraw(DeviceContext context, Matrix viewProjection);
    }
}
