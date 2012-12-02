using ProtoBuf;

namespace Utopia.Shared.Chunks.Tags
{
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

        public override byte Id
        {
            get { return 1; }
        }

        public override int GetHashCode()
        {
            return Pressure.GetHashCode() ^ LiquidType.GetHashCode() ^ Sourced.GetHashCode() ^ Id.GetHashCode();
        }
    }
}
