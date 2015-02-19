using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.Chunks.Enums;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Enums;
using S33M3Resources.Structs;

namespace Utopia.Worlds.Cubes
{
    public interface ICubeMeshFactory
    {
        void GenCubeFace(ref TerraCube cube, CubeFaces cubeFace, ref Vector4B cubePosition, ref Vector3I cubePosiInWorld, VisualChunk2D chunk, ref TerraCube topCube, Dictionary<long, int> verticeDico);
        bool FaceGenerationCheck(ref TerraCube cube, ref Vector3I cubePosiInWorld, CubeFaces cubeFace, ref TerraCube NeightBorFaceCube);
    }
}
