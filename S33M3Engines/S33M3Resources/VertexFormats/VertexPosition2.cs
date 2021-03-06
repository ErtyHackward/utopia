using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using S33M3Resources.VertexFormats.Interfaces;
using S33M3Resources.Structs.Helpers;
using S33M3DXEngine.VertexFormat;

namespace S33M3Resources.Structs.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPosition2 : IVertexType
    {
        public Vector2 Position;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPosition2(Vector2 position)
        {
            Position = position;
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

        static VertexPosition2()
        {
            var elements = new[] { new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0) };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}