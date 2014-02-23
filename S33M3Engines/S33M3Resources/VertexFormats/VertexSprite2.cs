using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using System.Globalization;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using S33M3Resources.VertexFormats.Interfaces;
using S33M3Resources.Structs.Helpers;
using S33M3DXEngine.VertexFormat;

namespace S33M3Resources.Structs.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexSprite2 : IVertexType
    {
        public Vector3 Position;
        public Vector3 TextureCoordinate;
        public ByteColor Color;
        public Vector4 Wrap;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexSprite2(Vector3 position, Vector3 textureCoordinate, ByteColor color) : 
            this(position, textureCoordinate, color, new Vector4())
        {
        }

        public VertexSprite2(Vector3 position, Vector3 textureCoordinate, ByteColor color, Vector4 wrap)
        {
            Position = position;
            TextureCoordinate = textureCoordinate;
            Color = color;
            Wrap = wrap;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }
        static VertexSprite2()
        {
            InputElement[] elements =
            { 
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0), 
                new InputElement("TEXCOORD", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0), 
                new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                new InputElement("VARIOUS", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0)
            };

            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}

