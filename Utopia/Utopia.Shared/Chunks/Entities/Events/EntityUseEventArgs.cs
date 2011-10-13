using System;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Structs;
using S33M3Engines.Shared.Math;

namespace Utopia.Shared.Chunks.Entities.Events
{
    public class EntityUseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets sender entity
        /// </summary>
        public IDynamicEntity Entity { get; set; }

        /// <summary>
        /// Gets or sets the tool was used (maybe null)
        /// </summary>
        public Tool Tool { get; set; }

        /// <summary>
        /// Gets character view vector at using moment
        /// </summary>
        public Vector3 SpaceVector { get; set; }

        /// <summary>
        /// Global position of currently picked block
        /// </summary>
        public Vector3I PickedBlockPosition { get; set; }

        /// <summary>
        /// Global position of new block to be inserted
        /// </summary>
        public Vector3I NewBlockPosition { get; set; }

        /// <summary>
        /// Gets entity that currently picked by character
        /// </summary>
        public Vector3D PickedEntityPosition { get; set; }

        /// <summary>
        /// Gets entity that currently picked by character
        /// </summary>
        public uint PickedEntityId { get; set; }

        public bool IsBlockPicked { get; set; }
        public bool IsEntityPicked { get; set; }

        /// <summary>
        /// Creates event args from entity state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="use"></param>
        /// <returns></returns>
        public static EntityUseEventArgs FromState(DynamicEntityState state)
        {
            var e = new EntityUseEventArgs();

            e.PickedBlockPosition = state.PickedBlockPosition;
            e.NewBlockPosition = state.NewBlockPosition;
            e.PickedEntityPosition = state.PickedEntityPosition;
            e.IsBlockPicked = state.IsPickingActive && (!state.IsEntityPicked);
            e.IsEntityPicked = state.IsPickingActive && state.IsEntityPicked;
            e.PickedEntityId = state.PickedEntityId;
            return e;
        }
    }
}