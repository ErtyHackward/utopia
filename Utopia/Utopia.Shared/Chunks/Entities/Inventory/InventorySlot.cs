using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    [Flags]
    public enum InventorySlot
    {

        Head = 1, Torso = 2, Legs = 4, Feet = 8, Arms = 16, LeftHand = 32,
        RightHand = 64, LeftRing = 128, RightRing = 256, Bags = 512, Neck = 1024

        //XXX never seen a game with stackable rings , would be cool
    }
}
