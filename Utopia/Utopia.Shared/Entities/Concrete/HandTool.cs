using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Special tool used when no tool is set
    /// </summary>
    public class HandTool : Item, ITool
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Hand; }
        }

        public override PickType CanPickBlock(CubeProfile cubeProfile)
        {
            if (cubeProfile.Id == WorldConfiguration.CubeId.Air)
                return PickType.Transparent;
            
            // don't allow to pick blocks by hand
            return PickType.Stop;
        }

        public IToolImpact Use(IDynamicEntity owner)
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
                var item = (IItem)entity;

                // entity should lose its voxel intance if put into the inventory
                item.ModelInstance = null;

                if (charEntity.Inventory.PutItem(item))
                {
                    cursor.RemoveEntity(owner.EntityState.PickedEntityLink, owner.DynamicId);
                    impact.Success = true;
                }
            }

            return impact;
        }


        public void Rollback(IToolImpact impact)
        {
            throw new System.NotImplementedException();
        }
    }
}
