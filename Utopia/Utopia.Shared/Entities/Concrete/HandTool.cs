using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using S33M3CoreComponents.Maths;
using S33M3CoreComponents.Sound;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Settings;
using Utopia.Shared.Entities.Concrete.Interface;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Special tool used when no tool is set (character mode)
    /// </summary>
    [EditorHide]
    public class HandTool : Item, ITool, ISoundEmitterEntity
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

            //Trigger item activation (Make it play sound, open, ...)
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
            return TakeImpact(owner, entity, impact, cursor, this);
        }

        public static IToolImpact TakeImpact(IDynamicEntity owner, IStaticEntity entity, EntityToolImpact impact, ILandscapeCursor cursor, Item hostItem)
        {
            var charEntity = owner as CharacterEntity;

            if (charEntity != null)
            {
                var item = (IItem)entity;
                impact.EntityId = entity.StaticId;

                var playerBindedItem = entity as IOwnerBindable;
                if (playerBindedItem != null && playerBindedItem.DynamicEntityOwnerID != charEntity.DynamicId)
                {
                    impact.Message = "This item is not binded to you !";
                    return impact;
                }

                if (item.IsDestroyedOnWorldRemove)
                {
                    item.BeforeDestruction(charEntity);
                    cursor.RemoveEntity(owner.EntityState.PickedEntityLink);
                    impact.Success = true;
                    item.ModelInstance = null;
                    impact.Message = "Item has been destroyed";
                    return impact;
                }

                var putItems = new List<KeyValuePair<IItem, int>>();
                putItems.Add(new KeyValuePair<IItem, int>(item, 1));

                var growing = item as PlantGrowingEntity;
                if (growing != null)
                {
                    putItems.Clear();
                    foreach (var slot in growing.CurrentGrowLevel.HarvestSlots)
                    {
                        if (slot.BlueprintId != 0)
                        {
                            putItems.Add(
                                new KeyValuePair<IItem, int>((Item)hostItem.EntityFactory.CreateFromBluePrint(slot.BlueprintId),
                                    slot.Count));
                        }
                    }
                }

                if (item.Transformations != null)
                {
                    var random = new FastRandom(owner.EntityState.PickedEntityPosition.GetHashCode() ^ owner.EntityState.Entropy);
                    bool transformed = false;
                    foreach (var itemTransformation in item.Transformations)
                    {
                        if (random.NextDouble() < itemTransformation.TransformChance)
                        {
                            putItems.Clear();
                            item.Transformations = null;
                            foreach (var slot in itemTransformation.GeneratedItems)
                            {
                                putItems.Add(
                                    new KeyValuePair<IItem, int>((Item)hostItem.EntityFactory.CreateFromBluePrint(slot.BlueprintId),
                                        slot.Count));
                                transformed = true;
                            }
                            break;
                        }
                    }
                    if (!transformed)
                    {
                        // don't allow future transforms
                        item.Transformations = null;
                    }
                }

                //Try to put the item into the inventory
                if (charEntity.Inventory.PutMany(putItems))
                {
                    //If inside the inventory, then remove it from the world
                    var removedEntity = (Item)cursor.RemoveEntity(owner.EntityState.PickedEntityLink);
                    impact.Success = true;

                    // entity should lose its voxel intance if put into the inventory
                    removedEntity.ModelInstance = null;

                    if (hostItem.SoundEngine != null && hostItem.EntityFactory.Config.EntityTake != null)
                    {
                        hostItem.SoundEngine.StartPlay3D(hostItem.EntityFactory.Config.EntityTake, removedEntity.Position.AsVector3());
                    }

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
