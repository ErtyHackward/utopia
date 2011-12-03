using System;
using System.Runtime.InteropServices;
using S33M3Engines.Struct.Vertex.Helper;
using SharpDX;
using S33M3Engines.Windows;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace S33M3Engines.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPosition : IVertexType
    {
        public Vector3 Position;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPosition(Vector3 position)
        {
            this.Position = position;
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

        static VertexPosition()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0), 
                                                            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
