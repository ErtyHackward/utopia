using System;
using S33M3Resources.Structs;
using Utopia.Shared.Server.Structs;

namespace Utopia.Shared.Server.Events
{
    public class EntityLeaveAreaEventArgs : EventArgs
    {
        public ServerDynamicEntity Entity { get; set; }
        public Vector3D PreviousPosition { get; set; }
    }
}