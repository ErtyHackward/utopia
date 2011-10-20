using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Provides character base properties
    /// </summary>
    public abstract class CharacterEntity : DynamicEntity
    {
        protected CharacterEntity()
        {
            Equipment = new CharacterEquipment(this);
            Inventory = new SlotContainer<ContainedSlot>();
        }

        /// <summary>
        /// Gets character equipment
        /// </summary>
        public CharacterEquipment Equipment { get; private set; }

        /// <summary>
        /// Gets character inventory
        /// </summary>
        public SlotContainer<ContainedSlot> Inventory { get; private set; }

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

        /// <summary>
        /// Returns tool that can be used
        /// </summary>
        /// <param name="toolId"></param>
        /// <returns>Tool instance or null</returns>
        public ITool FindToolById(uint toolId)
        {
            if (Equipment.LeftSlot != null && Equipment.LeftSlot.Item.EntityId == toolId)
                return (ITool)Equipment.LeftSlot.Item;
            if (Equipment.RightSlot != null && Equipment.RightSlot.Item.EntityId == toolId)
                return (ITool)Equipment.RightSlot.Item;
            return null;
        }

        public override void Load(System.IO.BinaryReader reader)
        {
            base.Load(reader);

            CharacterName = reader.ReadString();
            Equipment.Load(reader);
            Inventory.Load(reader);
            Health = reader.ReadInt32();
            MaxHealth = reader.ReadInt32();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);

            writer.Write(CharacterName);
            Equipment.Save(writer);
            Inventory.Save(writer);
            writer.Write(Health);
            writer.Write(MaxHealth);
        }
    }
}
