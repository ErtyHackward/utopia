using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Chunks
{
    public class ChunkMetaData : IBinaryStorable
    {
        #region Private Variables
        #endregion

        #region Public Properties
        public byte ChunkMasterBiomeType;
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

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
