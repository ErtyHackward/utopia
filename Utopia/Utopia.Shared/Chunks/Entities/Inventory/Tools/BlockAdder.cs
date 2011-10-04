
namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{
    public class BlockAdder : Tool
    {
        public byte CubeId;

        public override int MaxStackSize
        {
            get { return 999; }
        }

        public override EntityClassId ClassId
        {
            get { return EntityClassId.BlockAdder; }
        }

        public override void Load(System.IO.BinaryReader reader)
        {
            base.Load(reader);
            CubeId = reader.ReadByte();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(CubeId);
        }
    }
}
