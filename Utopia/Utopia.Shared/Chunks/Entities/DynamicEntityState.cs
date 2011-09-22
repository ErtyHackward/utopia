using SharpDX;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Represents a dynamic entity state. Hold view position, picked block, picked entity etc
    /// </summary>
    public struct DynamicEntityState
    {
        /// <summary>
        /// Global position of currently picked block
        /// </summary>
        public Location3<int> PickedBlockPosition;

        /// <summary>
        /// Is the entity a block at range, ready to by "picked-up"
        /// </summary>
        public bool IsBlockPicked;

        /// <summary>
        /// Global position of new block to be inserted
        /// </summary>
        public Location3<int> NewBlockPosition;

        /// <summary>
        /// Gets entity that currently picked by entity
        /// </summary>
        public uint PickedEntityId;
    }

}
