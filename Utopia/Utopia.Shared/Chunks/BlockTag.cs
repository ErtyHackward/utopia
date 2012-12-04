using ProtoBuf;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Base class for block tags
    /// </summary>
    [ProtoContract]
    public abstract class BlockTag
    {
        public abstract byte Id { get; }
    }
}