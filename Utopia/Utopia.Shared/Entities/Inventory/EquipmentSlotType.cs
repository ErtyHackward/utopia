namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Describes the equipment slot on the character
    /// </summary>
    public enum EquipmentSlotType : byte
    {
        None = 255,
        LeftHand = 0,
        RightHand = 1,
        Head = 2,
        Torso = 3,
        Legs = 4,
        Feet = 5,
        Arms = 6,
        LeftRing = 7,
        RightRing = 8,
        Neck = 9
    }
}
