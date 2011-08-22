using System;
using System.IO;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// Represents character equipment
    /// </summary>
    public class CharacterEquipment : IBinaryStorable
    {
        /// <summary>
        /// Occurs when the character wears something
        /// </summary>
        public event EventHandler<CharacterEqipmentEventArgs> ItemEquipped;

        /// <summary>
        /// Invokes ItemEquipped event
        /// </summary>
        /// <param name="e"></param>
        public void OnItemEquipped(CharacterEqipmentEventArgs e)
        {
            EventHandler<CharacterEqipmentEventArgs> handler = ItemEquipped;
            if (handler != null) handler(this, e);
        }

        private Armor _headGear;
        private Armor _torso;
        private Armor _arms;
        private Armor _legs;
        private Armor _feet;
        private Item _leftRing;
        private Item _rightRing;
        private Item _neckLace;
        private Tool _leftTool;
        private Tool _rightTool;

        /// <summary>
        /// Wear some item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="slot"></param>
        public void WearItem(Item item, EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Head:
                    HeadGear = item as Armor;
                    break;
                case EquipmentSlot.Torso:
                    Torso = item as Armor;
                    break;
                case EquipmentSlot.Legs:
                    Legs = item as Armor;
                    break;
                case EquipmentSlot.Feet:
                    Feet = item as Armor;
                    break;
                case EquipmentSlot.Arms:
                    Arms = item as Armor;
                    break;
                case EquipmentSlot.LeftHand:
                    LeftTool = item as Tool;
                    break;
                case EquipmentSlot.RightHand:
                    RightTool = item as Tool;
                    break;
                case EquipmentSlot.LeftRing:
                    LeftRing = item;
                    break;
                case EquipmentSlot.RightRing:
                    RightRing = item;
                    break;
                case EquipmentSlot.Bags:
                    break;
                case EquipmentSlot.Neck:
                    NeckLace = item;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("slot");
            }
        }

        public Armor HeadGear
        {
            get { return _headGear; }
            set
            {
                _headGear = value;
                OnItemEquipped(new CharacterEqipmentEventArgs {Item = value, Slot = EquipmentSlot.Head});
            }
        }
        
        public Armor Torso
        {
            get { return _torso; }
            set
            {
                _torso = value;
                OnItemEquipped(new CharacterEqipmentEventArgs {Item = value, Slot = EquipmentSlot.Torso});
            }
        }
        
        public Armor Arms
        {
            get { return _arms; }
            set
            {
                _arms = value;
                OnItemEquipped(new CharacterEqipmentEventArgs {Item = value, Slot = EquipmentSlot.Arms});
            }
        }
        
        public Armor Legs
        {
            get { return _legs; }
            set
            {
                _legs = value;
                OnItemEquipped(new CharacterEqipmentEventArgs {Item = value, Slot = EquipmentSlot.Legs});
            }
        }
        
        public Armor Feet
        {
            get { return _feet; }
            set
            {
                _feet = value;
                OnItemEquipped(new CharacterEqipmentEventArgs {Item = value, Slot = EquipmentSlot.Feet});
            }
        }
        
        public Item LeftRing
        {
            get { return _leftRing; }
            set
            {
                _leftRing = value;
                OnItemEquipped(new CharacterEqipmentEventArgs {Item = value, Slot = EquipmentSlot.LeftRing});
            }
        }
        
        public Item RightRing
        {
            get { return _rightRing; }
            set
            {
                _rightRing = value;
                OnItemEquipped(new CharacterEqipmentEventArgs {Item = value, Slot = EquipmentSlot.RightRing});
            }
        }
        
        public Item NeckLace
        {
            get { return _neckLace; }
            set
            {
                _neckLace = value;
                OnItemEquipped(new CharacterEqipmentEventArgs {Item = value, Slot = EquipmentSlot.Neck});
            }
        }
        
        public Tool LeftTool
        {
            get { return _leftTool; }
            set
            {
                _leftTool = value;
                OnItemEquipped(new CharacterEqipmentEventArgs {Item = value, Slot = EquipmentSlot.LeftHand});
            }
        }
        
        public Tool RightTool
        {
            get { return _rightTool; }
            set
            {
                _rightTool = value;
                OnItemEquipped(new CharacterEqipmentEventArgs {Item = value, Slot = EquipmentSlot.RightHand});
            }
        }

        private void SaveItem(IBinaryStorable entity, BinaryWriter writer)
        {
            if(entity != null)
                entity.Save(writer);
            else
            {
                // write entity
                NoEntity.SaveEmpty(writer);
            }
        }

        private T LoadItem<T>(BinaryReader reader) where T: class, IBinaryStorable
        {
            var item = EntityFactory.Instance.CreateFromBytes(reader);

            if (item is NoEntity)
                return null;

            return item as T;
        }

        /// <summary>
        /// Saves character equipment
        /// </summary>
        /// <param name="writer"></param>
        public void Save(BinaryWriter writer)
        {
            SaveItem(HeadGear, writer);
            SaveItem(Torso, writer);
            SaveItem(Arms, writer);
            SaveItem(Legs, writer);
            SaveItem(Feet, writer);
            SaveItem(LeftRing, writer);
            SaveItem(RightRing, writer);
            SaveItem(NeckLace, writer);
            SaveItem(LeftTool, writer);
            SaveItem(RightTool, writer);
        }

        /// <summary>
        /// Loads character equipment
        /// </summary>
        /// <param name="reader"></param>
        public void Load(BinaryReader reader)
        {
            HeadGear = LoadItem<Armor>(reader);
            Torso = LoadItem<Armor>(reader);
            Arms = LoadItem<Armor>(reader);
            Legs = LoadItem<Armor>(reader);
            Feet = LoadItem<Armor>(reader);

            LeftRing = LoadItem<Item>(reader);
            RightRing = LoadItem<Item>(reader);
            NeckLace = LoadItem<Item>(reader);

            LeftTool = LoadItem<Tool>(reader);
            RightTool = LoadItem<Tool>(reader);
        }
    }

    public class CharacterEqipmentEventArgs : EventArgs
    {
        public Item Item { get; set; }
        public EquipmentSlot Slot { get; set; }
    }
}
