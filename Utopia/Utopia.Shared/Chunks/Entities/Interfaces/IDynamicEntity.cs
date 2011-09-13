using System;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Management;

namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    /// <summary>
    /// Describes dynamic entity. Dynamic entity can listen events of entities from surrounding areas and perform AI logic in update
    /// </summary>
    public interface IDynamicEntity : IEntity
    {
        /// <summary>
        /// Occurs when entity changes its position
        /// </summary>
        event EventHandler<EntityMoveEventArgs> PositionChanged;

        /// <summary>
        /// Occurs when entity changes its view direction
        /// </summary>
        event EventHandler<EntityViewEventArgs> ViewChanged;

        /// <summary>
        /// Occurs when entity performs "use" operation
        /// </summary>
        event EventHandler<EntityUseEventArgs> Use;

        /// <summary>
        /// The speed at wich the dynamic entity can walk
        /// </summary>
        float MoveSpeed { get; set; }

        /// <summary>
        /// The speed at wich the dynamic is doing move rotation
        /// </summary>
        float RotationSpeed { get; set; }

        /// <summary>
        /// Perform actions when getting closer to area. Entity should add all needed event handlers
        /// </summary>
        /// <param name="area"></param>
        void AddArea(MapArea area);

        /// <summary>
        /// Perform actions when area is far away, entity should remove any event hadler it has
        /// </summary>
        /// <param name="area"></param>
        void RemoveArea(MapArea area);

        /// <summary>
        /// Perform dynamic update (AI logic)
        /// </summary>
        void Update(DateTime gameTime);

        /// <summary>
        /// Entity displacement mode
        /// </summary>
        EntityDisplacementModes DisplacementMode { get; set; }

        /// <summary>
        /// Gets or sets area there entity is now (not stored)
        /// </summary>
        MapArea CurrentArea { get; set; }
    }
}