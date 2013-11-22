using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Structs;

namespace Utopia.Worlds.Storage.Structs
{
    public class ChunkDataStorage
    {
        public Vector3I ChunkPos;
        public Md5Hash Md5Hash;
        public byte[] CubeData;
    }
}
