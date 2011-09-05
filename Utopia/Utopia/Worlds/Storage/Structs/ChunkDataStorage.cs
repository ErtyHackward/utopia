using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Worlds.Storage.Structs
{
    public class ChunkDataStorage
    {
        public int ChunkX;
        public int ChunkZ;
        public long ChunkId;
        public int md5;
        public byte[] CubeData;
    }
}
