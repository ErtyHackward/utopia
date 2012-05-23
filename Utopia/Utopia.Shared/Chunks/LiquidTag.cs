using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Chunks
{
    public class LiquidTag : IBinaryStorable
    {
        /// <summary>
        /// Pressure of the liquid, less than one values indicate non full block
        /// </summary>
        public float Pressure;
        
        public byte LiquidType;

        public bool Sourced;

        public void Save(System.IO.BinaryWriter writer)
        {
            writer.Write(LiquidType);
            writer.Write(Pressure);
            writer.Write(Sourced);
        }

        public void Load(System.IO.BinaryReader reader)
        {
            LiquidType = reader.ReadByte();
            Pressure = reader.ReadSingle();
            Sourced = reader.ReadBoolean();
        }
    }
}
