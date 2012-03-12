using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using System.Globalization;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using S33M3_Resources.VertexFormats.Interfaces;
using S33M3_Resources.Structs.Helpers;
using S33M3_Resources.Structs;
using S33M3_DXEngine.VertexFormat;

namespace S33M3_Resources.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPosition4Color : IVertexType
    {
        public Vector4 Position;
        public ByteColor Color;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPosition4Color(Vector4 position, ByteColor color)
        {
            this.Position = position;
            this.Color = color;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        public override int GetHashCode()
        {
            return SmartGetHashCode.Get(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{{Position:{0} Color:{1}}}", new object[] { this.Position, this.Color });
        }

        static VertexPosition4Color()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0), 
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 16, 0)
                                                            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }

}
