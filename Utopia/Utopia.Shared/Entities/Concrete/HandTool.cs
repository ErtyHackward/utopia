using System;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Special tool used when no tool is set (character mode)
    /// </summary>
    [EditorHide]
    public class HandTool : Item, ITool
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Hand; }
        }

        public override PickType CanPickBlock(BlockProfile blockProfile)
        {
            if (blockProfile.Id == WorldConfiguration.CubeId.Air)
                return PickType.Transparent;
            
            // don't allow to pick blocks by hand
            return PickType.Stop;
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            var impact = new ToolImpact();

            if (!CanDoEntityAction(owner, ref impact))
                return impact;
            
            if (owner.EntityState.PickedEntityLink.IsDynamic)
            {
                impact.Message = "Only static entities allowed to use";
                return impact;
            }

            var entity = owner.EntityState.PickedEntityLink.ResolveStatic(LandscapeManager);

            if (entity == null)
            {
                impact.Message = "There is no entity by this link";
                return impact;
            }

            if (entity is IUsableEntity)
            {
                var usable = (IUsableEntity)entity;
                usable.Use();
                impact.Success = true;
                return impact;
            }

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
                    return impact;
                }

                impact.Message = "Unable to put item to the inventory";
                return impact;
            }

            impact.Message = "Expected CharacterEntity owner";
            return impact;
        }
    }

    /// <summary>
    /// Allows to hide class from using as base in the editor
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorHideAttribute : Attribute
    {

    }
}
