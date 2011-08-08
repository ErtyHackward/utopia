using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex.Helper;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3Engines.Windows;
using System.Globalization;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace S33M3Engines.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexSprite : IVertexType
    {
        public Vector2 Position;
        public Vector2 TextureCoordinate;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexSprite(Vector2 position, Vector2 textureCoordinate)
        {
            this.Position = position;
            this.TextureCoordinate = textureCoordinate;
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
            return Helpers.SmartGetHashCode(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{{2D Position:{0} TextureCoordinate:{2}}}", new object[] { this.Position, this.TextureCoordinate });
        }

        static VertexSprite()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0), 
                                                            new InputElement("TEXCOORD", 0, Format.R32G32_Float, InputElement.AppendAligned, 0) //Instead of 8 use AppendAligned
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
