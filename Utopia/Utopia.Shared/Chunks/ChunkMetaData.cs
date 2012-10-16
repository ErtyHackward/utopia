using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Chunks
{
    public class ChunkMetaData : IBinaryStorable
    {
        public byte ChunkMasterBiomeType;
        public byte ChunkMaxHeightBuilt;

        public ChunkMetaData()
        {
        }

        public ChunkMetaData(ChunkMetaData copyFrom)
        {
            ChunkMasterBiomeType = copyFrom.ChunkMasterBiomeType;
            ChunkMaxHeightBuilt = copyFrom.ChunkMaxHeightBuilt;
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
