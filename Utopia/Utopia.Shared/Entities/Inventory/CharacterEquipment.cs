using System;
using System.Collections.Generic;
using System.IO;
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

        private EquipmentSlot<IHeadArmor> _headGear;
        private EquipmentSlot<ITorsoArmor> _torso;
        private EquipmentSlot<IArmsArmor> _arms;
        private EquipmentSlot<ILegsArmor> _legs;
        private EquipmentSlot<IFeetArmor> _feet;
        private EquipmentSlot<IRing> _leftRing;
        private EquipmentSlot<IRing> _rightRing;
        private EquipmentSlot<INecklace> _neckLace;
        private EquipmentSlot<ITool> _leftTool;
        private EquipmentSlot<ITool> _rightTool;

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
        
        public CharacterEquipment(DynamicEntity parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Wear some item
        /// </summary>
        /// <param name="newSlot"></param>
        /// <param name="slot"></param>
        /// <returns>Item was weared off</returns>
        public IItem WearItem(ContainedSlot newSlot, EquipmentSlotType slot)
        {
            ContainedSlot previous;
            
            switch (slot)
            {
                case EquipmentSlotType.Head:
                    previous = _headGear;
                    _headGear = EquipmentSlot<IHeadArmor>.FromBase(newSlot);
                    break;
                case EquipmentSlotType.Torso:
                    previous = _torso;
                    _torso = EquipmentSlot<ITorsoArmor>.FromBase(newSlot);
                    break;
                case EquipmentSlotType.Legs:
                    previous = _legs;
                    _legs = EquipmentSlot<ILegsArmor>.FromBase(newSlot);
                    break;
                case EquipmentSlotType.Feet:
                    previous = _feet;
                    _feet = EquipmentSlot<IFeetArmor>.FromBase(newSlot);
                    break;
                case EquipmentSlotType.Arms:
                    previous = _arms;
                    _arms = EquipmentSlot<IArmsArmor>.FromBase(newSlot);
                    break;
                case EquipmentSlotType.LeftHand:
                    previous = _leftTool;
                    _leftTool = EquipmentSlot<ITool>.FromBase(newSlot);
                    break;
                case EquipmentSlotType.RightHand:
                    previous = _rightTool;
                    _rightTool = EquipmentSlot<ITool>.FromBase(newSlot);
                    break;
                case EquipmentSlotType.LeftRing:
                    previous = _leftRing;
                    _leftRing = EquipmentSlot<IRing>.FromBase(newSlot);
                    break;
                case EquipmentSlotType.RightRing:
                    previous = _rightRing;
                    _rightRing = EquipmentSlot<IRing>.FromBase(newSlot);
                    break;
                case EquipmentSlotType.Neck:
                    previous = _neckLace;
                    _neckLace = EquipmentSlot<INecklace>.FromBase(newSlot);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("slot");
            }

            OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = newSlot, UnequippedItem = previous, Slot = slot });

            return previous != null ? previous.Item : null;
        }

        /// <summary>
        /// Determines if item can be put to equipment slot
        /// </summary>
        /// <param name="item"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public bool CanWear(ContainedSlot item, EquipmentSlotType slot)
        {
            if(item == null || item.Item == null) return false;
            switch (slot)
            {
                case EquipmentSlotType.None: return false;
                case EquipmentSlotType.Head: return item.Item is IHeadArmor;
                case EquipmentSlotType.Torso: return item.Item is ITorsoArmor;
                case EquipmentSlotType.Legs: return item.Item is ILegsArmor;
                case EquipmentSlotType.Feet: return item.Item is IFeetArmor;
                case EquipmentSlotType.Arms: return item.Item is IArmsArmor;
                case EquipmentSlotType.LeftHand: return item.Item is ITool;
                case EquipmentSlotType.RightHand: return item.Item is ITool;
                case EquipmentSlotType.LeftRing: return item.Item is IRing;
                case EquipmentSlotType.RightRing: return item.Item is IRing;
                case EquipmentSlotType.Neck: return item.Item is INecklace;
                default:
                    throw new ArgumentOutOfRangeException("slot");
            }
        }

        /// <summary>
        /// Returns slot with currently equipped gear
        /// </summary>
        /// <param name="slotType"></param>
        /// <returns></returns>
        public ContainedSlot this[EquipmentSlotType slotType]
        {
            get {
                switch (slotType)
                {
                    case EquipmentSlotType.None:
                        throw new ArgumentOutOfRangeException("slotType");
                    case EquipmentSlotType.Head:
                        return _headGear;
                    case EquipmentSlotType.Torso:
                        return _torso;
                    case EquipmentSlotType.Legs:
                        return _legs;
                    case EquipmentSlotType.Feet:
                        return _feet;
                    case EquipmentSlotType.Arms:
                        return _arms;
                    case EquipmentSlotType.LeftHand:
                        return _leftTool;
                    case EquipmentSlotType.RightHand:
                        return _rightTool;
                    case EquipmentSlotType.LeftRing:
                        return _leftRing;
                    case EquipmentSlotType.RightRing:
                        return _rightRing;
                    case EquipmentSlotType.Neck:
                        return _neckLace;
                    default:
                        throw new ArgumentOutOfRangeException("slotType");
                }
            }
        }
        
        public EquipmentSlot<ITool> LeftSlot
        {
            get { return _leftTool; }
            set
            {
                var previous = _leftTool;

                _leftTool = value;

                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = _leftTool, UnequippedItem = previous, Slot = EquipmentSlotType.LeftHand });
            }
        }

        public EquipmentSlot<ITool> RightSlot
        {
            get { return _rightTool; }
            set
            {
                var previous = _rightTool;

                _rightTool = value;

                OnItemEquipped(new CharacterEquipmentEventArgs { EquippedItem = _rightTool, UnequippedItem = previous, Slot = EquipmentSlotType.RightHand });
            }
        }

        private void SaveSlot(ContainedSlot slot, BinaryWriter writer)
        {
            if (slot != null)
                slot.Save(writer);
            else new ContainedSlot { ItemsCount = 0 }.Save(writer);
        }

        private T LoadSlot<T>(BinaryReader reader) where T : ContainedSlot, new()
        {
            var slot = new T();
            slot.Load(reader);
            return slot;
        }

        /// <summary>
        /// Saves character equipment
        /// </summary>
        /// <param name="writer"></param>
        public void Save(BinaryWriter writer)
        {
            SaveSlot(_headGear, writer);
            SaveSlot(_torso, writer);
            SaveSlot(_arms, writer);
            SaveSlot(_legs, writer);
            SaveSlot(_feet, writer);

            SaveSlot(_leftRing, writer);
            SaveSlot(_rightRing, writer);
            SaveSlot(_neckLace, writer);

            SaveSlot(_leftTool, writer);
            SaveSlot(_rightTool, writer);
        }

        /// <summary>
        /// Loads character equipment
        /// </summary>
        /// <param name="reader"></param>
        public void Load(BinaryReader reader)
        {
            _headGear = LoadSlot<EquipmentSlot<IHeadArmor>>(reader);
            _torso = LoadSlot<EquipmentSlot<ITorsoArmor>>(reader);
            _arms = LoadSlot<EquipmentSlot<IArmsArmor>>(reader);
            _legs = LoadSlot<EquipmentSlot<ILegsArmor>>(reader);
            _feet = LoadSlot<EquipmentSlot<IFeetArmor>>(reader);

            _leftRing = LoadSlot<EquipmentSlot<IRing>>(reader);
            _rightRing = LoadSlot<EquipmentSlot<IRing>>(reader);
            _neckLace = LoadSlot<EquipmentSlot<INecklace>>(reader);

            LeftSlot = LoadSlot<EquipmentSlot<ITool>>(reader);
            RightSlot = LoadSlot<EquipmentSlot<ITool>>(reader);
        }

        /// <summary>
        /// Enumerates all non-null slots
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ContainedSlot> AllItems()
        {
            if(_leftTool != null)
                yield return _leftTool;

            if(_rightTool != null)
                yield return _rightTool;
            
            if(_headGear != null)
                yield return _headGear;

            if(_torso != null)
                yield return _torso;

            if(_arms != null)
                yield return _arms;

            if(_legs != null)
                yield return _legs; 

            if(_feet != null)
                yield return _feet;

            if(_leftRing != null)
                yield return _leftRing;

            if(_rightRing != null)
                yield return _rightRing;

            if(_neckLace != null)
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
