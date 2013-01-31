using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using S33M3Resources.VertexFormats.Interfaces;
using System.Drawing;
using SharpDX;
using S33M3DXEngine.VertexFormat;
using SharpDX.Direct3D11;
using S33M3Resources.Structs;
using SharpDX.DXGI;

namespace S33M3Resources.VertexFormats
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexHLSLLTree : IVertexType
    {
        public Vector4 Position;
        public ByteColor Color;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexHLSLLTree(Vector3 position, float colorshade, ByteColor color)
        {
            this.Position = new Vector4(position.X, position.Y, position.Z, colorshade);
            this.Color = color;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        static VertexHLSLLTree()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0), 
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0)
                                                            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
