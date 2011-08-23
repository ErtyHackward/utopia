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
        private readonly LivingEntity _parent;

        /// <summary>
        /// Occurs when the character wears something
        /// </summary>
        public event EventHandler<CharacterEquipmentEventArgs> ItemEquipped;

        /// <summary>
        /// Invokes ItemEquipped event
        /// </summary>
        /// <param name="e"></param>
        public void OnItemEquipped(CharacterEquipmentEventArgs e)
        {
            EventHandler<CharacterEquipmentEventArgs> handler = ItemEquipped;
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

        public CharacterEquipment(LivingEntity parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Wear some item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="slot"></param>
        public void WearItem(Item item, EquipmentSlotType slot)
        {
            switch (slot)
            {
                case EquipmentSlotType.Head:
                    HeadGear = item as Armor;
                    break;
                case EquipmentSlotType.Torso:
                    Torso = item as Armor;
                    break;
                case EquipmentSlotType.Legs:
                    Legs = item as Armor;
                    break;
                case EquipmentSlotType.Feet:
                    Feet = item as Armor;
                    break;
                case EquipmentSlotType.Arms:
                    Arms = item as Armor;
                    break;
                case EquipmentSlotType.LeftHand:
                    LeftTool = item as Tool;
                    break;
                case EquipmentSlotType.RightHand:
                    RightTool = item as Tool;
                    break;
                case EquipmentSlotType.LeftRing:
                    LeftRing = item;
                    break;
                case EquipmentSlotType.RightRing:
                    RightRing = item;
                    break;
                case EquipmentSlotType.Bags:
                    break;
                case EquipmentSlotType.Neck:
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
                OnItemEquipped(new CharacterEquipmentEventArgs {Item = value, Slot = EquipmentSlotType.Head});
            }
        }
        
        public Armor Torso
        {
            get { return _torso; }
            set
            {
                _torso = value;
                OnItemEquipped(new CharacterEquipmentEventArgs {Item = value, Slot = EquipmentSlotType.Torso});
            }
        }
        
        public Armor Arms
        {
            get { return _arms; }
            set
            {
                _arms = value;
                OnItemEquipped(new CharacterEquipmentEventArgs {Item = value, Slot = EquipmentSlotType.Arms});
            }
        }
        
        public Armor Legs
        {
            get { return _legs; }
            set
            {
                _legs = value;
                OnItemEquipped(new CharacterEquipmentEventArgs {Item = value, Slot = EquipmentSlotType.Legs});
            }
        }
        
        public Armor Feet
        {
            get { return _feet; }
            set
            {
                _feet = value;
                OnItemEquipped(new CharacterEquipmentEventArgs {Item = value, Slot = EquipmentSlotType.Feet});
            }
        }
        
        public Item LeftRing
        {
            get { return _leftRing; }
            set
            {
                _leftRing = value;
                OnItemEquipped(new CharacterEquipmentEventArgs {Item = value, Slot = EquipmentSlotType.LeftRing});
            }
        }
        
        public Item RightRing
        {
            get { return _rightRing; }
            set
            {
                _rightRing = value;
                OnItemEquipped(new CharacterEquipmentEventArgs {Item = value, Slot = EquipmentSlotType.RightRing});
            }
        }
        
        public Item NeckLace
        {
            get { return _neckLace; }
            set
            {
                _neckLace = value;
                OnItemEquipped(new CharacterEquipmentEventArgs {Item = value, Slot = EquipmentSlotType.Neck});
            }
        }
        
        public Tool LeftTool
        {
            get { return _leftTool; }
            set
            {
                if (_leftTool != null)
                    _leftTool.Parent = null;

                _leftTool = value;

                if (_leftTool != null)
                    _leftTool.Parent = _parent;

                OnItemEquipped(new CharacterEquipmentEventArgs {Item = value, Slot = EquipmentSlotType.LeftHand});
            }
        }
        
        public Tool RightTool
        {
            get { return _rightTool; }
            set
            {
                if (_rightTool != null)
                    _rightTool.Parent = null;

                _rightTool = value;

                if (_rightTool != null)
                    _rightTool.Parent = _parent;

                OnItemEquipped(new CharacterEquipmentEventArgs {Item = value, Slot = EquipmentSlotType.RightHand});
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

    public class CharacterEquipmentEventArgs : EventArgs
    {
        public Item Item { get; set; }
        public EquipmentSlotType Slot { get; set; }
    }
}
