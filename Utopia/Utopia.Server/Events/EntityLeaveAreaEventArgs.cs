using System;
using Utopia.Server.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Server.Events
{
    public class EntityLeaveAreaEventArgs : EventArgs
    {
        public ServerDynamicEntity Entity { get; set; }
        public Vector3D PreviousPosition { get; set; }
    }
}