using System;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Events
{
    public class EntityCollectionEventArgs : EventArgs
    {
        public uint ParentDynamicEntityId { get; set; }
        public IStaticEntity Entity { get; set; }
        public AbstractChunk Chunk { get; set; }
        public bool AtChunkCreationTime { get; set; }
    }
}