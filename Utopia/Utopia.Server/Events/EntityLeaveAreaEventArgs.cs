using System;
using S33M3Engines.Shared.Math;
using Utopia.Server.Structs;

namespace Utopia.Server.Events
{
    public class EntityLeaveAreaEventArgs : EventArgs
    {
        public ServerDynamicEntity Entity { get; set; }
        public DVector3 PreviousPosition { get; set; }
    }
}