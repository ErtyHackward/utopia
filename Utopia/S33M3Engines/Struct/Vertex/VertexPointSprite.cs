using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex.Helper;
using SharpDX.Direct3D11;
using Utopia.Shared.Structs;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX;

namespace S33M3Engines.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPointSprite : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector3 Position;
        public ByteColor Color;
        public ByteVector4 Info; //x = index Array, Y = Scale

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexPointSprite()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32_Float, InputElement.AppendAligned , 0),  
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("INFO", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned, 0),
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexPointSprite(Vector3 Position, ByteColor Color, ByteVector4 Info)
        {
            this.Color = Color;
            this.Position = Position;
            this.Info = Info;
        }
    }
}
