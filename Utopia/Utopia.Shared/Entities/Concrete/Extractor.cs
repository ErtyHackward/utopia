using System.ComponentModel;
using System.Xml;
using ProtoBuf;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Allows to unmount any pickeable entity
    /// </summary>
    [ProtoContract]
    [Description("Allows to extract any kind of entities including a door or a chest.")]
    public class Extractor : Item, ITool
    {
        [Description("Is the tool will be used multiple times when the mouse putton is pressed")]
        [ProtoMember(1)]
        public bool RepeatedActionsAllowed { get; set; }

        public IToolImpact Use(IDynamicEntity owner)
        {
            IToolImpact checkImpact;

            if (!CanDoEntityAction(owner, out checkImpact))
                return checkImpact;

            var impact = new EntityToolImpact { 
                SrcBlueprintId = BluePrintId 
            };

            if (owner.EntityState.PickedEntityLink.IsDynamic)
            {
                impact.Message = "Only static entities allowed to use";
                return impact;
            }

            var entity = owner.EntityState.PickedEntityLink.ResolveStatic(LandscapeManager);

            if (entity == null)
            {
                impact.Message = "Unable to resolve the link";
                return impact;
            }

            var cursor = LandscapeManager.GetCursor(entity.Position);

            if (cursor == null)
            {
                impact.Dropped = true;
                return impact;
            }

            cursor.OwnerDynamicId = owner.DynamicId;

            var charEntity = owner as CharacterEntity;

            if (charEntity != null)
            {
                var item = (IItem)entity;

                impact.EntityId = entity.StaticId;
                // entity should lose its voxel intance if put into the inventory
                item.ModelInstance = null;

                if (charEntity.Inventory.PutItem(item))
                {
                    cursor.RemoveEntity(owner.EntityState.PickedEntityLink);
                    impact.Success = true;
                    return impact;
                }
                else
                {
                    impact.Message = "Unable to put item to the inventory";
                    return impact;
                }
            }
            impact.Message = "Expected CharacterEntity owner";
            return impact;
        }

        public override PickType CanPickBlock(BlockProfile blockProfile)
        {
            if (blockProfile.Id == WorldConfiguration.CubeId.Air)
                return PickType.Transparent;
            
            return PickType.Pick;
        }
    }
}
