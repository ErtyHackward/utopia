using System;

namespace Utopia.Shared.Chunks.Entities.Events
{
    public class EntityFactoryEventArgs : EventArgs
    {
        public Entity Entity { get; set; }
    }
}