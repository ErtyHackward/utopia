using System;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Structs;

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
        Md5Hash ModelHash { get; set; }
    }
}
