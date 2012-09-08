using System.Collections.Generic;
using SharpDX;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Represents a player character (it has a toolbar)
    /// </summary>
    /// <remarks></remarks>
    public sealed class PlayerCharacter : SpecialCharacterEntity
    {
        #region Public variables/properties

        public static float DefaultMoveSpeed = 5f;
        
        public List<uint> Toolbar { get; private set; }
        
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
            Toolbar = new List<uint>();
            for (int i = 0; i < 10; i++)
            {
                Toolbar.Add(0);
            }

            MoveSpeed = DefaultMoveSpeed;               //Default player MoveSpeed
            RotationSpeed = 10f;          //Default Player Rotation Speed
            DefaultSize = new SharpDX.Vector3(0.5f, 1.9f, 0.5f); //Default player size
            
            Type = EntityType.Dynamic;
            BodyRotation = Quaternion.Identity;
            ModelName = "s33m3";
        }

        #region Public Methods
        public void LeftToolUse(ToolUseMode useMode)
        {
            if (Equipment.LeftTool != null)
            {
                var args = EntityUseEventArgs.FromState(EntityState);
                args.Tool = Equipment.LeftTool;
                args.UseMode = useMode;
                OnUse(args);
            }
        }

        public void RightToolUse(ToolUseMode useMode)
        {
            if (Equipment.RightTool != null)
            {
                var args = EntityUseEventArgs.FromState(EntityState);
                args.Tool = Equipment.RightTool;
                args.UseMode = useMode;
                OnUse(args);
            }
        }

        public void EntityUse()
        {
            if (EntityState.IsEntityPicked)
            {
                var args = EntityUseEventArgs.FromState(EntityState);
                OnUse(args);
            }
        }

        public override void Load(System.IO.BinaryReader reader, EntityFactory factory)
        {
            base.Load(reader, factory);

            Toolbar.Clear();
            for (int i = 0; i < 10; i++)
            {
                Toolbar.Add(reader.ReadUInt32());
            }
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);
            for (int i = 0; i < 10; i++)
            {
                writer.Write(Toolbar[i]);
            }
        }

        public IItem LookupItem(uint itemId)
        {
            if (itemId==0) return null;
            foreach (var slot in Inventory)
            {
                if (slot.Item.StaticId == itemId) return slot.Item;
            }

            var equipmentSlot = Equipment.Find(itemId);

            if (equipmentSlot != null)
                return equipmentSlot.Item;

            return null;
        }

       

        #endregion
    }
}
