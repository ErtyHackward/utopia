﻿using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Cubes;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Represents a player character (it has a toolbar)
    /// </summary>
    public sealed class PlayerCharacter : SpecialCharacterEntity
    {
        #region Private variables
        #endregion

        #region Public variables/properties

        public static float DefaultMoveSpeed = 5f;
        
        public SlotContainer<ToolbarSlot> Toolbar { get; private set; }
        
        public override ushort ClassId
        {
            get { return EntityClassId.PlayerCharacter; }
        }

        public override string DisplayName
        {
            get { return CharacterName; }
        }
        #endregion

        public PlayerCharacter()
        {
            //Define the default PlayerCharacter ToolBar
            Toolbar = new SlotContainer<ToolbarSlot>(new Vector2I(10, 1));

            MoveSpeed = DefaultMoveSpeed;               //Default player MoveSpeed
            RotationSpeed = 10f;          //Default Player Rotation Speed
            Size = new SharpDX.Vector3(0.5f, 1.9f, 0.5f); //Default player size
            
            //Default Player Voxel Body
            Model.Blocks = new byte[1, 1, 1];
            Model.Blocks[0, 0, 0] = CubeId.PlayerHead;

            Type = EntityType.Dynamic;
        }

        #region Public Methods
        public void LeftToolUse()
        {
            if (Equipment.LeftTool != null)
            {
                var args = EntityUseEventArgs.FromState(EntityState);
                args.Tool = Equipment.LeftTool;
                OnUse(args);
            }
        }

        public void RightToolUse()
        {
            if (Equipment.RightTool != null)
            {
                var args = EntityUseEventArgs.FromState(EntityState);
                args.Tool = Equipment.RightTool;
                OnUse(args);
            }
        }

        public void EntityUse()
        {
            if (EntityState.PickedEntityId != 0)
            {
                var args = EntityUseEventArgs.FromState(EntityState);
                OnUse(args);
            }
        }

        public override void Load(System.IO.BinaryReader reader)
        {
            base.Load(reader);
            Toolbar.Load(reader);
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);
            Toolbar.Save(writer);
        }

        public IItem LookupItem(uint itemId)
        {
            if (itemId==0) return null;
            foreach (ContainedSlot slot in Inventory)
            {
                if (slot.Item.EntityId == itemId) return slot.Item;
            }

            IItem[] equipment = new IItem[]
                                    {
                                        Equipment.HeadGear, Equipment.LeftRing, Equipment.RightRing,
                                        Equipment.LeftTool, Equipment.RightTool, Equipment.Legs, Equipment.NeckLace,
                                        Equipment.Torso, Equipment.Feet
                                    };

            foreach (var item in equipment)
            {
                if (item!=null && item.EntityId == itemId) return item;
            }
            
            return null;
        }

       

        #endregion
    }
}
