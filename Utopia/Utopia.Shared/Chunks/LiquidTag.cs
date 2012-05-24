using System.IO;

namespace Utopia.Shared.Chunks
{
    public class LiquidTag : BlockTag
    {
        /// <summary>
        /// Pressure of the liquid, less than one values indicate non full block
        /// </summary>
        public float Pressure;
        
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
            Pressure = reader.ReadSingle();
            Sourced = reader.ReadBoolean();
        }
    }
}
