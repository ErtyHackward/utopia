using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents a base container implementation (this is not an entity)
    /// </summary>
    [ProtoContract]
    public class SlotContainer<T> : ISlotContainer<T> where T: ContainedSlot, new()
    {
        private readonly IEntity _parentEntity;
        private T[,] _items;
        private Vector2I _gridSize;
        private int _slotsCount;
        private uint _maxId;
        private List<SlotContainer<T>> _joinedScope;

        /// <summary>
        /// Don't use, serialize only
        /// </summary>
        [ProtoMember(1)]
        public int SerializeSlotsCount
        {
            get { return _slotsCount; }
            set { _slotsCount = value; }
        }

        /// <summary>
        /// Gets container grid size
        /// </summary>
        [ProtoMember(2)]
        public Vector2I GridSize
        {
            get { return _gridSize; }
            set
            {
                var prevSize = _gridSize;

                _gridSize = value;

                var newSlots = new T[_gridSize.X, _gridSize.Y];

                if (_items != null)
                {
                    var copySize = Vector2I.Min(prevSize, _gridSize);
                    var range = new Range2I(Vector2I.Zero, copySize);

                    foreach (var pos in range)
                    {
                        newSlots[pos.X, pos.Y] = _items[pos.X, pos.Y];
                    }
                }

                _items = newSlots;
            }
        }

        /// <summary>
        /// Don't use, serialize only
        /// </summary>
        [ProtoMember(3)]
        public uint SerializeMaxId
        {
            get { return _maxId; }
            set { _maxId = value; }
        }

        /// <summary>
        /// Don't use, serialize only
        /// </summary>
        [ProtoMember(4, OverwriteList = true)]
        public List<T> SerializeItems
        {
            get
            {
                var list = this.ToList();
                return list; 
            }
            set
            {
                _items = new T[_gridSize.X, _gridSize.Y];

                foreach (var slot in value)
                {
                    _items[slot.GridPosition.X, slot.GridPosition.Y] = slot;
                }
            }
        }

        /// <summary>
        /// Gets parent entity
        /// </summary>
        public IEntity Parent
        {
            get { return _parentEntity; }
        }

        /// <summary>
        /// Occurs when the item was taken from the container
        /// </summary>
        public event EventHandler<EntityContainerEventArgs<T>> ItemTaken;

        protected virtual void OnItemTaken(EntityContainerEventArgs<T> e)
        {
            var handler = ItemTaken;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when the item was put into the container
        /// </summary>
        public event EventHandler<EntityContainerEventArgs<T>> ItemPut;

        protected virtual void OnItemPut(EntityContainerEventArgs<T> e)
        {
            var handler = ItemPut;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when item was put to non-empty slot
        /// </summary>
        public event EventHandler<EntityContainerEventArgs<T>> ItemExchanged;

        protected virtual void OnItemExchanged(EntityContainerEventArgs<T> e)
        {
            var handler = ItemExchanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Creates new instance of container with gridSize specified
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <param name="containerGridSize"></param>
        public SlotContainer(IEntity parentEntity, Vector2I containerGridSize)
        {
            _parentEntity = parentEntity;
            GridSize = containerGridSize;
        }

        /// <summary>
        /// Creates new instance of container with GridSize of 6x5 items
        /// </summary>
        public SlotContainer() :this(null)
        {
            
        }

        /// <summary>
        /// Creates new instance of container with GridSize of 6x5 items
        /// </summary>
        public SlotContainer(IEntity parentEntity)
            : this(parentEntity, new Vector2I(6, 5))
        {
            
        }

        /// <summary>
        /// Deletes all items from the container
        /// </summary>
        public void Clear()
        {
            _slotsCount = 0;
            _maxId = 0;
            for (int x = 0; x < _gridSize.X; x++)
            {
                for (int y = 0; y < _gridSize.Y; y++)
                {
                    _items[x, y] = null;
                }
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
        /// Allows to perform item type validation
        /// </summary>
        /// <param name="item"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        protected virtual bool ValidateItem(IItem item, Vector2I position)
        {
            return true;
        }

        /// <summary>
        /// Makes id valid for current container scope, if needed creates new item instance
        /// </summary>
        /// <param name="item"></param>
        protected void ValidateId(ref IItem item)
        {
            var currentItem = Find(item.StaticId);

            if (currentItem != null && currentItem.Item == item)
            {
                // we need to duplicate the item to avoid staticId collision
                item = (Item)item.Clone();
                item.StaticId = GetFreeId();
                return;
            }

            if (item.StaticId == 0 || Find(item.StaticId) != null)
                item.StaticId = GetFreeId();
            else
                _maxId = Math.Max(_maxId, item.StaticId);
        }

        /// <summary>
        /// Joins two containers to have single id scope
        /// </summary>
        /// <param name="otherContainer"></param>
        public void JoinIdScope(SlotContainer<T> otherContainer)
        {
            if (otherContainer == null) throw new ArgumentNullException("otherContainer");
            if (otherContainer == this) throw new ArgumentException("Unable to join id scope with myself");

            if (_joinedScope == null)
                _joinedScope = new List<SlotContainer<T>>();

            _joinedScope.Add(otherContainer);
        }

        /// <summary>
        /// Determines if item can be put or stacked to slot 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="position"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool CanPut(IItem item, Vector2I position, int count = 1)
        {
            ValidatePosition(position);

            if (!ValidateItem(item, position))
                return false;

            var slot = _items[position.X, position.Y];

            var newSlot = new T { Item = item, ItemsCount = count };
            
            return slot == null || slot.CanStackWith(newSlot);
        }

        public bool CanPut(IItem item, int count = 1)
        {
            return FindSlotFor(item, count) != null;
        }

        private T FindSlotFor(IItem item, int count = 1)
        {
            if (item.MaxStackSize > 1)
            {
                var slot = this.FirstOrDefault(s => s.Item.StackType == item.StackType && s.ItemsCount + count <= item.MaxStackSize && ValidateItem(item, s.GridPosition));

                if (slot != null)
                    return slot;
            }

            for (int x = 0; x < _gridSize.X; x++)
            {
                for (int y = 0; y < _gridSize.Y; y++)
                {
                    if (_items[x, y] == null)
                    {
                        var pos = new Vector2I(x, y);

                        if (!ValidateItem(item, pos))
                            continue;

                        var slot = new T { 
                            GridPosition = pos
                        };

                        return slot;
                    }

                }
            }

            return null;
        }

        /// <summary>
        /// Tries to put item into the inventory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="count"></param>
        /// <returns>True if item put into the inventory otherwise false</returns>
        public bool PutItem(IItem item, int count = 1)
        {
            if (count == 0)
                return true;

            // inventory is full?
            if (item.MaxStackSize == 1 && _slotsCount == _gridSize.X * _gridSize.Y)
                return false;

            var slot = FindSlotFor(item, count);

            if (slot == null)
                return false;

            if (slot.ItemsCount == 0)
            {
                // new slot
                _slotsCount++;
                ValidateId(ref item);
                slot.Item = item;
                slot.ItemsCount = count;
                _items[slot.GridPosition.X, slot.GridPosition.Y] = slot;
            }
            else
            {
                // stacking
                _items[slot.GridPosition.X, slot.GridPosition.Y].ItemsCount += count;
            }

            slot = (T)slot.Clone();
            slot.ItemsCount = count;

            OnItemPut(new EntityContainerEventArgs<T> { Slot = slot });
            return true;
        }

        /// <summary>
        /// Tries to add an item into slot
        /// if slot has already an item, add to the stack
        /// if slot is empty, initialize it with slot.item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="position"></param>
        /// <param name="itemsCount"></param>
        /// <returns>True if succeed otherwise false</returns>
        public bool PutItem(IItem item, Vector2I position, int itemsCount = 1)
        {
            ValidatePosition(position);

            if (itemsCount == 0)
                throw new InvalidOperationException("No items to put");

            if (!ValidateItem(item, position))
                return false;

            var currentItem = _items[position.X, position.Y];
            
            if (currentItem != null)
            {
                // check if slot is busy by other entity (different entities are unstackable)
                if (currentItem.Item.StackType != item.StackType)
                    return false;

                // check for stack limit
                if (currentItem.ItemsCount + itemsCount > item.MaxStackSize)
                    return false;

                currentItem.ItemsCount += itemsCount;
            }
            else
            {
                ValidateId(ref  item);

                // adding new slot
                var addSlot = new T { Item = item, GridPosition = position, ItemsCount = itemsCount };
                _items[position.X, position.Y] = addSlot;
                _slotsCount++;
            }

            OnItemPut(new EntityContainerEventArgs<T> { Slot = new T{ Item = item, GridPosition = position, ItemsCount = itemsCount } });
            return true;
        }

        /// <summary>
        /// Tries to get item from slot.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="itemsCount"></param>
        /// <returns>True if succeed otherwise false</returns>
        public bool TakeItem(Vector2I position, int itemsCount = 1)
        {
            ValidatePosition(position);

            var currentItem = _items[position.X, position.Y];

            // unable to take items from empty slot
            if (currentItem == null) 
                return false;

            // unable to take more items than container have
            if (currentItem.ItemsCount < itemsCount)
                return false;

            currentItem.ItemsCount -= itemsCount;

            if (currentItem.ItemsCount == 0)
            {
                // no more items in slot
                _items[position.X, position.Y] = null;
                _slotsCount--;

                if (_slotsCount == 0)
                    _maxId = 0;
            }

            OnItemTaken(new EntityContainerEventArgs<T> { Slot = new T { GridPosition = position, ItemsCount = itemsCount, Item = currentItem.Item } });
            return true;
        }

        /// <summary>
        /// Returns slot without taking it from the container
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>slot object or null</returns>
        public T PeekSlot(Vector2I pos)
        {
            ValidatePosition(pos);

            var slot = _items[pos.X, pos.Y];
            if (slot != null)
                return (T)slot.Clone();
            return null;
        }

        /// <summary>
        /// Updates specified slot, should be used in scripts and editor
        /// </summary>
        /// <param name="slot"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void WriteSlot(T slot)
        {
            if (slot == null) throw new ArgumentNullException("slot");

            var pos = slot.GridPosition;
            ValidatePosition(pos);

            if (_items[pos.X, pos.Y] == null)
                _slotsCount++;

            _items[pos.X, pos.Y] = slot;
        }

        /// <summary>
        /// Removes all entities from position specified
        /// </summary>
        /// <param name="pos"></param>
        public void ClearSlot(Vector2I pos)
        {
            ValidatePosition(pos);

            if (_items[pos.X, pos.Y] == null)
                return;

            _items[pos.X, pos.Y] = null;
            _slotsCount--;

            if (_slotsCount == 0)
                _maxId = 0;

        }

        /// <summary>
        /// Puts the item to already occupied slot (Items should have different type)
        /// </summary>
        /// <param name="itemsCount"></param>
        /// <param name="slotTaken"></param>
        /// <param name="item"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool PutItemExchange(IItem item, Vector2I position, int itemsCount, out T slotTaken)
        {
            slotTaken = null;

            if (item == null)
                return false;
            
            ValidatePosition(position);

            if (!ValidateItem(item, position))
                return false;

            var currentItem = _items[position.X, position.Y];

            if (currentItem == null || currentItem.CanStackWith(item, itemsCount))
                return false;

            slotTaken = (T)currentItem.Clone();

            var put = new T { 
                Item = item, 
                GridPosition = position, 
                ItemsCount = itemsCount 
            };

            if (!IsIdFree(item.StaticId))
                item.StaticId = GetFreeId();

            _items[position.X, position.Y] = put;
            OnItemExchanged(new EntityContainerEventArgs<T> { Slot = put, Exchanged = slotTaken });
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
                        yield return (T)_items[x, y].Clone();
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
        public bool Contains(IStaticEntity entity)
        {
            for (int x = 0; x < _gridSize.X; x++)
            {
                for (int y = 0; y < _gridSize.Y; y++)
                {
                    if (_items[x, y] != null)
                    {
                        if (_items[x, y].Item.StaticId == entity.StaticId)
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Performs search for an entity and returns a slot
        /// </summary>
        /// <param name="staticEntityid">static entity id</param>
        /// <returns></returns>
        public T Find(uint staticEntityid)
        {
            for (int x = 0; x < _gridSize.X; x++)
            {
                for (int y = 0; y < _gridSize.Y; y++)
                {
                    if (_items[x, y] != null)
                    {
                        if (_items[x, y].Item.StaticId == staticEntityid)
                            return _items[x, y];
                    }
                }
            }
            return null;
        }
        
        /// <summary>
        /// This method is not supported. Use PutItem instead.
        /// </summary>
        /// <param name="entity"></param>
        public void Add_(IStaticEntity entity)
        {
            throw new NotSupportedException("This method is not supported. Use PutItem instead.");
        }

        /// <summary>
        /// This method is not supported. Use TakeItem instead.
        /// </summary>
        /// <param name="entity"></param>
        public void Remove(IStaticEntity entity)
        {
            throw new NotSupportedException("This method is not supported. Use TakeItem instead.");
        }

        /// <summary>
        /// Gets entity by its static id
        /// </summary>
        /// <param name="staticId"></param>
        /// <returns></returns>
        public IStaticEntity GetStaticEntity(uint staticId)
        {
            var slot = Find(staticId);
            if (slot != null)
                return slot.Item;
            return null;
        }


        /// <summary>
        /// Returns free unique number for this collection (and all joined collections)
        /// </summary>
        /// <returns></returns>
        public uint GetFreeId()
        {
            if (_joinedScope != null)
            {
                var otherMax = _joinedScope.Max(c => c._maxId);

                _maxId = Math.Max(_maxId, otherMax);
            }

            return ++_maxId;
        }

        private bool IsIdFree(uint staticId)
        {
            // id 0 is not allowed
            if (staticId == 0) return false;

            // check linked containers
            if (_joinedScope != null)
            {
                if (_joinedScope.Exists(c => c.Find(staticId) != null))
                    return false;
            }

            // check our container
            return Find(staticId) == null;
        }

        /// <summary>
        /// Allows to put many items at once or nothing (transaction way)
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public bool PutMany(IEnumerable<KeyValuePair<IItem, int>> items)
        {
            var put = new List<KeyValuePair<IItem, int>>();
            bool success = true;
            foreach (var keyValuePair in items)
            {
                if (!PutItem(keyValuePair.Key, keyValuePair.Value))
                {
                    success = false;
                    break;
                }
                put.Add(keyValuePair);
            }

            if (!success)
            {
                foreach (var keyValuePair in put)
                {
                    TakeItem(keyValuePair.Key.BluePrintId, keyValuePair.Value);
                }
            }

            return success;
        }

        internal bool TakeItem(ushort blueprintId, int count)
        {
            while (count > 0)
            {
                var slot = this.LastOrDefault(s => s.Item.BluePrintId == blueprintId);

                if (slot == null)
                    break;

                var takeItems = Math.Min(slot.ItemsCount, count);

                TakeItem(slot.GridPosition, takeItems);

                count -= takeItems;
            }

            return count == 0;
        }
    }
}
