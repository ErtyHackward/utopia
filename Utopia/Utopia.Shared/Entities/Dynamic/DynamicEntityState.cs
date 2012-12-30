using Utopia.Shared.Structs;
using S33M3Resources.Structs;
using SharpDX;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Represents a dynamic entity state. Hold view position, picked block, picked entity etc
    /// </summary>
    public struct DynamicEntityState
    {
        /// <summary>
        /// Global position of currently picked block
        /// </summary>
        public Vector3I PickedBlockPosition;

        /// <summary>
        /// Get an offset from block face that specify the location point being clicked (face origin being 0;0;0).
        /// </summary>
        public Vector3 PickedBlockFaceOffset;

        /// <summary>
        /// Is the entity a block at range, ready to by "picked-up"
        /// </summary>
        public bool IsBlockPicked;

        /// <summary>
        /// Is the entity ready to by "picked-up"
        /// </summary>
        public bool IsEntityPicked;

        /// <summary>
        /// Global position of new block to be inserted
        /// </summary>
        public Vector3I NewBlockPosition;

        /// <summary>
        /// Gets entity that currently picked by entity
        /// </summary>
        public Vector3D PickedEntityPosition;

        /// <summary>
        /// Gets entity that currently picked by entity
        /// </summary>
        public EntityLink PickedEntityLink;

        /// <summary>
        /// Gets or sets block/entity intersection point
        /// </summary>
        public Vector3 PickPoint;

        /// <summary>
        /// Gets or sets block/entity normale from the point
        /// </summary>
        public Vector3 PickPointNormal;

    }

}
