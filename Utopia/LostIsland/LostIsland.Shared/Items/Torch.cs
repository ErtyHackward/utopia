using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Entities;

namespace LostIsland.Shared.Items
{
    public class Torch : VoxelItem
    {
        public Torch()
        {
            UniqueName = DisplayName;
            Type = EntityType.Static;
        }

        public override int MaxStackSize
        {
            get { return int.MaxValue; }
        }

        
        public override string Description
        {
            get { return "Let there be light"; }
        }

        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.Torch; }
        }

        public override string DisplayName
        {
            get { return "Torch"; }
        }
    }
}
