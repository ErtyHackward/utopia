using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX;
using S33M3Resources.VertexFormats.Interfaces;
using S33M3Resources.Structs;
using S33M3DXEngine.VertexFormat;

namespace S33M3Resources.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexCubeFaceLQ : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector4B Position;
        public HalfVector2 UV;
        public ByteColor Color;
        public HalfVector2 VertexInfo;  //X = Vertex Y offset; Y = Still not use

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexCubeFaceLQ()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned , 0),  
                                                            new InputElement("TEXCOORD", 0, Format.R16G16_Float, InputElement.AppendAligned , 0),
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("INFO", 0, Format.R16G16_Float, InputElement.AppendAligned, 0)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexCubeFaceLQ(ref Vector4B position, ref HalfVector2 UV, Byte textureArrayId, ref ByteColor lighting)
        {
            this.VertexInfo = new HalfVector2();
            this.Color = lighting;
            this.Position = position;
            this.Position.W = textureArrayId;
            this.UV = UV;
        }

        public VertexCubeFaceLQ(ref Vector4B position, ref HalfVector2 UV, Byte textureArrayId, ref ByteColor lighting, ref HalfVector2 vertexInfo)
        {
            this.VertexInfo = vertexInfo;
            this.Color = lighting;
            this.Position = position;
            this.Position.W = textureArrayId;
            this.UV = UV;
        }
    }
}
