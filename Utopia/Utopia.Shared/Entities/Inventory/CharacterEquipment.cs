using System;
using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents character equipment
    /// </summary>
    [ProtoContract(IgnoreListHandling = true)]
    public class CharacterEquipment : SlotContainer<ContainedSlot>
    {
        /// <summary>
        /// Occurs when the character wears/unwears something
        /// </summary>
        public event EventHandler<CharacterEquipmentEventArgs> ItemEquipped;

        /// <summary>
        /// Invokes ItemEquipped event
        /// </summary>
        /// <param name="e"></param>
        public void OnItemEquipped(CharacterEquipmentEventArgs e)
        {
            e.Entity = (IDynamicEntity)Parent;
            var handler = ItemEquipped;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Creates new instance of character eqipment
        /// </summary>
        public CharacterEquipment(IDynamicEntity parent) : base(parent,new Vector2I(1, 10))
        {
            ItemPut += CharacterEquipmentItemPut;
            ItemTaken += CharacterEquipmentItemTaken;
            ItemExchanged += CharacterEquipmentItemExchanged;
        }
        
        void CharacterEquipmentItemExchanged(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = e.Slot, Slot = (EquipmentSlotType)e.Slot.GridPosition.Y, UnequippedItem = e.Exchanged });
        }

        void CharacterEquipmentItemTaken(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            var actualItem = PeekSlot(e.Slot.GridPosition);

            if (actualItem == null)
            {
                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = null, Slot = (EquipmentSlotType)e.Slot.GridPosition.Y });
            }
        }

        void CharacterEquipmentItemPut(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            var actualItem = PeekSlot(e.Slot.GridPosition);

            if (actualItem.ItemsCount == e.Slot.ItemsCount)
            {
                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = actualItem, Slot = (EquipmentSlotType)e.Slot.GridPosition.Y });
            }
        }

        /// <summary>
        /// Returns slot with currently equipped gear
        /// </summary>
        /// <param name="slotType"></param>
        /// <returns></returns>
        public ContainedSlot this[EquipmentSlotType slotType] 
        {
            get { return PeekSlot( new Vector2I(0, (int)slotType)); }
        }

        public bool Equip(EquipmentSlotType slotType, ContainedSlot slot, out ContainedSlot itemTaken)
        {
            itemTaken = null;

            var internalPosition = new Vector2I(0, (int)slotType);
            var actualSlot = PeekSlot(internalPosition);

            if (actualSlot == null || actualSlot.CanStackWith(slot))
            {
                return PutItem(slot.Item, internalPosition, slot.ItemsCount);
            }

            return PutItemExchange(slot.Item, internalPosition, slot.ItemsCount, out itemTaken);
        }

        public bool Unequip(EquipmentSlotType slotType, out ContainedSlot itemTaken)
        {
            itemTaken = null;

            var internalPosition = new Vector2I(0, (int)slotType);
            var actualSlot = PeekSlot(internalPosition);

            if (actualSlot == null)
                return false;

            itemTaken = actualSlot;

            return TakeItem(internalPosition, actualSlot.ItemsCount);
        }

        /// <summary>
        /// Return equiped tool (ITool) to the Right Hand
        /// </summary>
        public IItem RightTool
        {
            get
            {
                var tool = PeekSlot(new Vector2I(0, (int)EquipmentSlotType.Hand));
                if (tool != null)
                    return tool.Item;
                return null;
            }
        }

        protected override bool ValidateItem(IItem item, Vector2I position)
        {
            switch ((EquipmentSlotType)position.Y)
            {
                case EquipmentSlotType.Hand: return item is IItem;
                case EquipmentSlotType.Head: return item is IHeadArmor;
                case EquipmentSlotType.Torso: return item is ITorsoArmor;
                case EquipmentSlotType.Legs: return item is ILegsArmor;
                case EquipmentSlotType.Feet: return item is IFeetArmor;
                case EquipmentSlotType.Arms: return item is IArmsArmor;
                case EquipmentSlotType.Ring: return item is IRing;
                case EquipmentSlotType.Neck: return item is INecklace;
                default: return false;
            }
        }
    }

    public class CharacterEquipmentEventArgs : EventArgs
    {
        /// <summary>
        /// Parent dynamic entity object
        /// </summary>
        public IDynamicEntity Entity { get; set; }

        public ContainedSlot EquippedItem { get; set; }
        public ContainedSlot UnequippedItem { get; set; }

        /// <summary>
        /// Slot like "Left arm"
        /// </summary>
        public EquipmentSlotType Slot { get; set; }
    }
}
