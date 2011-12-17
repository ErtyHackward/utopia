using System;
using System.Runtime.InteropServices;
using S33M3Engines.Struct.Vertex.Helper;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Utopia.Shared.Structs;

namespace S33M3Engines.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexVoxel : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public ByteVector4 Position;

        public VertexVoxel(ByteVector4 pos)
        {
            Position = pos;
        }

        static VertexVoxel()
        {
            var elements = new[] { 
                                    new InputElement("POSITION", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned , 0)
                                 };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }
}
