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

        public ChunkMetaData()
        {
            
        }

        public ChunkMetaData(ChunkMetaData copyFrom)
        {
            ChunkMasterBiomeType = copyFrom.ChunkMasterBiomeType;
        }

        public void Save(System.IO.BinaryWriter writer)
        {
            writer.Write(ChunkMasterBiomeType);
        }

        public void Load(System.IO.BinaryReader reader)
        {
            ChunkMasterBiomeType = reader.ReadByte();
        }
    }
}
