using System;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Events
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
        public ITool Tool { get; set; }

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
        public EntityLink PickedEntityLink { get; set; }

        public bool IsBlockPicked { get; set; }

        public bool IsEntityPicked { get; set; }

        public ToolUseMode UseMode { get; set; }

        /// <summary>
        /// Creates event args from entity state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static EntityUseEventArgs FromState(DynamicEntityState state)
        {
            var e = new EntityUseEventArgs
                        {
                            PickedBlockPosition = state.PickedBlockPosition,
                            NewBlockPosition = state.NewBlockPosition,
                            PickedEntityPosition = state.PickedEntityPosition,
                            IsBlockPicked = state.IsBlockPicked,
                            IsEntityPicked = state.IsEntityPicked,
                            PickedEntityLink = state.PickedEntityLink
                        };

            return e;
        }
    }
}