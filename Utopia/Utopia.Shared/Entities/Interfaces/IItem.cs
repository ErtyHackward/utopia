using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Interfaces
{
    public interface IItem : IStaticEntity
    {
        /// <summary>
        /// Gets possible slot types where the item can be put to
        /// </summary>
        EquipmentSlotType AllowedSlots { get; } 
  
        /// <summary>
        /// Gets the maximum number of items that can be put in a single slot
        /// </summary>
        int MaxStackSize { get; }

        /// <summary>
        /// Gets stack string. Entities with the same stack string will be possible to put together in a single slot
        /// </summary>
        string StackType { get; }

        /// <summary>
        /// Gets an item description
        /// </summary>
        string Description { get; }
    }
}
