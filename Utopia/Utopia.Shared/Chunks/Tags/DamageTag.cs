using ProtoBuf;

namespace Utopia.Shared.Chunks.Tags
{
    /// <summary>
    /// Indicates that the block was damaged
    /// </summary>
    [ProtoContract]
    public class DamageTag : BlockTag
    {
        /// <summary>
        /// Gets the remaining strength of the block (0 or less means the block is destroyed)
        /// </summary>
        [ProtoMember(1)]
        public int Strength { get; set; }
    }
}