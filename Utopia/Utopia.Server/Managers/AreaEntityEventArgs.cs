using System;
using Utopia.Server.Structs;

namespace Utopia.Server.Managers
{
    public class AreaEntityEventArgs : EventArgs
    {
        public ServerDynamicEntity Entity { get; set; }
    }
}
