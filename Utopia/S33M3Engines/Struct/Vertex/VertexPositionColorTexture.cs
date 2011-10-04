﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using S33M3Engines.Struct.Vertex.Helper;
using SharpDX;
using S33M3Engines.Windows;
using System.Globalization;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Utopia.Shared.Structs;

namespace S33M3Engines.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColorTexture : IVertexType
    {
        public Vector3 Position;
        public ByteColor Color;
        public Vector3 TextureCoordinate;

        public static readonly VertexDeclaration VertexDeclaration;
        public VertexPositionColorTexture(Vector3 position, ByteColor color, Vector3 textureCoordinate)
        {
            this.Position = position;
            this.Color = color;
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
            return string.Format(CultureInfo.CurrentCulture, "{{Position:{0} Color:{1} TextureCoordinate:{2}}}", new object[] { this.Position, this.Color, this.TextureCoordinate });
        }


        static VertexPositionColorTexture()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32_Float,0 , 0), 
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 12, 0),
                                                            new InputElement("TEXCOORD", 0, Format.R32G32B32_Float, 16, 0)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }
    }

}
