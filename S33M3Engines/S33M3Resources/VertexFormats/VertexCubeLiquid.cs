using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX;
using S33M3Resources.VertexFormats.Interfaces;
using S33M3DXEngine.VertexFormat;
using S33M3Resources.Structs;

namespace S33M3Resources.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexCubeLiquid : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector4B Position;
        public ByteColor Color;
        public Vector4B VertexInfo1;
        public Vector4 VertexInfo2;  //X = Vertex Y offset; Y = Still not use

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexCubeLiquid()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned , 0),  
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("INFO", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned, 0),
                                                            new InputElement("INFO", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexCubeLiquid(ref Vector4B position, Byte textureArrayId, ref ByteColor lighting)
        {
            this.VertexInfo1 = new Vector4B();
            this.VertexInfo2 = new Vector4();
            this.Color = lighting;
            this.Position = position;
            this.Position.W = textureArrayId;
        }

        public VertexCubeLiquid(ref Vector4B position, Byte textureArrayId, ref ByteColor lighting, ref Vector4 vertexInfo2, ref Vector4B VertexInfo1)
        {
            this.VertexInfo1 = VertexInfo1;
            this.VertexInfo2 = vertexInfo2;
            this.Color = lighting;
            this.Position = position;
            this.Position.W = textureArrayId;
        }
    }
}
