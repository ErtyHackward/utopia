using System;

namespace Utopia.Shared.Chunks.Entities
{
    public class EntityCollectionEventArgs : EventArgs
    {
        public uint ParentEntityId { get; set; }
        public Entity Entity { get; set; }
        public AbstractChunk Chunk { get; set; }
    }
}