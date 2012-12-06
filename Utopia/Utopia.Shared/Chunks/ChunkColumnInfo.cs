using ProtoBuf;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Contains common information about column of blocks in the chunk
    /// </summary>
    [ProtoContract]
    public struct ChunkColumnInfo : IBinaryStorable
    {
        [ProtoMember(1)]
        public byte Temperature;
        [ProtoMember(2)]
        public byte Moisture;
        [ProtoMember(3)]
        public byte MaxHeight;
        [ProtoMember(4)]
        public byte MaxGroundHeight;
        [ProtoMember(5)]
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
