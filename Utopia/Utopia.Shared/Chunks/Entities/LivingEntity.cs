using System;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Roleplay;

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
            Inventory = new BaseContainer();
            PrimaryAttributes = new CharacterPrimaryAttributes();
            SecondaryAttributes = new CharacterSecondaryAttributes();
        }

        #region Properties
        /// <summary>
        /// Gets character equipment
        /// </summary>
        public CharacterEquipment Equipment { get; private set; }

        /// <summary>
        /// Gets character inventory
        /// </summary>
        public BaseContainer Inventory { get; private set; }
        
        /// <summary>
        /// Gets character primary attributes
        /// </summary>
        public CharacterPrimaryAttributes PrimaryAttributes { get; set; }

        /// <summary>
        /// Gets character secondary attributes (skills)
        /// </summary>
        public CharacterSecondaryAttributes SecondaryAttributes { get; set; }

        #endregion

        /// <summary>
        /// Loads current entity from a binaryReader
        /// </summary>
        /// <param name="reader"></param>
        public override void Load(System.IO.BinaryReader reader)
        {
            base.Load(reader);

            Equipment.Load(reader);
            Inventory.Load(reader);
            PrimaryAttributes.Load(reader);
            SecondaryAttributes.Load(reader);
        }

        /// <summary>
        /// Saves(serializes) current entity instance to a binaryWriter
        /// </summary>
        /// <param name="writer"></param>
        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);

            Equipment.Save(writer);
            Inventory.Save(writer);
            PrimaryAttributes.Save(writer);
            SecondaryAttributes.Save(writer);
        }
    }
}
