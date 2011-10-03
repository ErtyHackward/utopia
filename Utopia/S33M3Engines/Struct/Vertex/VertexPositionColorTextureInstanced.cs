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
    public struct VertexPositionColorTextureInstanced : IVertexType
    {
        public Matrix WorldMatrix;
        public ByteColor Color;
        public uint TextureArrayIndex;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPositionColorTextureInstanced(ref Matrix tranform, ref ByteColor color, uint textureArrayIndex)
        {
            this.WorldMatrix = tranform;
            this.Color = color;
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
        static VertexPositionColorTextureInstanced()
        {
            // !!! The VertexDeclaration must incorporate the Fixed vertex Part !!!!
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0), 
                                                            new InputElement("TEXCOORD", 0, Format.R32G32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0), //Instead of 8 use AppendAligned
                                                            new InputElement("TRANSFORM", 0, Format.R32G32B32A32_Float, 0, 1, InputClassification.PerInstanceData, 1),  //World Matrix Row0
                                                            new InputElement("TRANSFORM", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1), //World Matrix Row1
                                                            new InputElement("TRANSFORM", 2, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1), //World Matrix Row2
                                                            new InputElement("TRANSFORM", 3, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1), //World Matrix Row3
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                                                            new InputElement("TEXINDEX", 0, Format.R32_UInt, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1)
            };

            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
