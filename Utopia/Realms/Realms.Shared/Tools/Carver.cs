using System;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Realms.Shared.Tools
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
            get { return RealmsEntityClassId.Carver; }
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


        public override EquipmentSlotType AllowedSlots
        {
            get { return EquipmentSlotType.LeftHand; }
            set { throw new NotSupportedException(); }
        }

        public IToolImpact Use(IDynamicEntity owner, ToolUseMode useMode, bool runOnServer)
        {
            throw new NotImplementedException();
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }
    }
}