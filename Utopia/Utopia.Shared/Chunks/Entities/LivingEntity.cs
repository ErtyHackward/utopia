using System;
using Utopia.Shared.Chunks.Entities.Inventory;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Represents an alive entity (players, animals, NPC)
    /// </summary>
    public abstract class LivingEntity : Entity
    {
        #region Events
        /// <summary>
        /// Occurs when left tool of the player is used
        /// </summary>
        public event EventHandler<LivingEntityUseEventArgs> LeftToolUse;

        public virtual void OnLeftToolUse(LivingEntityUseEventArgs e)
        {
            var handler = LeftToolUse;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when right tool of the player is used
        /// </summary>
        public event EventHandler<LivingEntityUseEventArgs> RightToolUse;

        public virtual void OnRightToolUse(LivingEntityUseEventArgs e)
        {
            var handler = RightToolUse;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when player pressing "E" key to use entity that he is point to
        /// </summary>
        public event EventHandler<LivingEntityUseEventArgs> EntityUse;

        public virtual void OnEntityUse(LivingEntityUseEventArgs e)
        {
            var handler = EntityUse;
            if (handler != null) handler(this, e);
        }

        #endregion

        protected LivingEntity()
        {
            Equipment = new CharacterEquipment(this);
            Inventory = new SlotContainer<ContainedSlot>();

        }

        #region Properties
        /// <summary>
        /// Gets character equipment
        /// </summary>
        public CharacterEquipment Equipment { get; private set; }

        /// <summary>
        /// Gets character inventory
        /// </summary>
        public SlotContainer<ContainedSlot> Inventory { get; private set; }

        /// <summary>
        /// Gets or sets entity state (this field should be refreshed before using the tool)
        /// </summary>
        public LivingEntityState EntityState { get; set; }

        /// <summary>
        /// Gets character name
        /// </summary>
        public string CharacterName { get; set; }

        /// <summary>
        /// Indicates if this charater controlled by real human
        /// </summary>
        public bool IsRealPlayer { get; set; }

        /// <summary>
        /// Gets current health points of entity
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// Gets maximum health point of entity
        /// </summary>
        public int MaxHealth { get; set; }

        #endregion

        /// <summary>
        /// Request tool using
        /// </summary>
        /// <param name="type"></param>
        public void UseTool(LivingEntityUseType type)
        {
            switch (type)
            {
                case LivingEntityUseType.LeftTool:
                    if (Equipment.LeftTool != null)
                    {
                        if (Equipment.LeftTool.Use())
                            OnLeftToolUse(LivingEntityUseEventArgs.FromState(EntityState, LivingEntityUseType.LeftTool));
                    }
                    break;
                case LivingEntityUseType.RightTool:
                    if (Equipment.RightTool != null)
                    {
                        if (Equipment.RightTool.Use())
                            OnRightToolUse(LivingEntityUseEventArgs.FromState(EntityState, LivingEntityUseType.RightTool));
                    }
                    break;
                case LivingEntityUseType.Use:
                    OnEntityUse(LivingEntityUseEventArgs.FromState(EntityState, LivingEntityUseType.RightTool));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        /// <summary>
        /// Loads current entity from a binaryReader
        /// </summary>
        /// <param name="reader"></param>
        public override void Load(System.IO.BinaryReader reader)
        {
            base.Load(reader);

            CharacterName = reader.ReadString();
            Equipment.Load(reader);
            Inventory.Load(reader);

        }

        /// <summary>
        /// Saves(serializes) current entity instance to a binaryWriter
        /// </summary>
        /// <param name="writer"></param>
        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);

            writer.Write(CharacterName);
            Equipment.Save(writer);
            Inventory.Save(writer);
        }
    }
}
