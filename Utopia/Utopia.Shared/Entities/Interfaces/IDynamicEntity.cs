using System;
using SharpDX;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;

namespace Utopia.Shared.Entities.Interfaces
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
        /// Occurs when entity change its displacement mode
        /// </summary>
        event EventHandler<EntityDisplacementModeEventArgs> DisplacementModeChanged;

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

        /// <summary>
        /// Gets or sets entity state
        /// </summary>
        DynamicEntityState EntityState { get; set; }

        /// <summary>
        /// Gets an unique entity identificator
        /// </summary>
        uint DynamicId { get; set; }

        /// <summary>
        /// Gets or sets entity head rotation
        /// </summary>
        Quaternion HeadRotation { get; set; }

        /// <summary>
        /// Gets or sets entity body rotation
        /// </summary>
        Quaternion BodyRotation { get; set; }

        /// <summary>
        /// Fires use event from current entity state
        /// </summary>
        IToolImpact ToolUse(ITool tool);

        /// <summary>
        /// Indicates if user can do any changes in the world or not
        /// </summary>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// Indicates if the player can change its mode to fly
        /// </summary>
        bool CanFly { get; set; }
    }
}