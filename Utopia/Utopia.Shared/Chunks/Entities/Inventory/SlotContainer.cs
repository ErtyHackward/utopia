﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// Represents a base container implementation (this is not an entity)
    /// </summary>
    public class SlotContainer<T> : ISlotContainer<T>, IBinaryStorable where T: ContainedSlot, new()
    {
        private T[,] _items;
        private Vector2I _gridSize;
        private int _slotsCount;

        /// <summary>
        /// Occurs when the item was taken from the container
        /// </summary>
        public event EventHandler<EntityContainerEventArgs<T>> ItemTaken;

        protected void OnItemTaken(EntityContainerEventArgs<T> e)
        {
            var handler = ItemTaken;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when the item was put into the container
        /// </summary>
        public event EventHandler<EntityContainerEventArgs<T>> ItemPut;

        protected void OnItemPut(EntityContainerEventArgs<T> e)
        {
            var handler = ItemPut;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when item was put to non-empty slot
        /// </summary>
        public event EventHandler<EntityContainerEventArgs<T>> ItemExchanged;

        protected void OnItemExchanged(EntityContainerEventArgs<T> e)
        {
            var handler = ItemExchanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Creates new instance of container with gridSize specified
        /// </summary>
        /// <param name="containerGridSize"></param>
        public SlotContainer(Vector2I containerGridSize)
        {
            GridSize = containerGridSize;
        }

        /// <summary>
        /// Creates new instance of container with GridSize of 8x5 items
        /// </summary>
        public SlotContainer()
            : this(new Vector2I(8, 5))
        {
            
        }

        /// <summary>
        /// Gets maximum container capacity
        /// </summary>
        public int Capacity { get; set; }
        
        /// <summary>
        /// Gets container grid size
        /// </summary>
        public Vector2I GridSize
        {
            get { return _gridSize; }
            set { 
                _gridSize = value;
                //todo: copy of items to new container from old
                _items = new T[_gridSize.X, _gridSize.Y];
            }
        }
        
        public void Save(BinaryWriter writer)
        {
            // we need to save items count to be able to load again
            writer.Write(_slotsCount);

            // writing grid size
            writer.Write(_gridSize);

            // saving containing items
            foreach (var slot in this)
            {
                slot.Save(writer);
            }
        }

        public void Load(BinaryReader reader)
        {
            // read contained slots count
            _slotsCount = reader.ReadInt32();

            // read container grid size
            _gridSize = reader.ReadVector2I();

            // load contained slots (slot is count and entity example)
            for (int i = 0; i < _slotsCount; i++)
            {
                var containedSlot = new T();

                containedSlot.Load(reader);
                _items[containedSlot.GridPosition.X, containedSlot.GridPosition.Y] = containedSlot;
            }
        }

// ReSharper disable UnusedParameter.Local
        private void ValidatePosition(Vector2I position)
// ReSharper restore UnusedParameter.Local
        {
            if (position.X < 0 || position.Y < 0 || position.X >= _gridSize.X || position.Y >= _gridSize.Y)
                throw new ArgumentException("Slot position is unacceptable for this container");
        }

        /// <summary>
        /// Tries to put item into the inventory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="count"></param>
        /// <returns>True if item put into the inventory otherwise false</returns>
        public bool PutItem(IItem item, int count = 1)
        {
            // inventory is full?
            if (item.MaxStackSize == 1 && _slotsCount == _gridSize.X * _gridSize.Y)
                return false;

            if (item.MaxStackSize > 1)
            {
                // need to find uncomplete stack if exists

                var e = this.Any(s =>
                               {
                                   if (s.Item.StackType == item.StackType && s.ItemsCount + count <= item.MaxStackSize)
                                   {
                                       s.ItemsCount++;
                                       var t = new T { GridPosition = s.GridPosition, ItemsCount = count, Item = s.Item };
                                       OnItemPut(new EntityContainerEventArgs<T> { Slot = t });
                                       return true;
                                   }
                                   return false;
                               });

                if (e) return true;

            }

            // take first free cell and put item
            for (int x = 0; x < _gridSize.X; x++)
            {
                for (int y = 0; y < _gridSize.Y; y++)
                {
                    if (_items[x, y] == null)
                    {
                        _items[x, y] = new T { Item = item, GridPosition = new Vector2I(x, y), ItemsCount = count };
                        _slotsCount++;
                        OnItemPut(new EntityContainerEventArgs<T> { Slot = _items[x, y] });
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Tries to add an item into slot
        /// if slot has already an item, add to the stack
        /// if slot is empty, initialize it with slot.item
        /// </summary>
        /// <param name="slot"></param>
        /// <returns>True if succeed otherwise false</returns>
        public bool PutItem(T slot)
        {
            ValidatePosition(slot.GridPosition);

            if (slot.ItemsCount == 0)
                throw new InvalidOperationException("No items to put");

            var currentItem = _items[slot.GridPosition.X, slot.GridPosition.Y];
            
            if (currentItem != null)
            {
                // check if slot is busy by other entity (different entities are unstackable)
                if (currentItem.Item.StackType != slot.Item.StackType)
                    return false;

                // check for stack limit
                if (currentItem.ItemsCount + slot.ItemsCount > slot.Item.MaxStackSize)
                    return false;

                currentItem.ItemsCount += slot.ItemsCount;
            }
            else
            {
                // adding new slot
                _items[slot.GridPosition.X, slot.GridPosition.Y] = slot;
                _slotsCount++;
            }

            OnItemPut(new EntityContainerEventArgs<T> { Slot = slot });
            return true;
        }

        /// <summary>
        /// Tries to get item from slot. Checks the Entity type 
        /// </summary>
        /// <param name="slot"></param>
        /// <returns>True if succeed otherwise false</returns>
        public bool TakeItem(T slot)
        {
            ValidatePosition(slot.GridPosition);

            var currentItem = _items[slot.GridPosition.X, slot.GridPosition.Y];

            // unable to take items from empty slot
            if (currentItem == null) return false;

            // unable to take items of other types
            if (currentItem.Item.StackType != slot.Item.StackType)
                return false;

            // unable to take more items than container have
            if (currentItem.ItemsCount < slot.ItemsCount)
                return false;

            currentItem.ItemsCount -= slot.ItemsCount;

            if (currentItem.ItemsCount == 0)
            {
                // no more items in slot
                _items[slot.GridPosition.X, slot.GridPosition.Y] = null;
                _slotsCount--;
            }

            OnItemTaken(new EntityContainerEventArgs<T> { Slot = slot });
            return true;
        }

        /// <summary>
        /// Tries to get item from slot. Slot entity will be filled from slot position
        /// </summary>
        /// <param name="slot"></param>
        /// <returns>slot parameter, same reference but changed !</returns>
        public T TakeSlot(T slot)
        {
            ValidatePosition(slot.GridPosition);

            var currentItem = _items[slot.GridPosition.X, slot.GridPosition.Y];

            // unable to take items from empty slot
            if (currentItem == null) return null;

            // unable to take more items than container have
            if (currentItem.ItemsCount < slot.ItemsCount)
                return null;

            currentItem.ItemsCount -= slot.ItemsCount;
            slot.Item = currentItem.Item;

            if (currentItem.ItemsCount == 0)
            {
                // no more items in slot
                _items[slot.GridPosition.X, slot.GridPosition.Y] = null;
                _slotsCount--;
            }

            OnItemTaken(new EntityContainerEventArgs<T> { Slot = slot });

            return slot;
        }

        /// <summary>
        /// Returns slot without taking it from the container
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public T PeekSlot(Vector2I pos)
        {
            ValidatePosition(pos);
            return _items[pos.X, pos.Y];
        }

        /// <summary>
        /// Puts the item to already occupied slot (Items should have different type)
        /// </summary>
        /// <param name="slotToPut">to put : what's in your hand before exchanging </param>
        /// <param name="slotTaken">result of the itemexchange = you get this in you hand</param>
        /// <returns></returns>
        public bool PutItemExchange(T slotToPut, out T slotTaken)
        {
            slotTaken = null;

            if (slotToPut == null || slotToPut.Item == null)
                return false;

            ValidatePosition(slotToPut.GridPosition);
            
            var currentItem = _items[slotToPut.GridPosition.X, slotToPut.GridPosition.Y];

            if (currentItem == null || currentItem.Item.StackType == slotToPut.Item.StackType)
                return false;

            slotTaken = currentItem;

            _items[slotToPut.GridPosition.X, slotToPut.GridPosition.Y] = slotToPut;
            OnItemExchanged(new EntityContainerEventArgs<T> { Slot = slotToPut, Exchanged = slotTaken });
            return true;
        }

        /// <summary>
        /// Allows to enumerate slots in the container
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int x = 0; x < _gridSize.X; x++)
            {
                for (int y = 0; y < _gridSize.Y; y++)
                {
                    if(_items[x,y] != null)
                        yield return _items[x, y];
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Checks if entity inside
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Contains(Entity entity)
        {
            for (int x = 0; x < _gridSize.X; x++)
            {
                for (int y = 0; y < _gridSize.Y; y++)
                {
                    if (_items[x, y] != null)
                    {
                        if (_items[x, y].Item.EntityId == entity.EntityId)
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Performs search for entity and returns slot
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Find(Entity entity)
        {
            for (int x = 0; x < _gridSize.X; x++)
            {
                for (int y = 0; y < _gridSize.Y; y++)
                {
                    if (_items[x, y] != null)
                    {
                        if (_items[x, y].Item.EntityId == entity.EntityId)
                            return _items[x, y];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Drop 'from' on 'to' . Switches 'from' and 'to' when items are different, adds to the to stacks when items are stackable 
        /// //TODO check performance when moving stacks of items. maybe replace by a proper switch slots message
        /// </summary>
        /// <param name="taken"></param>
        /// <param name="to">Destination slot</param>
        public T DropOn(ref T taken, T to)
        {
            IItem itemFrom = taken.Item;
            IItem itemTo = to.Item;

            T hand;

            if (itemTo==null)
            {
                to.Item = taken.Item; 
                PutItem(to);   //not the same thing as putItem(itemfrom) , i want to put on this specific slot not in the next available one
                //TODO handle case when stack is full                
                //TODO move a full stack    
                hand = null;
            }
            else if (itemTo.MaxStackSize > 1 && itemFrom.StackType == itemTo.StackType)
            {
                //de-stack on from, stack on to                 
                PutItem(to);//TODO handle case when stack is full 
                hand = null;
            } 
            else
            {
                T slotPut = taken;
                slotPut.GridPosition = to.GridPosition;              
                PutItemExchange(slotPut, out hand);
            }

            return hand;
        }
    }
}
