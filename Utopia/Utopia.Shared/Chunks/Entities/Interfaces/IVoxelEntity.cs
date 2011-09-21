
using System;
using Utopia.Shared.Chunks.Entities.Events;

namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    public interface IVoxelEntity : IEntity
    {
        /// <summary>
        /// Occurs when entity voxel model was changed
        /// </summary>
        event EventHandler<VoxelModelEventArgs> VoxelModelChanged;

        VoxelModel Model
        {
            get;
            set;
        }
    }
}
