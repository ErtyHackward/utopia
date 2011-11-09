using System;
using Utopia.Shared.Entities.Events;

namespace Utopia.Shared.Entities.Interfaces
{
    public interface IVoxelEntity : IEntity
    {
        /// <summary>
        /// Occurs when entity voxel model was changed
        /// </summary>
        event EventHandler<VoxelModelEventArgs> VoxelModelChanged;

        VoxelModel Model { get; }

        void CommitModel();
    }
}
