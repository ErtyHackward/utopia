using System.IO;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Torch : CubePlaceableItem, IBlockLinkedEntity
    {
        public override string ModelName
        {
            get { return "Torch"; }
        }

        /// <summary>
        /// Gets entity class id
        /// </summary>
        public override ushort ClassId
        {
            get { return EntityClassId.Torch; }
        }

        /// <summary>
        /// Gets maximum allowed number of items in one stack (set one if item is not stackable)
        /// </summary>
        public override int MaxStackSize
        {
            get { return 40; }
        }

        /// <summary>
        /// Gets item description
        /// </summary>
        public override string Description
        {
            get { return "Basic light source"; }
        }

        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        public Vector3I LinkedCube { get; set; }
        
        /// <summary>
        /// Gets or sets allowed faces where the entity can be mount on
        /// </summary>
        public BlockFace MountPoint
        {
            get { return BlockFace.Sides; }
        }

        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);
            LinkedCube = reader.ReadVector3I();
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(LinkedCube);
        }
    }
}
