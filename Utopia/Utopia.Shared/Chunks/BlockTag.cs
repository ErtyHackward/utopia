using ProtoBuf;
using Utopia.Shared.Chunks.Tags;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Base class for block tags
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(LiquidTag))]
    [ProtoInclude(101, typeof(DamageTag))]
    public abstract class BlockTag
    {
        
    }
}