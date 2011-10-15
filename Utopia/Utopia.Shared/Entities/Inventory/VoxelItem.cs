using System;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents any lootable voxelEntity, tool, weapon, armor, collectible. This entity can be put into the inventory
    /// </summary>
    public abstract class VoxelItem : VoxelEntity, IItem, IDisposable
    {   

        /// <summary>
        /// This is name can vary for concrete class instance (Example: Simon's steel shovel)
        /// </summary>
        public virtual string UniqueName { get; set; }

        public virtual string StackType
        {
            get { return this.GetType().Name; }            
        }

        public EquipmentSlotType AllowedSlots { get; set;}

        /// <summary>
        /// Gets maximum allowed number of items in one stack (set one if item is not stackable)
        /// </summary>
        public abstract int MaxStackSize { get; }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        public override string DisplayName
        {
            get { return UniqueName; }
        }
        
           
        // we need to override save and load!
        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);
            UniqueName = reader.ReadString();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(UniqueName);
        }


        public void Dispose()
        {           
        }
    }
}
