using System;
using System.Runtime.InteropServices;
using S33M3DXEngine.VertexFormat;
using S33M3Resources.Structs.Helpers;
using S33M3Resources.VertexFormats.Interfaces;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace S33M3Resources.Structs.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPosition2Texture : IVertexType
    {
        public Vector2 Position;
        public Vector2 TextureCoordinate;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPosition2Texture(Vector2 position, Vector2 texCoord)
        {
            Position = position;
            TextureCoordinate = texCoord;
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

        static VertexPosition2Texture()
        {
            var elements = new[] { new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
                                   new InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0) };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}