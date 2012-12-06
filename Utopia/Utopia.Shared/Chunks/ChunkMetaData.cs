using System.Linq;
using ProtoBuf;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Chunks
{
    [ProtoContract]
    public class ChunkMetaData : IBinaryStorable
    {
        [ProtoMember(1)]
        public byte ChunkMasterBiomeType;
        [ProtoMember(2)]
        public byte ChunkMaxHeightBuilt;

        public ChunkMetaData()
        {
        }

        public ChunkMetaData(ChunkMetaData copyFrom)
        {
            ChunkMasterBiomeType = copyFrom.ChunkMasterBiomeType;
            ChunkMaxHeightBuilt = copyFrom.ChunkMaxHeightBuilt;
        }

        public void setChunkMaxHeightBuilt(ChunkColumnInfo[] columnsInfo)
        {
            ChunkMaxHeightBuilt = columnsInfo.Max(x => x.MaxHeight);
        }

        public void Save(System.IO.BinaryWriter writer)
        {
            writer.Write(ChunkMasterBiomeType);
            writer.Write(ChunkMaxHeightBuilt);
        }

        public void Load(System.IO.BinaryReader reader)
        {
            ChunkMasterBiomeType = reader.ReadByte();
            ChunkMaxHeightBuilt = reader.ReadByte();
        }
    }
}
