using System;
using Utopia.Server.Structs;

namespace Utopia.Server.Events
{
    public class AreaEntityEventArgs : EventArgs
    {
        public ServerDynamicEntity Entity { get; set; }
    }
}
