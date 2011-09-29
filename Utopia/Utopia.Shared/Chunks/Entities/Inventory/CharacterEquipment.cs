using System;
using System.IO;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Chunks.Entities.Inventory
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
        private VoxelItem _leftRing;
        private VoxelItem _rightRing;
        private VoxelItem _neckLace;
        private Tool _leftTool;
        private Tool _rightTool;

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
        public IItem WearItem(VoxelItem item, EquipmentSlotType slot)
        {
            VoxelItem previous = null;
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
                    previous = LeftTool;
                    LeftTool = item as Tool;
                    break;
                case EquipmentSlotType.RightHand:
                    previous = RightTool;
                    RightTool = item as Tool;
                    break;
                case EquipmentSlotType.LeftRing:
                    previous = LeftRing;
                    LeftRing = item;
                    break;
                case EquipmentSlotType.RightRing:
                    previous = RightRing;
                    RightRing = item;
                    break;
                case EquipmentSlotType.Bags:
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

        public VoxelItem LeftRing
        {
            get { return _leftRing; }
            set
            {
                _leftRing = value;
                OnItemEquipped(new CharacterEquipmentEventArgs {Item = value, Slot = EquipmentSlotType.LeftRing});
            }
        }

        public VoxelItem RightRing
        {
            get { return _rightRing; }
            set
            {
                _rightRing = value;
                OnItemEquipped(new CharacterEquipmentEventArgs {Item = value, Slot = EquipmentSlotType.RightRing});
            }
        }

        public VoxelItem NeckLace
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

            LeftRing = LoadItem<VoxelItem>(reader);
            RightRing = LoadItem<VoxelItem>(reader);
            NeckLace = LoadItem<VoxelItem>(reader);

            LeftTool = LoadItem<Tool>(reader);
            RightTool = LoadItem<Tool>(reader);
        }
    }

    public class CharacterEquipmentEventArgs : EventArgs
    {
        public IDynamicEntity Entity { get; set; }
        public VoxelItem Item { get; set; }
        public EquipmentSlotType Slot { get; set; }
    }
}
