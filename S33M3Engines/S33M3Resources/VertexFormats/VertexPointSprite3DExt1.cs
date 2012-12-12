using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX;
using S33M3Resources.VertexFormats.Interfaces;
using S33M3Resources.Structs;
using S33M3DXEngine.VertexFormat;

namespace S33M3Resources.Structs.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPointSprite3DExt1 : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector4 Position; //XYZ : Position, W = Texture index Array
        public ByteColor Color;  //Color
        public ByteColor Color2; //XY = second color
        public Vector2 Size;     //XY = Size

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexPointSprite3DExt1()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned , 0),  
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("COLOR", 1, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("SIZE", 0, Format.R32G32_Float, InputElement.AppendAligned, 0)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexPointSprite3DExt1(Vector4 Position, ByteColor Color, ByteColor Color2 ,Vector2 Size)
        {
            this.Color = Color;
            this.Color2 = Color2;
            this.Position = Position;
            this.Size = Size;
        }
    }
}
