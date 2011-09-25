using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Inventory;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message to inform about entity equipment change
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct EntityEquipmentMessage : IBinaryMessage
    {
        private EquipmentSlotType[] _slots;
        private Entity[] _items;

        /// <summary>
        /// Array of slots item. This array must have the same size and order as Items
        /// </summary>
        public EquipmentSlotType[] Slots
        {
            get { return _slots; }
            set { _slots = value; }
        }
        
        /// <summary>
        /// Array of items are equipped. This array must have the same size and order as Slots
        /// </summary>
        public Entity[] Items
        {
            get { return _items; }
            set { _items = value; }
        }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityEquipment; }
        }

        public static EntityEquipmentMessage Read(BinaryReader reader)
        {
            EntityEquipmentMessage msg;
            var count = reader.ReadInt32();

            msg._slots = new EquipmentSlotType[count];
            msg._items = new Item[count];

            for (int i = 0; i < count; i++)
            {
                msg._slots[i] = (EquipmentSlotType)reader.ReadUInt16();
                msg._items[i] = EntityFactory.Instance.CreateFromBytes(reader);
            }

            return msg;
        }

        /// <summary>
        /// Writes all necessary instance members
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(_slots.Length);

            for (int i = 0; i < _slots.Length; i++)
            {
                writer.Write((ushort)_slots[i]);
                _items[i].Save(writer);
            }
        }
    }
}
