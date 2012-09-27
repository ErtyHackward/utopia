using System.ComponentModel;
using System.Drawing.Design;
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
        /// Gets or sets current voxel model name
        /// </summary>
        [Editor(typeof(ModelSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets voxel model instance
        /// </summary>
        [Browsable(false)]
        public VoxelModelInstance ModelInstance { get; set; }
        
        /// <summary>
        /// This is name can vary for concrete class instance (Example: Simon's steel shovel)
        /// </summary>
        public string UniqueName { get; set; }

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
        public EquipmentSlotType AllowedSlots { get; set; }

        /// <summary>
        /// Gets maximum allowed number of items in one stack (set one if item is not stackable)
        /// </summary>
        public int MaxStackSize { get; set; }
        
        /// <summary>
        /// Gets an item description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        public override string DisplayName
        {
            get { return UniqueName; }
        }

        #endregion

        public Item()
        {
            AllowedSlots = EquipmentSlotType.Hand;
        }

        // we need to override save and load!
        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);

            UniqueName = reader.ReadString();
            ModelName = reader.ReadString();
            Description = reader.ReadString();
            MaxStackSize = reader.ReadInt32();
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);

            if (UniqueName == null)
                UniqueName = "";

            writer.Write(UniqueName);
            writer.Write(ModelName);
            writer.Write(Description);
            writer.Write(MaxStackSize);
        }      
    }
}
