﻿using System.Linq;
using ProtoBuf;
using Utopia.Shared.Tools.BinarySerializer;
using System.Collections.Generic;

namespace Utopia.Shared.Chunks
{
    [ProtoContract]
    public class ChunkMetaData : IBinaryStorable
    {
        [ProtoMember(1)]
        public byte ChunkMasterBiomeType;
        [ProtoMember(2)]
        public byte ChunkMaxHeightBuilt;
        [ProtoMember(3)]
        public bool IsWild;
        [ProtoMember(4, OverwriteList = true)]
        public List<ChunkSpawnableEntity> SpawnableEntities;

        public ChunkMetaData()
        {
            SpawnableEntities = new List<ChunkSpawnableEntity>();
        }

        public ChunkMetaData(ChunkMetaData copyFrom)
        {
            ChunkMasterBiomeType = copyFrom.ChunkMasterBiomeType;
            ChunkMaxHeightBuilt = copyFrom.ChunkMaxHeightBuilt;
            IsWild = copyFrom.IsWild;
            SpawnableEntities = new List<ChunkSpawnableEntity>(copyFrom.SpawnableEntities);
        }

        public void setChunkMaxHeightBuilt(ChunkColumnInfo[] columnsInfo)
        {
            ChunkMaxHeightBuilt = columnsInfo.Max(x => x.MaxHeight);
        }

        public void setChunkWildStatus(ChunkColumnInfo[] columnsInfo)
        {
            IsWild = (columnsInfo.Count(x => x.IsWild) / (float)columnsInfo.Length) > 0.6;
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
