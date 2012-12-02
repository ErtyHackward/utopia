using System;
using ProtoBuf;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message to inform about entity equipment change
    /// </summary>
    [ProtoContract]
    public struct EntityEquipmentMessage : IBinaryMessage
    {
        /// <summary>
        /// Array of items are equipped.
        /// </summary>
        [ProtoMember(1)]
        public EquipmentItem[] Items { get; set; }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityEquipment; }
        }
    }

    [ProtoContract]
    public struct EquipmentItem
    {
        private Entity _entity;
        private EquipmentSlotType _slot;

        [ProtoMember(1)]
        public EquipmentSlotType Slot
        {
            get { return _slot; }
            set { _slot = value; }
        }

        [ProtoMember(2)]
        public Entity Entity
        {
            get { return _entity; }
            set { _entity = value; }
        }

        public EquipmentItem(EquipmentSlotType slot, Entity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            _slot = slot;
            _entity = entity;
        }
    }
}
