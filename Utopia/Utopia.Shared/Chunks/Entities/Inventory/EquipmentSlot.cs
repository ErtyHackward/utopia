using System;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// Describes the slot on the character
    /// </summary>
    [Flags]
    public enum EquipmentSlot
    {
        Head = 1,
        Torso = 2,
        Legs = 4,
        Feet = 8,
        Arms = 16,
        LeftHand = 32,
        RightHand = 64,
        LeftRing = 128,
        RightRing = 256,
        Bags = 512,
        Neck = 1024

        //XXX never seen a game with stackable rings , would be cool
    }
}
