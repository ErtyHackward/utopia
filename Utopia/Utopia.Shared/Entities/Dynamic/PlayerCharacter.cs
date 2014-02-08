using System.Collections.Generic;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Represents a player character (it has a toolbar)
    /// </summary>
    /// <remarks></remarks>
    [ProtoContract]
    [EditorHide]
    public sealed class PlayerCharacter : RpgCharacterEntity
    {
        public static float DefaultMoveSpeed = 5f;
        
        /// <summary>
        /// List of player toolbar 
        /// Each items represents the BlueprintId of an item
        /// </summary>
        [ProtoMember(1, OverwriteList = true)]
        public List<ushort> Toolbar { get; set; }
        
        public override ushort ClassId
        {
            get { return EntityClassId.PlayerCharacter; }
        }
        
        public PlayerCharacter()
        {
            //Define the default PlayerCharacter ToolBar
            Toolbar = new List<ushort>();
            for (int i = 0; i < 10; i++)
            {
                Toolbar.Add(0);
            }

            MoveSpeed = DefaultMoveSpeed;               //Default player MoveSpeed
            RotationSpeed = 10f;          //Default Player Rotation Speed
            DefaultSize = new Vector3(0.5f, 1.9f, 0.5f); //Default player size
            
            BodyRotation = Quaternion.Identity;
            ModelName = "Girl";
            Name = "Player";
        }
        
        public void ToolUse()
        {
            ToolUse((ITool)Equipment.RightTool);
        }

        public void HandUse()
        {
            HandTool.Use(this);

            ToolUse(null);
        }

        public void PutUse()
        {
            if (Equipment.RightTool != null)
            {
                Equipment.RightTool.Put(this);

                var args = EntityUseEventArgs.FromState(this);
                args.Tool = Equipment.RightTool;
                args.UseType = UseType.Put;
                OnUse(args);
            }
        }

        public IItem LookupItem(uint itemId)
        {
            if (itemId == 0) return null;
            foreach (var slot in Inventory)
            {
                if (slot.Item.StaticId == itemId) return slot.Item;
            }

            var equipmentSlot = Equipment.Find(itemId);

            if (equipmentSlot != null)
                return equipmentSlot.Item;

            return null;
        }

    }
}
