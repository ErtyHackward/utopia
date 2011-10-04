namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Enumerates all available entities IDs (just for easy use in code). Please add new IDs to the bottom of the enum
    /// </summary>
    public enum EntityClassId : ushort
    {
        // items
        None = 0,
        Sword = 1,
        PickAxe = 2,
        Shovel = 3,
        Hoe = 4,
        Axe = 5,
        Survey = 6,
        Annihilator = 7,
        DirtAdder = 8,

        // static
        Chest = 1000,
        Chair = 1001,
        Door = 1002,
        Bed = 1003,
        Tree = 1004,

        // blocks
        ThinGlass = 2001,

        //alive
        PlayerCharacter = 3000,
        NonPlayerCharacter = 3001,
        Zombie = 3002,

        //Static Sprite
        Grass = 4000,

        //Special case
        EditableEntity = 10001,
        
    }
}
