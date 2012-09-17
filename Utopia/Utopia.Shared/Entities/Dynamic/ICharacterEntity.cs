using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Provides character base properties. Character entity has an equipment and inventory. It can wear the tool.
    /// </summary>
    public interface ICharacterEntity
    {
        /// <summary>
        /// Gets character equipment
        /// </summary>
        CharacterEquipment Equipment { get; }

        /// <summary>
        /// Gets character inventory
        /// </summary>
        SlotContainer<ContainedSlot> Inventory { get; }

        /// <summary>
        /// Gets character name
        /// </summary>
        string CharacterName { get; set; }
    }
}