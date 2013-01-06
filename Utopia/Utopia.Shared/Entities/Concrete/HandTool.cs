using S33M3Resources.Structs;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Special tool used when no tool is set
    /// </summary>
    public class HandTool : Item
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Hand; }
        }

        public override bool CanUse
        {
            get { return true; }
        }

        public override PickType CanPickBlock(byte blockId)
        {
            if (blockId == 0)
                return PickType.Transparent;
            
            // don't allow to pick blocks by hand
            return PickType.Stop;
        }

        public override IToolImpact Use(IDynamicEntity owner)
        {
            var impact = new ToolImpact();

            if (!owner.EntityState.IsEntityPicked)
                return impact;

            if (owner.EntityState.PickedEntityLink.IsDynamic)
                return impact;

            var entity = owner.EntityState.PickedEntityLink.ResolveStatic(LandscapeManager);

            var cursor = LandscapeManager.GetCursor(entity.Position);
            
            var charEntity = owner as CharacterEntity;

            if (charEntity != null)
            {
                if (charEntity.Inventory.PutItem((IItem)entity))
                {
                    cursor.RemoveEntity(owner.EntityState.PickedEntityLink, owner.DynamicId);
                    impact.Success = true;
                }
            }

            return impact;
        }
    }
}
