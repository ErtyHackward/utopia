using ProtoBuf;
using S33M3CoreComponents.Inputs.Actions;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;
using SharpDX;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Represents a dynamic entity state. Hold view position, picked block, picked entity etc
    /// </summary>
    [ProtoContract]
    public class DynamicEntityState
    {
        /// <summary>
        /// Global position of currently picked block
        /// </summary>
        [ProtoMember(1)]
        public Vector3I PickedBlockPosition;

        /// <summary>
        /// Get an offset from block face that specify the location point being clicked (face origin being 0;0;0).
        /// </summary>
        [ProtoMember(2)]
        public Vector3 PickedBlockFaceOffset;

        /// <summary>
        /// Is the entity a block at range, ready to by "picked-up"
        /// </summary>
        [ProtoMember(3)]
        public bool IsBlockPicked;

        /// <summary>
        /// Is the entity ready to by "picked-up"
        /// </summary>
        [ProtoMember(4)]
        public bool IsEntityPicked;

        /// <summary>
        /// Global position of new block to be inserted
        /// </summary>
        [ProtoMember(5)]
        public Vector3I NewBlockPosition;

        /// <summary>
        /// Gets entity that currently picked by entity
        /// </summary>
        [ProtoMember(6)]
        public Vector3D PickedEntityPosition;

        /// <summary>
        /// Gets entity that currently picked by entity
        /// </summary>
        [ProtoMember(7)]
        public EntityLink PickedEntityLink;

        /// <summary>
        /// Gets or sets block/entity intersection point
        /// </summary>
        [ProtoMember(8)]
        public Vector3 PickPoint;

        /// <summary>
        /// Gets or sets block/entity normale from the point
        /// </summary>
        [ProtoMember(9)]
        public Vector3I PickPointNormal;

        /// <summary>
        /// Indicates if current use operation is happened on mouse up event (or down if the value is false)
        /// </summary>
        [ProtoMember(10)]
        public bool MouseUp;

        /// <summary>
        /// Active mouse button
        /// </summary>
        [ProtoMember(11)]
        public MouseButton MouseButton;

        /// <summary>
        /// Tool specific data here
        /// </summary>
        [ProtoMember(12)]
        public ToolState ToolState;

        /// <summary>
        /// Random number
        /// </summary>
        [ProtoMember(13)]
        public int Entropy { get; set; }

        public DynamicEntityState Clone()
        {
            return (DynamicEntityState)this.MemberwiseClone();
        }
    }
}
