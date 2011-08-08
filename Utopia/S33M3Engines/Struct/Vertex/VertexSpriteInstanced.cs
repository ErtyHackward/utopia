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
    public struct VertexSpriteInstanced : IVertexType
    {
        public Matrix Tranform;
        public Color4 Color;
        public Vector4 SourceRect;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexSpriteInstanced(Matrix Tranform, Color4 Color, Vector4 SourceRect)
        {
            this.Tranform = Tranform;
            this.Color = Color;
            this.SourceRect = SourceRect;
        }

        public VertexSpriteInstanced(Matrix Tranform, Color4 Color)
        {
            this.Tranform = Tranform;
            this.Color = Color;
            this.SourceRect = default(Vector4);
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
                                                            new InputElement("SOURCERECT", 0, Format.R32G32B32A32_Float, 80, 1, InputClassification.PerInstanceData, 1)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
