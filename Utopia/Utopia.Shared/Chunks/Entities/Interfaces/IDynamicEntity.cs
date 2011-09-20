using System;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    /// <summary>
    /// Describes dynamic entity. Dynamic entity can listen events of entities from surrounding areas and perform AI logic in update
    /// </summary>
    public interface IDynamicEntity : IVoxelEntity
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
        /// Entity displacement mode
        /// </summary>
        EntityDisplacementModes DisplacementMode { get; set; }

    }
}