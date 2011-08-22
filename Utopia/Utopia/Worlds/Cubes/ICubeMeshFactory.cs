using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.Chunks.Enums;
using Utopia.Shared.Structs;
using S33M3Engines.Struct.Vertex;
using Utopia.Worlds.Chunks;

namespace Utopia.Worlds.Cubes
{
    public interface ICubeMeshFactory
    {
        void GenCubeFace(byte cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, VisualChunk chunk);
    }
}
