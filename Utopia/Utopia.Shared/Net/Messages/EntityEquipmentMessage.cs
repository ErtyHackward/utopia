using System;
using System.IO;
using System.Runtime.InteropServices;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message to inform about entity equipment change
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct EntityEquipmentMessage : IBinaryMessage
    {
        private EquipmentItem[] _items;
        
        /// <summary>
        /// Array of items are equipped.
        /// </summary>
        public EquipmentItem[] Items
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

            msg._items = new EquipmentItem[count];

            for (int i = 0; i < count; i++)
            {
                msg._items[i] = new EquipmentItem((EquipmentSlotType)reader.ReadUInt16(), EntityFactory.Instance.CreateFromBytes(reader));
            }

            return msg;
        }

        /// <summary>
        /// Writes all necessary instance members
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(_items.Length);

            foreach (var t in _items)
            {
                writer.Write((ushort)t.Slot);
                t.Entity.Save(writer);
            }
        }
    }

    public struct EquipmentItem
    {
        private Entity _entity;
        private EquipmentSlotType _slot;

        public EquipmentItem(EquipmentSlotType slot, Entity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            _slot = slot;
            _entity = entity;
        }
        
        public EquipmentSlotType Slot
        {
            get { return _slot; }
            set { _slot = value; }
        }
        
        public Entity Entity
        {
            get { return _entity; }
            set { _entity = value; }
        }
    }
}
