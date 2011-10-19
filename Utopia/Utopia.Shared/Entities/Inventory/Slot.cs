using System;
using System.IO;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Each slot has an entity and number of entities count.
    /// </summary>
    public class Slot : IBinaryStorable, ICloneable 
    {
        /// <summary>
        /// Gets or sets items count
        /// </summary>
        public int ItemsCount { get; set; }

        /// <summary>
        /// Gets or sets entity
        /// </summary>
        public IItem Item { get; set; }

        /// <summary>
        /// Indicates if slot is empty
        /// </summary>
        public bool IsEmpty { get { return Item == null || ItemsCount == 0; } }

        public virtual void Save(BinaryWriter writer)
        {
            writer.Write(ItemsCount);

            if (ItemsCount > 0)
            {
                Item.Save(writer);
            }
        }

        public virtual void Load(BinaryReader reader)
        {
            ItemsCount = reader.ReadInt32();
            
            if (ItemsCount > 0)
            {
                Item = (IItem)EntityFactory.Instance.CreateFromBytes(reader);
            }
            else Item = null;
        }

        /// <summary>
        /// Allows to write empty slot
        /// </summary>
        /// <param name="writer"></param>
        public static void WriteEmpty(BinaryWriter writer)
        {
            writer.Write(0);
        }

        public virtual object Clone()
        {
            var slot = new Slot {
                Item = Item, 
                ItemsCount = ItemsCount
            };
            return slot;
        }
    }
}