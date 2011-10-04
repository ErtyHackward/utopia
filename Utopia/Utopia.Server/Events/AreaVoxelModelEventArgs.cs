using System;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Net.Messages;

namespace Utopia.Server.Events
{
    public class AreaVoxelModelEventArgs : EventArgs
    {
        public VoxelEntity Entity { get; set; }
        public EntityVoxelModelMessage Message { get; set; }
    }
}