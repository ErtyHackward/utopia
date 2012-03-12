using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using S33M3_Resources.VertexFormats.Interfaces;
using S33M3_DXEngine.VertexFormat;
using S33M3_Resources.Structs;

namespace S33M3_Resources.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexCubeFaceHQ : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector4B Position; //The W component will be use to represent the kind of cube face ( use in the GEO shader to build the face)
        public Byte TextureArrayId;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexCubeFaceHQ()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R8G8B8A8_UInt,0 , 0), 
                                                            new InputElement("TEXARRAYID", 0, Format.R8_UInt, 4, 0),
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexCubeFaceHQ(Vector4B position, Byte textureArrayId)
        {
            this.Position = position;
            this.TextureArrayId = textureArrayId;
        }
    }
}
