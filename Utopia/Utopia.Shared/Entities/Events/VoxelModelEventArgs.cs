using System;
using Utopia.Shared.Entities.Models;

namespace Utopia.Shared.Entities.Events
{
    public class VoxelModelEventArgs : EventArgs
    {
        public VoxelModel Model { get; set; }
    }
}