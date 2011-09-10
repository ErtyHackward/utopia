using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using S33M3Engines.Struct.Vertex.Helper;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX;
using Utopia.Shared.Structs;

namespace S33M3Engines.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexCubeSolid : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public ByteVector4 Position;
        public ByteColor Color;
        public ByteVector4 VertexInfo;  //X = Vertex Y offset; Y = Still not use

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexCubeSolid()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned , 0),  
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("INFO", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned, 0)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexCubeSolid(ref ByteVector4 position, Byte textureArrayId, ref ByteColor lighting)
        {
            this.VertexInfo = new ByteVector4();
            this.Color = lighting;
            this.Position = position;
            this.Position.W = textureArrayId;
        }

        public VertexCubeSolid(ByteVector4 position, Byte textureArrayId, ref ByteColor lighting, ref ByteVector4 vertexInfo)
        {
            this.VertexInfo = vertexInfo;
            this.Color = lighting;
            this.Position = position;
            this.Position.W = textureArrayId;
        }

        public VertexCubeSolid(ref ByteVector4 position, Byte textureArrayId, ref ByteColor lighting, ref ByteVector4 vertexInfo)
        {
            this.VertexInfo = vertexInfo;
            this.Color = lighting;
            this.Position = position;
            this.Position.W = textureArrayId;
        }
    }
}
