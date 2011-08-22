using System;
using System.Collections.Generic;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Describes a container for entities
    /// </summary>
    public interface IEntityContainer : IEnumerable<ContainedSlot>
    {
        /// <summary>
        /// Occurs when the item was taken from the container
        /// </summary>
        event EventHandler<EntityContainerEventArgs> ItemTaken;

        /// <summary>
        /// Occurs when the item was put into the container
        /// </summary>
        event EventHandler<EntityContainerEventArgs> ItemPut;

        /// <summary>
        /// Gets maximum container capacity
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Gets container grid size
        /// </summary>
        Location2<byte> GridSize { get; }
        
        /// <summary>
        /// Tries to put item into slot specified
        /// </summary>
        /// <param name="slot"></param>
        /// <returns>True if succeed otherwise false</returns>
        bool PutItem(ContainedSlot slot);

        /// <summary>
        /// Tries to get item from slot
        /// </summary>
        /// <param name="slot"></param>
        /// <returns>True if succeed otherwise false</returns>
        bool GetItem(ContainedSlot slot);

    }

    public class EntityContainerEventArgs : EventArgs
    {
        public ContainedSlot Slot { get; set; }
    }
}
