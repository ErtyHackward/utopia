using System;
using System.Collections.Generic;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Interfaces
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
        /// Gets container grid size
        /// </summary>
        Vector2I GridSize { get; }

        /// <summary>
        /// Gets parent entity
        /// </summary>
        IEntity Parent { get; }

        /// <summary>
        /// Tries to put items
        /// </summary>
        /// <param name="item"></param>
        /// <param name="count"></param>
        /// <returns>True if succeed otherwise false</returns>
        bool PutItem(IItem item, int count = 1);

        /// <summary>
        /// Tries to put items into slot specified
        /// </summary>
        /// <param name="item"></param>
        /// <param name="position"></param>
        /// <param name="itemsCount"></param>
        /// <returns></returns>
        bool PutItem(IItem item, Vector2I position, int itemsCount = 1);

        /// <summary>
        /// Tries to get item from slot. Checks the Entity type 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="itemsCount"></param>
        /// <returns>True if succeed otherwise false</returns>
        bool TakeItem(Vector2I position, int itemsCount = 1);

        /// <summary>
        /// Returns slot without taking it from the container
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        T PeekSlot(Vector2I pos);

        /// <summary>
        /// Puts the item to already occupied slot (Items should have different type)
        /// </summary>
        /// <param name="itemsCount"></param>
        /// <param name="slotTaken"></param>
        /// <param name="item"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        bool PutItemExchange(IItem item, Vector2I position, int itemsCount, out T slotTaken);

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
