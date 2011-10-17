using System;
using Utopia.Shared.Chunks;

namespace Utopia.Shared.Entities.Events
{
    public class EntityCollectionEventArgs : EventArgs
    {
        public uint ParentEntityId { get; set; }
        public Entity Entity { get; set; }
        public AbstractChunk Chunk { get; set; }
    }
}