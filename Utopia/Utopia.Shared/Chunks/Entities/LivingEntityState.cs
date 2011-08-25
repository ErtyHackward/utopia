using SharpDX;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Represents a living entity state. Hold view position, picked block, picked entity etc
    /// </summary>
    public struct LivingEntityState
    {
        /// <summary>
        /// Gets character view vector at using moment
        /// </summary>
        public Vector3 SpaceVector { get; set; }

        /// <summary>
        /// Global position of currently picked block
        /// </summary>
        public Location3<int> PickedBlockPosition { get; set; }

        /// <summary>
        /// Global position of new block to be inserted
        /// </summary>
        public Location3<int> NewBlockPosition { get; set; }

        /// <summary>
        /// Gets entity that currently picked by character
        /// </summary>
        public uint PickedEntityId { get; set; }
    }

}
