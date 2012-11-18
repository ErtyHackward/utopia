using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents a base container implementation (this is not an entity)
    /// </summary>
    public class SlotContainer<T> : ISlotContainer<T>, IStaticContainer where T: ContainedSlot, new()
    {
        private readonly IEntity _parentEntity;
        private T[,] _items;
        private Vector2I _gridSize;
        private int _slotsCount;
        private uint _maxId;
        private List<SlotContainer<T>> _joinedScope;

        /// <summary>
        /// Gets container grid size
        /// </summary>
        public Vector2I GridSize
        {
            get { return _gridSize; }
            set
            {
                _gridSize = value;
                //todo: copy of items to new container from old
                _items = new T[_gridSize.X, _gridSize.Y];
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
        public SlotContainer(IEntity parentEntity , Vector2I containerGridSize)
        {
            _parentEntity = parentEntity;
            GridSize = containerGridSize;
        }

        /// <summary>
        /// Creates new instance of container with GridSize of 8x5 items
        /// </summary>
        public SlotContainer(IEntity parentEntity = null)
            : this(parentEntity, new Vector2I(6, 5))
        {
            
        }
        
        public void Save(BinaryWriter writer)
        {
            // we need to save items count to be able to load again
            writer.Write(_slotsCount);

            // writing grid size
            writer.Write(_gridSize);

            // write max id
            writer.Write(_maxId);

            // saving containing items
            foreach (var slot in this)
            {
                slot.Save(writer);
            }
        }

        public void Load(BinaryReader reader, EntityFactory factory)
        {
            // read contained slots count
            _slotsCount = reader.ReadInt32();

            // read container grid size
            _gridSize = reader.ReadVector2I();

            // read max id
            _maxId = reader.ReadUInt32();

            // load contained slots (slot is count and entity example)
            for (int i = 0; i < _slotsCount; i++)
            {
                var containedSlot = new T();

                containedSlot.LoadSlot(reader, factory);
                _items[containedSlot.GridPosition.X, containedSlot.GridPosition.Y] = containedSlot;
            }
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
        /// Makes id valid for current container scope
        /// </summary>
        /// <param name="item"></param>
        protected void ValidateId(IItem item)
        {
            if (item.StaticId == 0 || Find(item.StaticId) != null)
                item.StaticId = GetFreeId();
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
        /// Determines if item can be put or exchanged to slot 
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
                                       if (!ValidateItem(item, s.GridPosition))
                                           return false;

                                       s.ItemsCount += count;

                                       var t = new T {
                                           GridPosition = s.GridPosition,
                                           ItemsCount = count,
                                           Item = s.Item
                                       };

                                       OnItemPut(new EntityContainerEventArgs<T> { Slot = t });
                                       return true;
                                   }
                                   return false;
                               });

                if (e) return true;

            }

            // take first free cell and put the item
            for (int x = 0; x < _gridSize.X; x++)
            {
                for (int y = 0; y < _gridSize.Y; y++)
                {
                    if (_items[x, y] == null)
                    {
                        var pos = new Vector2I(x, y);

                        if (!ValidateItem(item, pos))
                            continue;

                        var addSlot = new T { Item = item, GridPosition = pos, ItemsCount = count };
                        ValidateId(item);
                        _items[x, y] = addSlot;
                        
                        _slotsCount ++;
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
                // adding new slot
                var addSlot = new T { Item = item, GridPosition = position, ItemsCount = itemsCount };
                ValidateId(item);
                _items[position.X, position.Y] = addSlot;
                _slotsCount++;
                item.Container = this;
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
            if (currentItem == null) return false;

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
            if(slot != null)
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

            if (currentItem == null || currentItem.Item.StackType == item.StackType)
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
        public void Add(IStaticEntity entity)
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
    }
}
