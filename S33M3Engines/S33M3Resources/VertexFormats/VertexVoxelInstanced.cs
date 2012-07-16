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
    public struct VertexVoxelInstanced : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector4B Position; // x,y,z and color index
        public Vector4B FaceType; // facetype, light

        public VertexVoxelInstanced(Vector4B pos, byte faceType, byte light)
        {
            Position = pos;
            FaceType = new Vector4B((int)faceType, light, 0, 0);
        }

        static VertexVoxelInstanced()
        {
            var elements = new[] 
            { 
                new InputElement("POSITION",  0, Format.R8G8B8A8_UInt,      0,                          0, InputClassification.PerVertexData,   0), 
                new InputElement("INFO",      0, Format.R8G8B8A8_UInt,      InputElement.AppendAligned, 0, InputClassification.PerVertexData,   0),
                new InputElement("TRANSFORM", 0, Format.R32G32B32A32_Float, 0,                          1, InputClassification.PerInstanceData, 1), //World Matrix Row0
                new InputElement("TRANSFORM", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1), //World Matrix Row1
                new InputElement("TRANSFORM", 2, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1), //World Matrix Row2
                new InputElement("TRANSFORM", 3, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1), //World Matrix Row3
                new InputElement("COLOR",     0, Format.R32G32B32_Float,    InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1)
            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }
}
