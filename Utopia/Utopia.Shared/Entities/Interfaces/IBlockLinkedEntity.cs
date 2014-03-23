using System.ComponentModel;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Concrete.Interface;

namespace Utopia.Shared.Entities.Interfaces
{
    public interface IBlockLinkedEntity
    {
        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        Vector3I LinkedCube { get; set; }

        /// <summary>
        /// Gets or sets allowed faces where the entity can be mount on
        /// </summary>
        BlockFace MountPoint { get; }

        /// <summary>
        /// Indicates if this item has block link or not
        /// </summary>
        [Browsable(false)]
        bool Linked { get; set; }
    }
}
