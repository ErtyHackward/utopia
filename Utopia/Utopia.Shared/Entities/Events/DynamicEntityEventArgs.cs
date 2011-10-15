using System;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Events
{
    public class DynamicEntityEventArgs : EventArgs
    {
        public IDynamicEntity Entity { get; set; }
    }
}