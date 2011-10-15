using System.IO;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Each slot has an entity and number of entities count.
    /// </summary>
    public class Slot : IBinaryStorable
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
        public bool IsEmpty { get { return ItemsCount == 0; } }

        public virtual void Save(BinaryWriter writer)
        {
            writer.Write(ItemsCount);

            if (!IsEmpty)
            {
                Item.Save(writer);
            }
        }

        public virtual void Load(BinaryReader reader)
        {
            ItemsCount = reader.ReadInt32();

            if (!IsEmpty)
            {
                Item = (IItem)EntityFactory.Instance.CreateFromBytes(reader);
            }
        }

        /// <summary>
        /// Allows to write empty slot
        /// </summary>
        /// <param name="writer"></param>
        public static void WriteEmpty(BinaryWriter writer)
        {
            writer.Write(0);
        }
    }
}