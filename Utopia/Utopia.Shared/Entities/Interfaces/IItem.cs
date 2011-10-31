using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Interfaces
{
    public interface IItem : IStaticEntity
    {
        EquipmentSlotType AllowedSlots { get; set; }   
        int MaxStackSize { get; }
        string UniqueName { get; set; }
        string StackType { get; }

        /// <summary>
        /// Gets item description
        /// </summary>
        string Description { get; }
    }
}
