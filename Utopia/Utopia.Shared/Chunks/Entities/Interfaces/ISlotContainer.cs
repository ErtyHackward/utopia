using System;
using System.Collections.Generic;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    /// <summary>
    /// Describes a container for entities
    /// </summary>
    public interface ISlotContainer<T> : IEnumerable<T>
    {
        /// <summary>
        /// Occurs when the item was taken from the container
        /// </summary>
        event EventHandler<EntityContainerEventArgs<T>> ItemTaken;

        /// <summary>
        /// Occurs when the item was put into the container
        /// </summary>
        event EventHandler<EntityContainerEventArgs<T>> ItemPut;

        /// <summary>
        /// Gets maximum container capacity
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Gets container grid size
        /// </summary>
        Vector2I GridSize { get; }
        
        /// <summary>
        /// Tries to put item into slot specified
        /// </summary>
        /// <param name="slot"></param>
        /// <returns>True if succeed otherwise false</returns>
        bool PutItem(T slot);

        /// <summary>
        /// Tries to get item from slot. Checks the Entity type 
        /// </summary>
        /// <param name="slot"></param>
        /// <returns>True if succeed otherwise false</returns>
        bool TakeItem(T slot);

        /// <summary>
        /// Tries to get item from slot. Slot entity will be filled from slot position
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        T TakeSlot(T slot);

    }

    public class EntityContainerEventArgs<T> : EventArgs
    {
        public T Slot { get; set; }
    }
}
