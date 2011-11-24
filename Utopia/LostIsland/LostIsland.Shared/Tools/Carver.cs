using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace LostIsland.Shared.Tools
{
    /// <summary>
    /// Carver is a spriteItem to show an icon in UI 
    /// and eventually to show the carving tool itself in world, it's surely too small to be a voxel item
    /// maybe it will become a voxel item some day in fact ;)
    /// </summary>
    
    public class Carver : SpriteItem, IGameStateTool
    {
        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.Carver; }
        }

        public override string DisplayName
        {
            get { return "Carver"; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }

        public override string Description
        {
            get { return "Carve tool to make sculptures"; }
        }
    }
}
