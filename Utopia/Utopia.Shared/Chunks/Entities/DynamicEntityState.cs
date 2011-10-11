﻿using SharpDX;
using Utopia.Shared.Structs;
using S33M3Engines.Shared.Math;

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
        public Vector3I PickedBlockPosition;

        /// <summary>
        /// Is the entity a block at range, ready to by "picked-up"
        /// </summary>
        public bool IsPickingActive;

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
        public uint PickedEntityId;

        public bool IsEntityPicked;
    }

}
