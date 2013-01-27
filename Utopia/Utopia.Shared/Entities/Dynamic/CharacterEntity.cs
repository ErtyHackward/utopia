using System;
using System.Linq;
using System.Collections.Generic;
using ProtoBuf;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Provides character base properties. Character entity has an equipment and inventory. It can wear the tool.
    /// </summary>
    [ProtoContract]
    public abstract class CharacterEntity : DynamicEntity, ICharacterEntity, IWorldIntercatingEntity
    {
        /// <summary>
        /// Gets character name
        /// </summary>
        [ProtoMember(1)]
        public string CharacterName { get; set; }

        /// <summary>
        /// Gets character equipment
        /// </summary>
        [ProtoMember(2)]
        public CharacterEquipment Equipment { get; private set; }

        /// <summary>
        /// Gets character inventory
        /// </summary>
        [ProtoMember(3)]
        public SlotContainer<ContainedSlot> Inventory { get; private set; }

        /// <summary>
        /// Gets current health points of the entity
        /// </summary>
        [ProtoMember(4)]
        public int Health { get; set; }

        /// <summary>
        /// Gets maximum health point of the entity
        /// </summary>
        [ProtoMember(5)]
        public int MaxHealth { get; set; }

        /// <summary>
        /// Gets entityFactory, this field is injected
        /// </summary>
        public EntityFactory EntityFactory { get; set; }

        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        public ILandscapeManager2D LandscapeManager { get; set; }

        /// <summary>
        /// Gets dynamic entity manager, this field is injected
        /// </summary>
        public IDynamicEntityManager DynamicEntityManager { get; set; }

        /// <summary>
        /// Indicates if this charater controlled by real human
        /// </summary>
        public bool IsRealPlayer { get; set; }
        
        protected CharacterEntity()
        {
            Equipment = new CharacterEquipment(this);
            Inventory = new SlotContainer<ContainedSlot>(this, new S33M3Resources.Structs.Vector2I(7,5));

            // we need to have single id scope with two of these containers
            Equipment.JoinIdScope(Inventory);
            Inventory.JoinIdScope(Equipment);
        }
        
        /// <summary>
        /// Returns tool that can be used
        /// </summary>
        /// <param name="staticId">tool static id</param>
        /// <returns>Tool instance or null</returns>
        public IItem FindItemById(uint staticId)
        {
            if (Equipment.RightTool != null && Equipment.RightTool.StaticId == staticId)
                return Equipment.RightTool;

            var slot = Inventory.Find(staticId);

            return slot != null ? slot.Item : null;
        }

        /// <summary>
        /// Performs search in Inventory and Equipment
        /// </summary>
        /// <returns></returns>
        public ContainedSlot FindSlot(Func<ContainedSlot, bool> pred)
        {
            var slot = Equipment.FirstOrDefault(pred);

            if (slot != null)
                return slot;

            slot = Inventory.FirstOrDefault(pred);

            return slot;
        }

        public IEnumerable<ContainedSlot> FindAll(Func<ContainedSlot, bool> pred)
        {
            foreach (var slot in Equipment.Where(pred))
            {
                yield return slot;
            }

            foreach (var slot in Inventory.Where(pred))
            {
                yield return slot;
            }
        }

        public bool TakeItems(ushort blueprintId, int count)
        {
            while (count > 0)
            {
                var slot = Inventory.LastOrDefault(s => s.Item.BluePrintId == blueprintId);

                if (slot == null)
                    break;

                var takeItems = Math.Min(slot.ItemsCount, count);

                Inventory.TakeItem(slot.GridPosition, takeItems);

                count -= takeItems;
            }

            while (count > 0)
            {
                var slot = Equipment.LastOrDefault(s => s.Item.BluePrintId == blueprintId);

                if (slot == null)
                    break;

                var takeItems = Math.Min(slot.ItemsCount, count);

                Equipment.TakeItem(slot.GridPosition, takeItems);

                count -= takeItems;
            }

            return count == 0;
        }

        /// <summary>
        /// Fires entity use event for the craft operation
        /// </summary>
        /// <param name="recipeIndex"></param>
        public void CraftUse(int recipeIndex)
        {
            var args = EntityUseEventArgs.FromState(EntityState, this);
            args.UseType = UseType.Craft;
            args.RecipeIndex = recipeIndex;
            OnUse(args);
        }

        /// <summary>
        /// Creates the item from its recipe
        /// </summary>
        /// <param name="recipeIndex"></param>
        /// <returns></returns>
        public bool Craft(int recipeIndex)
        {
            try
            {
                var recipe = EntityFactory.Config.Recipes[recipeIndex];

                // check if we have all ingredients
                foreach (var ingredient in recipe.Ingredients)
                {
                    var count = FindAll(s => s.Item.BluePrintId == ingredient.BlueprintId).Sum(s => s.ItemsCount);
                    if (count < ingredient.Count)
                        return false;
                }

                foreach (var ingredient in recipe.Ingredients)
                {
                    if (!TakeItems(ingredient.BlueprintId, ingredient.Count))
                        return false;
                }
                
                var item = (Item)EntityFactory.CreateFromBluePrint(recipe.ResultBlueprintId);

                if (!Inventory.PutItem(item, recipe.ResultCount))
                    return false;

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
