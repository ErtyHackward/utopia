
using Utopia.Shared.Chunks.Entities.Concrete;
using SharpDX;
using System;
namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// Represents any lootable voxelEntity, tool, weapon, armor, collectible
    /// </summary>
    public class Item : VoxelEntity
    {
  
        //FIXME icon stuff should probably not be here , rendering the voxel model as a 2d icon would be better,
        // but for now I need to port the XNA UI code
        public static int IconSize = 64;
        public SharpDX.Direct3D11.Texture2D Icon { get; set; }
        public Nullable<Rectangle> IconSourceRectangle { get; set; }
  
        /// <summary>
        /// Gets or sets tool wear
        /// </summary>
        public byte Durability { get; set; }

        /// <summary>
        /// This is name can vary for concrete class instance (Example: Simon's steel shovel)
        /// </summary>
        public string UniqueName { get; set; }

        public InventorySlot AllowedSlots { get; set;}


        
           
           // we need to override save and load!

        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);

            Durability = reader.ReadByte();
            UniqueName = reader.ReadString();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);

            writer.Write(Durability);
            writer.Write(UniqueName);
        }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        public override string DisplayName
        {
            get { return UniqueName; }
        }

        
    }
}
