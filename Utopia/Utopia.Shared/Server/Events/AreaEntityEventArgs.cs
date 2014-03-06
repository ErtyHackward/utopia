using System;
using Utopia.Shared.Server.Structs;

namespace Utopia.Shared.Server.Events
{
    public class AreaEntityEventArgs : EventArgs
    {
        public ServerDynamicEntity Entity { get; set; }
    }
}
