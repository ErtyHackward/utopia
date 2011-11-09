using System;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Messages;

namespace Utopia.Server.Events
{
    public class AreaVoxelModelEventArgs : EventArgs
    {
        public IVoxelEntity Entity { get; set; }
        public EntityVoxelModelMessage Message { get; set; }
    }
}