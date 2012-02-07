using System;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Events
{
    public class VoxelModelEventArgs : EventArgs
    {
        public Md5Hash ModelHash { get; set; }
    }
}