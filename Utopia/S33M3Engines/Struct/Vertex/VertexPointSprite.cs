using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex.Helper;
using SharpDX.Direct3D11;
using Utopia.Shared.Structs;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace S33M3Engines.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPointSprite : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public ByteVector4 Position;
        public ByteVector4 Info;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexPointSprite()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned , 0),  
                                                            new InputElement("INFO", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned, 0),
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexPointSprite(byte textureArrayId, ByteVector4 Info)
        {
            this.Info = Info;
            this.Position = new ByteVector4();
            this.Position.W = textureArrayId;
        }
    }
}
