using System;
using System.Collections.Generic;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities.Interfaces;
using S33M3DXEngine.Main.Interfaces;
using Utopia.Shared.Entities.Events;

namespace Utopia.Entities.Managers.Interfaces
{
    /// <summary>
    /// Keeps a collection of IDynamicEntities received from the server.
    /// Responsible to draw them, also used to check collission detection with the player
    /// Draws the player entity if player is in 3rd person mode
    /// </summary>
    public interface IDynamicEntityManager : IDrawableComponent
    {
        event EventHandler<DynamicEntityEventArgs> EntityAdded;

        event EventHandler<DynamicEntityEventArgs> EntityRemoved;

        void AddEntity(IDynamicEntity entity);

        void RemoveEntity(IDynamicEntity entity);

        void RemoveEntityById(uint entityId,bool dispose=true);

        IDynamicEntity GetEntityById(uint p);
    
        List<IVisualVoxelEntityContainer> DynamicEntities { get; set; }

        /// <summary>
        /// Gets or sets current player entity to display
        /// Set to null in first person mode
        /// </summary>
        IDynamicEntity PlayerEntity { get; set; }
    }

}
