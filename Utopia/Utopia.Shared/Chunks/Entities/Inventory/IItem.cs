using System;
using S33M3Engines.Shared.Sprites;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Interfaces;
namespace Utopia.Shared.Chunks.Entities.Inventory
{
    public interface IItem : IEntity
    {
        EquipmentSlotType AllowedSlots { get; set; }   
        int MaxStackSize { get; }
        string UniqueName { get; set; }
    }
}
