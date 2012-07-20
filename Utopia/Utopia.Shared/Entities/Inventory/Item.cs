using System.IO;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents any lootable voxelEntity, tool, weapon, armor, collectible. This entity can be put into the inventory
    /// </summary>
    public abstract class Item : StaticEntity, IItem, IVoxelEntity
    {
        #region Properties

        /// <summary>
        /// Gets current voxel model name
        /// </summary>
        public abstract string ModelName { get; }

        /// <summary>
        /// Gets or sets voxel model instance
        /// </summary>
        public VoxelModelInstance ModelInstance { get; set; }
        
        /// <summary>
        /// This is name can vary for concrete class instance (Example: Simon's steel shovel)
        /// </summary>
        public virtual string UniqueName { get; set; }

        /// <summary>
        /// Gets stack string. Entities with the same stack string will be possible to put together in a single slot
        /// </summary>
        public virtual string StackType
        {
            get { return GetType().Name; }            
        }

        /// <summary>
        /// Gets possible slot types where the item can be put to
        /// </summary>
        public virtual EquipmentSlotType AllowedSlots
        {
            get { return EquipmentSlotType.None; }
        }

        /// <summary>
        /// Gets maximum allowed number of items in one stack (set one if item is not stackable)
        /// </summary>
        public abstract int MaxStackSize { get; }
        
        /// <summary>
        /// Gets an item description
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        public override string DisplayName
        {
            get { return UniqueName; }
        }

        #endregion

        // we need to override save and load!
        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);

            UniqueName = reader.ReadString();
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);

            if (UniqueName == null)
                UniqueName = "";

            writer.Write(UniqueName);
        }      
    }
}
