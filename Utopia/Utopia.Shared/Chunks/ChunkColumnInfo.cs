using ProtoBuf;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Contains common information about column of blocks in the chunk
    /// </summary>
    [ProtoContract]
    public struct ChunkColumnInfo
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
        [ProtoMember(6)]
        public byte Zone;
        [ProtoMember(7)]
        public bool IsWild;
    }
}
