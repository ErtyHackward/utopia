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
    public struct VertexPointSprite3DTexCoord : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector4 Position; //XYZ : Position, W = Texture index Array
        public ByteColor Color;  //Color
        public Vector3 Info;     //XY = Size, Z Billboard Type (0 = Entity facing, 1 = Entity View facing)
        public Vector4 TextCoordU;
        public Vector4 TextCoordV;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexPointSprite3DTexCoord()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned , 0),  
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("INFO", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0),
                                                            new InputElement("TEXC", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned , 0),
                                                            new InputElement("TEXC", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned , 0)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexPointSprite3DTexCoord(Vector4 Position, ByteColor Color, Vector3 Info, Vector4 TextCoordU, Vector4 TextCoordV)
        {
            this.Color = Color;
            this.Position = Position;
            this.Info = Info;
            this.TextCoordU = TextCoordU;
            this.TextCoordV = TextCoordV;
        }
    }
}
