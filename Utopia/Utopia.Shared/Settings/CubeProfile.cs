using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Settings
{
    //public partial struct CubeProfile
    //{
    //    public delegate bool CanGenerateCubeFaceDelegate(ref TerraCube cube, ref Vector3I cubelocation, CubeFace cubeFace, ref TerraCube neightboorFaceCube, int seaLevel);
    //    public CanGenerateCubeFaceDelegate CanGenerateCubeFace;

    //    public delegate void GenerateSolidMesh(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Vector3I cubePosiInWorld, List<VertexCubeLiquid> vertices, Dictionary<long, int> verticesDico, List<ushort> indices);
    //    public GenerateSolidMesh CreateSolidCubeMesh;

    //    public delegate void GenerateLiquidMesh(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Vector3I cubePosiInWorld, List<VertexCubeLiquid> vertices, Dictionary<long, int> verticesDico, List<ushort> indices);
    //    public GenerateLiquidMesh CreateLiquidCubeMesh;

    //    public string Name;
    //    public byte Id; //Represent the ID of the cube and it's linked texture in the array
    //    public bool IsPickable;
    //    public bool IsBlockingLight;
    //    public bool IsSeeThrough;
    //    public bool IsBlockingWater;
    //    public bool IsFloodPropagation;
    //    public bool IsSolidToEntity;
    //    public bool IsEmissiveColorLightSource;
    //    public bool IsFlooding;
    //    public int FloodingPropagationPower;
    //    public Color EmissiveColor;
    //    public float YBlockOffset;
    //    //Texture id foreach face
    //    public byte Tex_Front, Tex_Back, Tex_Left, Tex_Right, Tex_Top, Tex_Bottom;
    //    public byte[] Textures = new byte[6];
    //}
}
