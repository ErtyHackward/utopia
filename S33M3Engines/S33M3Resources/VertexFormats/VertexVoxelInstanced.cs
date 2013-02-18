using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using S33M3Resources.VertexFormats.Interfaces;
using S33M3DXEngine.VertexFormat;
using S33M3Resources.Structs;

namespace S33M3Resources.Structs.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexVoxelInstanced
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector4B Position; // x,y,z and color index
        public Vector4B FaceType; // facetype, light

        public VertexVoxelInstanced(Vector4B pos, byte faceType, byte light)
        {
            Position = pos;
            FaceType = new Vector4B((int)faceType, light, 0, 0);
        }
              
    }
}
