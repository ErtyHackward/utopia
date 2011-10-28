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
using Utopia.Shared.Structs;

namespace S33M3Engines.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexSpriteInstanced : IVertexType
    {
        public Matrix Tranform;
        public ByteColor Color;
        public RectangleF SourceRect;
        public int TextureArrayIndex;
        public float Depth;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexSpriteInstanced(Matrix tranform, ByteColor color, RectangleF sourceRect)
        {
            this.Tranform = tranform;
            this.Color = color;
            this.SourceRect = sourceRect;
            TextureArrayIndex = 0;
            Depth = 0;
        }

        public VertexSpriteInstanced(Matrix tranform, ByteColor color)
        {
            this.Tranform = tranform;
            this.Color = color;
            this.SourceRect = default(RectangleF);
            TextureArrayIndex = 0;
            Depth = 0;
        }

        public VertexSpriteInstanced(Matrix tranform, ByteColor color, int textureArrayIndex)
        {
            this.Tranform = tranform;
            this.Color = color;
            this.SourceRect = default(RectangleF);
            TextureArrayIndex = textureArrayIndex;
            Depth = 0;
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
                                                            new InputElement("TRANSFORM", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("TRANSFORM", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("TRANSFORM", 2, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("TRANSFORM", 3, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("SOURCERECT", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("TEXINDEX", 0, Format.R32_UInt, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("DEPTH", 0, Format.R32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1)
                                                                                                            
            };

            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
