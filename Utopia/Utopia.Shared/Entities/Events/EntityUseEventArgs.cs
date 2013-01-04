using System;
using SharpDX;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

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

        public Vector3 PickedBlockFaceOffset;

        public bool IsBlockPicked { get; set; }

        public bool IsEntityPicked { get; set; }

        public Vector3 PickPosition { get; set; }

        public Vector3I PickNormal { get; set; }

        /// <summary>
        /// Creates event args from entity state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static EntityUseEventArgs FromState(DynamicEntityState state, IDynamicEntity owner)
        {
            var e = new EntityUseEventArgs
                        {
                            PickedBlockPosition = state.PickedBlockPosition,
                            NewBlockPosition = state.NewBlockPosition,
                            PickedEntityPosition = state.PickedEntityPosition,
                            IsBlockPicked = state.IsBlockPicked,
                            IsEntityPicked = state.IsEntityPicked,
                            PickedEntityLink = state.PickedEntityLink,
                            PickedBlockFaceOffset = state.PickedBlockFaceOffset,
                            PickPosition = state.PickPoint,
                            PickNormal = state.PickPointNormal,
                            Entity = owner
                        };

            return e;
        }
    }
}