using System;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Events
{
    public class EntityViewEventArgs : EventArgs
    {
        public IDynamicEntity Entity { get; set; }
    }
}
