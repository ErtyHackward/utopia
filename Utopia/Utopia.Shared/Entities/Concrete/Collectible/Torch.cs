using System.IO;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Torch : CubePlaceableItem
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
        /// Gets or sets allowed faces where the entity can be mount on
        /// </summary>
        public override BlockFace MountPoint
        {
            get { return BlockFace.Sides; }
        }
    }
}
