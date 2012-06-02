using System.IO;
using Utopia.Shared.Chunks.Tags;

namespace Utopia.Shared.Chunks.Tags
{
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
        public ushort Pressure;
        
        public byte LiquidType;

        public bool Sourced;

        public override byte Id
        {
            get { return 1; }
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(LiquidType);
            writer.Write(Pressure);
            writer.Write(Sourced);
        }

        public override void Load(BinaryReader reader)
        {
            LiquidType = reader.ReadByte();
            Pressure = reader.ReadUInt16();
            Sourced = reader.ReadBoolean();
        }
    }
}
