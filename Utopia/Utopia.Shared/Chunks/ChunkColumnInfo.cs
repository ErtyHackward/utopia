using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Contains common information about column of blocks in the chunk
    /// </summary>
    public struct ChunkColumnInfo : IBinaryStorable
    {
        public byte Temperature;
        public byte Moisture;
        public byte MaxHeight;
        public byte MaxGroundHeight;
        public byte Biome;

        public void Save(System.IO.BinaryWriter writer)
        {
            writer.Write(Temperature);
            writer.Write(Moisture);
            writer.Write(MaxHeight);
            writer.Write(MaxGroundHeight);
            writer.Write(Biome);
        }

        public void Load(System.IO.BinaryReader reader)
        {
            Temperature = reader.ReadByte();
            Moisture = reader.ReadByte();
            MaxHeight = reader.ReadByte();
            MaxGroundHeight = reader.ReadByte();
            Biome = reader.ReadByte();
        }
    }
}
