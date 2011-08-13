﻿namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Enumerates all available entities IDs (just for easy use in code). Please add new IDs to the bottom of the enum
    /// </summary>
    public enum EntityId : ushort
    {
        // items
        None = 0,
        Sword = 1,
        PickAxe = 2,
        Shovel = 3,
        Hoe = 4,
        Axe = 5,

        // static
        Chest = 1000,
        Chair = 1001,
        Door = 1002,
        Bed = 1003,

        // blocks
        ThinGlass = 2001,
    }
}
