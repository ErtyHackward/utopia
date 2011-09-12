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
using RectangleF = System.Drawing.RectangleF;

namespace S33M3Engines.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexSpriteInstanced : IVertexType
    {
        public Matrix Tranform;
        public Color4 Color;
        public RectangleF SourceRect;
        public int TextureArrayIndex;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexSpriteInstanced(Matrix tranform, Color4 color, RectangleF sourceRect)
        {
            this.Tranform = tranform;
            this.Color = color;
            this.SourceRect = sourceRect;
            TextureArrayIndex = 0;
        }

        public VertexSpriteInstanced(Matrix tranform, Color4 color)
        {
            this.Tranform = tranform;
            this.Color = color;
            this.SourceRect = default(RectangleF);
            TextureArrayIndex = 0;
        }

        public VertexSpriteInstanced(Matrix tranform, Color4 color,int textureArrayIndex )
        {
            this.Tranform = tranform;
            this.Color = color;
            this.SourceRect = default(RectangleF);
            TextureArrayIndex = textureArrayIndex;
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

        static VertexSpriteInstanced()
        {
            // !!! The VertexDeclaration must incorporate the Fixed vertex Part !!!!
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0), 
                                                            new InputElement("TEXCOORD", 0, Format.R32G32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0), //Instead of 8 use AppendAligned
                                                            new InputElement("TRANSFORM", 0, Format.R32G32B32A32_Float, 0, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("TRANSFORM", 1, Format.R32G32B32A32_Float, 16, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("TRANSFORM", 2, Format.R32G32B32A32_Float, 32, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("TRANSFORM", 3, Format.R32G32B32A32_Float, 48, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 64, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("SOURCERECT", 0, Format.R32G32B32A32_Float, 80, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("TEXINDEX", 0, Format.R32_UInt, 96, 1, InputClassification.PerInstanceData, 1)
                                                                                                            
            };

            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
