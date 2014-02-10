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
    /// Allows to unmount any pickeable entity
    /// </summary>
    [ProtoContract]
    [Description("Allows to extract any kind of entities including a door or a chest.")]
    public class Extractor : Item, ITool
    {
        public override ushort ClassId
        {
            get { return EntityClassId.CubeResource; }
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            var impact = new ToolImpact();

            if (!owner.EntityState.IsEntityPicked)
            {
                impact.Message = "Entity should be picked to use";
                return impact;
            }

            if (owner.EntityState.PickedEntityLink.IsDynamic)
            {
                impact.Message = "Only static entities allowed to use";
                return impact;
            }

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
            // extractor can pick only entities
            if (blockProfile.Id == WorldConfiguration.CubeId.Air)
                return PickType.Transparent;
            
            return PickType.Stop;
        }
    }
}
