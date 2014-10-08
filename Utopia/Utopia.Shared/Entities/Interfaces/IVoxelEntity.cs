using Utopia.Shared.Entities.Models;

namespace Utopia.Shared.Entities.Interfaces
{
    /// <summary>
    /// Represents an entity that have a visual voxel representation
    /// </summary>
    public interface IVoxelEntity : IEntity
    {
        /// <summary>
        /// Gets or sets current voxel model name
        /// </summary>
        string ModelName { get; set; }

        /// <summary>
        /// Gets or sets voxel model state to use
        /// </summary>
        string ModelState { get; set; }

        /// <summary>
        /// Gets or sets voxel model instance
        /// </summary>
        VoxelModelInstance ModelInstance { get; set; }
    }
}
