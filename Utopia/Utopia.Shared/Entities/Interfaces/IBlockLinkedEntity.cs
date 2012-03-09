using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Shared.Entities.Interfaces
{
    public interface IBlockLinkedEntity
    {
        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        Vector3I LinkedCube { get; set; }
    }
}
