using System;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Models;

namespace Utopia.Shared.Entities.Interfaces
{
    public interface IVoxelEntity : IEntity
    {
        /// <summary>
        /// Occurs when entity voxel model was changed
        /// </summary>
        event EventHandler<VoxelModelEventArgs> VoxelModelChanged;

        /// <summary>
        /// Get current model md5 hash
        /// </summary>
        VoxelModelInstance ModelInstance { get; set; }

        /// <summary>
        /// Gets or sets current voxel model name
        /// </summary>
        string ModelName { get; set; }

        /// <summary>
        /// The entity can have a rnd rotation along its Y axis at creation time
        /// </summary>
        bool RndCreationYAxisRotation { get; set; }
    }
}
