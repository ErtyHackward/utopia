using System;
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
    public abstract class BlockTag : ICloneable
    {
        public abstract bool Equals(BlockTag tag);

        public abstract override int GetHashCode();

        public abstract bool RequireChunkMeshUpdate { get; }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public static bool operator ==(BlockTag tag1, BlockTag tag2)
        {
            if (ReferenceEquals(tag1, tag2))
                return true;
            if (ReferenceEquals(tag1, null))
                return false;
            if (ReferenceEquals(tag2, null))
                return false;
            if (tag1.GetType() != tag2.GetType())
                return false;

            return tag1.Equals(tag2);
        }

        public static bool operator !=(BlockTag tag1, BlockTag tag2)
        {
            return !(tag1 == tag2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BlockTag);
        }
    }
}