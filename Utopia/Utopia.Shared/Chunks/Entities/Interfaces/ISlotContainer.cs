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
        /// Occurs when one item being replaced by other
        /// </summary>
        event EventHandler<EntityContainerEventArgs<T>> ItemExchanged;

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

        /// <summary>
        /// Returns slot without taking it from the container
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        T PeekSlot(Vector2I pos);

        /// <summary>
        /// Puts the item to already occupied slot (Items should have different type)
        /// </summary>
        /// <param name="slotPut"></param>
        /// <param name="slotTaken"></param>
        /// <returns></returns>
        bool PutItemExchange(T slotPut, out T slotTaken);

    }

    public class EntityContainerEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets item that was taken or put
        /// </summary>
        public T Slot { get; set; }

        /// <summary>
        /// Gets item that was taken by exchange operation
        /// </summary>
        public T Exchanged { get; set; }
    }
}
