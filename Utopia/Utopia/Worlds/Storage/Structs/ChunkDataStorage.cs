using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;

namespace Utopia.Worlds.Storage.Structs
{
    public class ChunkDataStorage
    {
        public int ChunkX;
        public int ChunkZ;
        public long ChunkId;
        public Md5Hash Md5Hash;
        public byte[] CubeData;
    }
}
