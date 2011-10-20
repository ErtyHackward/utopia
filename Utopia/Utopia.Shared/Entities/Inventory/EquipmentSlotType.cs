using System;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Describes the slot on the character
    /// </summary>
    [Flags]
    public enum EquipmentSlotType : ushort
    {
        None = 0,
        Head = 1,
        Torso = 2,
        Legs = 4,
        Feet = 8,
        Arms = 16,
        LeftHand = 32,
        RightHand = 64,
        LeftRing = 128,
        RightRing = 256,
        Neck = 512

        //XXX never seen a game with stackable rings , would be cool
    }
}
