using System;
using System.Collections.Generic;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// Represents a base container implementation (this is not an entity)
    /// </summary>
    public class BaseContainer : IEntityContainer, IBinaryStorable
    {
        /// <summary>
        /// Occurs when the item was taken from the container
        /// </summary>
        public event EventHandler<EntityContainerEventArgs> ItemTaken;

        public void OnItemTaken(EntityContainerEventArgs e)
        {
            EventHandler<EntityContainerEventArgs> handler = ItemTaken;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when the item was put into the container
        /// </summary>
        public event EventHandler<EntityContainerEventArgs> ItemPut;

        public void OnItemPut(EntityContainerEventArgs e)
        {
            EventHandler<EntityContainerEventArgs> handler = ItemPut;
            if (handler != null) handler(this, e);
        }

        private ContainedSlot[,] _items;
        private Location2<byte> _gridSize;
        private int _slotsCount;

        public BaseContainer()
        {
            GridSize = new Location2<byte>(8, 5);
        }

        /// <summary>
        /// Gets maximum container capacity
        /// </summary>
        public int Capacity { get; set; }
        
        /// <summary>
        /// Gets container grid size
        /// </summary>
        public Location2<byte> GridSize
        {
            get { return _gridSize; }
            set { 
                _gridSize = value;
                //todo: copy of items to new container from old
                _items = new ContainedSlot[_gridSize.X, _gridSize.Z];
            }
        }


        public void Save(System.IO.BinaryWriter writer)
        {
            // we need to save items count to be able to load again
            writer.Write(_slotsCount);

            // writing grid size
            writer.Write(_gridSize.X);
            writer.Write(_gridSize.Z);

            // saving containing items
            foreach (var slot in this)
            {
                slot.Save(writer);
            }
        }

        public void Load(System.IO.BinaryReader reader)
        {
            // read contained entites count
            _slotsCount = reader.ReadInt32();

            // read container grid size
            Location2<byte> gridSize;
            gridSize.X = reader.ReadByte();
            gridSize.Z = reader.ReadByte();
            _gridSize = gridSize;

            // load contained slots (slot is count and entity example)
            for (int i = 0; i < _slotsCount; i++)
            {
                var containedSlot = new ContainedSlot();

                containedSlot.Load(reader);
                _items[containedSlot.GridPosition.X, containedSlot.GridPosition.Z] = containedSlot;
            }
        }

        private void ValidatePosition(Location2<byte> position)
        {
            if (position.X < 0 || position.Z < 0 || position.X >= _gridSize.X || position.Z >= _gridSize.Z)
                throw new ArgumentException("Slot position is unacceptable for this container");
        }

        /// <summary>
        /// Tries to put item into slot specified
        /// </summary>
        /// <param name="slot"></param>
        /// <returns>True if succeed otherwise false</returns>
        public bool PutItem(ContainedSlot slot)
        {
            ValidatePosition(slot.GridPosition);

            var currentItem = _items[slot.GridPosition.X, slot.GridPosition.Z];

            
            if (currentItem != null)
            {
                // check if slot is busy by other entity (different entities are unstackable)
                if (currentItem.Item.GetType() != slot.Item.GetType())
                    return false;

                // check for stack limit
                if (currentItem.ItemsCount + slot.ItemsCount > slot.Item.MaxStackSize)
                    return false;

                currentItem.ItemsCount += slot.ItemsCount;
            }
            else
            {
                // adding new slot
                _items[slot.GridPosition.X, slot.GridPosition.Z] = slot;
                _slotsCount++;
            }

            OnItemPut(new EntityContainerEventArgs { Slot = slot });
            return true;
        }

        /// <summary>
        /// Tries to get item from slot
        /// </summary>
        /// <param name="slot"></param>
        /// <returns>True if succeed otherwise false</returns>
        public bool GetItem(ContainedSlot slot)
        {
            ValidatePosition(slot.GridPosition);

            var currentItem = _items[slot.GridPosition.X, slot.GridPosition.Z];

            // unable to take items from empty slot
            if (currentItem == null) return false;

            // unable to take items of other types
            if (currentItem.Item.GetType() != slot.Item.GetType())
                return false;

            // unable to take more items than container have
            if (currentItem.ItemsCount < slot.ItemsCount)
                return false;

            currentItem.ItemsCount -= slot.ItemsCount;

            if (currentItem.ItemsCount == 0)
            {
                // no more items in slot
                _items[slot.GridPosition.X, slot.GridPosition.Z] = null;
                _slotsCount--;
            }

            OnItemTaken(new EntityContainerEventArgs { Slot = slot });
            return true;
        }

        /// <summary>
        /// Allows to enumerate slots in the container
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ContainedSlot> GetEnumerator()
        {
            for (int x = 0; x < _gridSize.X; x++)
            {
                for (int y = 0; y < _gridSize.Z; y++)
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
    }
}
