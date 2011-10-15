using System;

namespace Utopia.Shared.Entities.Events
{
    public class EntityFactoryEventArgs : EventArgs
    {
        public Entity Entity { get; set; }
    }
}