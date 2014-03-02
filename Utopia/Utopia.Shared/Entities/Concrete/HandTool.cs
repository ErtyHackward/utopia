using System;
using System.ComponentModel;
using ProtoBuf;
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
        [Description("Is the tool will be used multiple times when the mouse putton is pressed")]
        [ProtoMember(1)]
        public bool RepeatedActionsAllowed { get; set; }

        public override PickType CanPickBlock(BlockProfile blockProfile)
        {
            if (blockProfile.Id == WorldConfiguration.CubeId.Air)
                return PickType.Transparent;
            
            // don't allow to pick blocks by hand
            return PickType.Stop;
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            IToolImpact checkImpact;

            if (!CanDoEntityAction(owner, out checkImpact))
                return checkImpact;

            var impact = new EntityToolImpact();

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
                impact.EntityId = entity.StaticId;
                return impact;
            }

            var cursor = LandscapeManager.GetCursor(entity.Position);

            if (cursor == null)
            {
                impact.Dropped = true;
                return impact;
            }

            if (!entity.IsPickable)
            {
                impact.Message = "You need a special tool to pick this item";
                return impact;
            }

            cursor.OwnerDynamicId = owner.DynamicId;
            var charEntity = owner as CharacterEntity;

            if (charEntity != null)
            {
                var item = (IItem)entity;
                

                if (charEntity.Inventory.PutItem(item))
                {
                    cursor.RemoveEntity(owner.EntityState.PickedEntityLink);
                    impact.Success = true;

                    // entity should lose its voxel intance if put into the inventory
                    item.ModelInstance = null;

                    return impact;
                }

                impact.Message = "Unable to put item to the inventory, is it full?";
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
