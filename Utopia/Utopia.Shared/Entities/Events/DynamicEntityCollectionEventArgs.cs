using System;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Events
{
    public class DynamicEntityCollectionEventArgs : EventArgs
    {
        public uint ParentDynamicEntityId { get; set; }
        public IDynamicEntity Entity { get; set; }
        public AbstractChunk Chunk { get; set; }
    }
}