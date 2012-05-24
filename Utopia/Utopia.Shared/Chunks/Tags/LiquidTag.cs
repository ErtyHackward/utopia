using System.IO;
using Utopia.Shared.Chunks.Tags;

namespace Utopia.Shared.Chunks.Tags
{
    public class LiquidTag : BlockTag, ICubeYOffsetModifier
    {
        //YOffset : 0 = FULL cube, 1 = Empty cube
        public float YOffset
        {
            get { return (Pressure - 1) * -1; }
        }

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
