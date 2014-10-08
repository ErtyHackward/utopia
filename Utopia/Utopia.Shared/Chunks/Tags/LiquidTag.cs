using ProtoBuf;

namespace Utopia.Shared.Chunks.Tags
{
    /// <summary>
    /// Holds water amount information at the block
    /// </summary>
    [ProtoContract]
    public class LiquidTag : BlockTag, ICubeYOffsetModifier
    {
        //YOffset : 0 = FULL cube, 1 = Empty cube
        public float YOffset
        {
            get { return Pressure > 9 ? 0 : (10f - Pressure) / 10; }
        }

        /// <summary>
        /// Pressure of the liquid, 10 - full block
        /// </summary>
        [ProtoMember(1)]
        public ushort Pressure;

        [ProtoMember(2)]
        public byte LiquidType;

        [ProtoMember(3)]
        public bool Sourced;

        public override bool Equals(BlockTag tag)
        {
            var t = tag as LiquidTag;

            if (ReferenceEquals(t, null))
                return false;

            return Pressure == t.Pressure && LiquidType == t.LiquidType && Sourced == t.Sourced;
        }

        public override int GetHashCode()
        {
            return Pressure.GetHashCode() ^ LiquidType.GetHashCode() ^ Sourced.GetHashCode();
        }

        /// <summary>
        /// Liquid tag changes water block shape, so we need to rebuild the chunk
        /// </summary>
        public override bool RequireChunkMeshUpdate
        {
            get { return true; }
        }
    }
}
