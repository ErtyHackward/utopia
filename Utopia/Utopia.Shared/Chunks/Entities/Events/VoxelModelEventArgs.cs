using System;

namespace Utopia.Shared.Chunks.Entities.Events
{
    public class VoxelModelEventArgs : EventArgs
    {
        public VoxelModel Model { get; set; }
    }
}