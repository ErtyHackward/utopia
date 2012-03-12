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
    public struct VertexSprite3D : IVertexType
    {
        public Vector4 Position; //w = Sprite corner type in case of billboard only, 0 otherwhile
        public ByteColor Color;
        public Vector3 TextureCoordinate;
        public Vector3 MetaData; //Mostly used by Billboard to contains the Size

        public static readonly VertexDeclaration VertexDeclaration;
        public VertexSprite3D(Vector4 position, ByteColor color, Vector3 textureCoordinate, Vector3 metaData)
        {
            this.Position = position;
            this.Color = color;
            this.TextureCoordinate = textureCoordinate;
            this.MetaData = metaData;
        }

        public VertexSprite3D(Vector4 position, ByteColor color, Vector3 textureCoordinate)
        {
            this.Position = position;
            this.Color = color;
            this.TextureCoordinate = textureCoordinate;
            this.MetaData = Vector3.Zero;
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

        static VertexSprite3D()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned , 0), 
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("TEXCOORD", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0),
                                                            new InputElement("METADATA", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }
    }

}


