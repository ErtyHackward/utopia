using System;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Server.Managers
{
    public class AreaEntityEventArgs : EventArgs
    {
        public IDynamicEntity Entity { get; set; }
    }
}
