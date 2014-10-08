using System;
using System.Windows.Forms;
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

        /// <summary>
        /// Gets max strength of the block
        /// </summary>
        [ProtoMember(2)]
        public int TotalStrength { get; set; }

        public override bool Equals(BlockTag tag)
        {
            var t = tag as DamageTag;

            if (ReferenceEquals(t, null))
                return false;

            return Strength == t.Strength && TotalStrength == t.TotalStrength;
        }

        public override int GetHashCode()
        {
            return Strength << 16 + TotalStrength;
        }

        /// <summary>
        /// Cracks are the separate part and no need to rebuild the mesh
        /// </summary>
        public override bool RequireChunkMeshUpdate
        {
            get { return false; }
        }
    }
}