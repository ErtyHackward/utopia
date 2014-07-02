using System.Collections.Generic;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Entities.Sound;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Entities.Interfaces
{
    public interface IItem : IStaticEntity, IVoxelEntity
    {
        /// <summary>
        /// Gets possible slot types where the item can be put to
        /// </summary>
        EquipmentSlotType AllowedSlots { get; } 
  
        /// <summary>
        /// Gets the maximum number of items that can be put in a single slot
        /// </summary>
        int MaxStackSize { get; }

        /// <summary>
        /// Gets stack string. Entities with the same stack string will be possible to put together in a single slot
        /// </summary>
        string StackType { get; }

        /// <summary>
        /// Gets an item description
        /// </summary>
        string Description { get; }


        StaticEntitySoundSource PutSound { get; set; }

        StaticEntitySoundSource EmittedSound { get; set; }

        /// <summary>
        /// Gets maximum pick range, 0 means default
        /// </summary>
        float PickRange { get; set; }

        /// <summary>
        /// Gets maximum allowed number of items in one stack (set one if item is not stackable)
        /// </summary>
        List<ItemTransformation> Transformations { get; set; }
        
        /// <summary>
        /// Returns new entity position correspoding to the player
        /// </summary>
        /// <param name="owner">An entity wich trying to put the entity</param>
        /// <returns></returns>
        EntityPosition GetPosition(IDynamicEntity owner);

        /// <summary>
        /// Defines tool pick behaviour for the blocks
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns></returns>
        PickType CanPickBlock(BlockProfile blockProfile);

        /// <summary>
        /// Defines tool pick behaviour for the entities
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        PickType CanPickEntity(IEntity entity);

        /// <summary>
        /// Executes put operation
        /// Removes one item from the inventory and puts it into 
        /// the world
        /// </summary>
        /// <param name="owner">entity that runs the operation</param>
        /// <returns></returns>
        IToolImpact Put(IDynamicEntity owner, Item worldDroppedItem = null);
    }
}
