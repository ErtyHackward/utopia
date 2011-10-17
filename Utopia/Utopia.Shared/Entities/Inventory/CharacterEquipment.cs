using System;
using System.Collections.Generic;
using System.IO;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents character equipment
    /// </summary>
    public class CharacterEquipment : IBinaryStorable
    {
        private readonly DynamicEntity _parent;

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
            e.Entity = _parent;
            var handler = ItemEquipped;
            if (handler != null) handler(this, e);
        }

        private Armor _headGear;
        private Armor _torso;
        private Armor _arms;
        private Armor _legs;
        private Armor _feet;
        private IItem _leftRing;
        private IItem _rightRing;
        private IItem _neckLace;
        private ToolSlot _leftTool;
        private ToolSlot _rightTool;

        public CharacterEquipment(DynamicEntity parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Wear some item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="slot"></param>
        /// <returns>Item was weared off</returns>
        public IItem WearItem(IItem item, EquipmentSlotType slot)
        {
            IItem previous;
            switch (slot)
            {
                case EquipmentSlotType.Head:
                    previous = HeadGear;
                    HeadGear = item as Armor;
                    break;
                case EquipmentSlotType.Torso:
                    previous = Torso;
                    Torso = item as Armor;
                    break;
                case EquipmentSlotType.Legs:
                    previous = Legs;
                    Legs = item as Armor;
                    break;
                case EquipmentSlotType.Feet:
                    previous = Feet;
                    Feet = item as Armor;
                    break;
                case EquipmentSlotType.Arms:
                    previous = Arms;
                    Arms = item as Armor;
                    break;
                case EquipmentSlotType.LeftHand:
                    throw new InvalidOperationException("Use LeftHand property instead of WearItem");
                case EquipmentSlotType.RightHand:
                    throw new InvalidOperationException("Use RightHand property instead of WearItem");
                case EquipmentSlotType.LeftRing:
                    previous = LeftRing;
                    LeftRing = item;
                    break;
                case EquipmentSlotType.RightRing:
                    previous = RightRing;
                    RightRing = item;
                    break;
                case EquipmentSlotType.Neck:
                    previous = NeckLace;
                    NeckLace = item;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("slot");
            }

            return previous;
        }

        public Armor HeadGear
        {
            get { return _headGear; }
            set
            {
                var previous = _headGear;
                _headGear = value;
                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = new ContainedSlot { Item = value }, UnequippedItem = new ContainedSlot { Item = previous }, Slot = EquipmentSlotType.Head });
            }
        }
        
        public Armor Torso
        {
            get { return _torso; }
            set
            {
                var previous = _torso;
                _torso = value;
                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = new ContainedSlot { Item = value }, UnequippedItem = new ContainedSlot { Item = previous }, Slot = EquipmentSlotType.Torso });
            }
        }
        
        public Armor Arms
        {
            get { return _arms; }
            set
            {
                var previous = _arms;
                _arms = value;
                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = new ContainedSlot { Item = value }, UnequippedItem = new ContainedSlot { Item = previous }, Slot = EquipmentSlotType.Arms });
            }
        }
        
        public Armor Legs
        {
            get { return _legs; }
            set
            {
                var previous = _legs;
                _legs = value;
                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = new ContainedSlot { Item = value }, UnequippedItem = new ContainedSlot { Item = previous }, Slot = EquipmentSlotType.Legs });
            }
        }
        
        public Armor Feet
        {
            get { return _feet; }
            set
            {
                var previous = _feet;
                _feet = value;
                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = new ContainedSlot { Item = value }, UnequippedItem = new ContainedSlot { Item = previous }, Slot = EquipmentSlotType.Feet });
            }
        }

        public IItem LeftRing
        {
            get { return _leftRing; }
            set
            {
                var previous = _leftRing;
                _leftRing = value;
                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = new ContainedSlot { Item = value }, UnequippedItem = new ContainedSlot { Item = previous }, Slot = EquipmentSlotType.LeftRing });
            }
        }

        public IItem RightRing
        {
            get { return _rightRing; }
            set
            {
                var previous = _rightRing;
                _rightRing = value;
                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = new ContainedSlot { Item = value }, UnequippedItem = new ContainedSlot { Item = previous }, Slot = EquipmentSlotType.RightRing });
            }
        }

        public IItem NeckLace
        {
            get { return _neckLace; }
            set
            {
                var previous = _neckLace;
                _neckLace = value;
                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = new ContainedSlot { Item = value }, UnequippedItem = new ContainedSlot { Item = previous }, Slot = EquipmentSlotType.Neck });
            }
        }

        public ToolSlot LeftTool
        {
            get { return _leftTool; }
            set
            {
                var previous = _leftTool;

                _leftTool = value;

                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = _leftTool, UnequippedItem = previous, Slot = EquipmentSlotType.LeftHand });
            }
        }

        public ToolSlot RightTool
        {
            get { return _rightTool; }
            set
            {
                var previous = _rightTool;

                _rightTool = value;

                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = _rightTool, UnequippedItem = previous, Slot = EquipmentSlotType.RightHand });
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

            if (LeftTool != null)
                LeftTool.Save(writer);
            else new ContainedSlot { ItemsCount = 0 }.Save(writer);
            if (RightTool != null)
                RightTool.Save(writer);
            else new ContainedSlot { ItemsCount = 0 }.Save(writer);
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

            LeftRing = LoadItem<VoxelItem>(reader);
            RightRing = LoadItem<VoxelItem>(reader);
            NeckLace = LoadItem<VoxelItem>(reader);

            LeftTool = new ToolSlot();
            LeftTool.Load(reader);
            RightTool = new ToolSlot();
            RightTool.Load(reader);
        }

        public IEnumerable<IItem> AllItems()
        {
            if(_leftTool != null)
                yield return _leftTool.Item;

            if(_rightTool != null)
                yield return _rightTool.Item;
            
            yield return _headGear;
            yield return _torso;
            yield return _arms;
            yield return _legs; 
            yield return _feet;
            yield return _leftRing;
            yield return _rightRing;
            yield return _neckLace;
        }
    }

    public class CharacterEquipmentEventArgs : EventArgs
    {
        public IDynamicEntity Entity { get; set; }
        public ContainedSlot EquippedItem { get; set; }
        public ContainedSlot UnequippedItem { get; set; }
        public EquipmentSlotType Slot { get; set; }
    }
}
