using System;
using Utopia.Net.Messages;
using Utopia.Shared.Chunks.Entities;

namespace Utopia.Server.Events
{
    public class AreaVoxelModelEventArgs : EventArgs
    {
        public VoxelEntity Entity { get; set; }
        public EntityVoxelModelMessage Message { get; set; }
    }
}