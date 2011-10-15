using System;

namespace Utopia.Shared.Entities.Events
{
    public class VoxelModelEventArgs : EventArgs
    {
        public VoxelModel Model { get; set; }
    }
}