using System;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Shared.Chunks.Entities.Events
{
    public class EntityMoveEventArgs : EventArgs
    {
        public IDynamicEntity Entity { get; set; }
    }
}