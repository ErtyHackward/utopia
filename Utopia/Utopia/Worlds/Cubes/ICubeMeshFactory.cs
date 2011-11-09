using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.Chunks.Enums;
using Utopia.Shared.Structs;
using S33M3Engines.Struct.Vertex;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Enums;

namespace Utopia.Worlds.Cubes
{
    public interface ICubeMeshFactory
    {
        void GenCubeFace(ref TerraCube cube, CubeFaces cubeFace, ref ByteVector4 cubePosition, ref Vector3I cubePosiInWorld, VisualChunk chunk);
        bool FaceGenerationCheck(ref TerraCube cube, ref Vector3I cubePosiInWorld, CubeFaces cubeFace, ref TerraCube neightboorFaceCube, int seaLevel);
    }
}
