using System;
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
    public struct VertexPositionColor : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPositionColor(Vector3 position, Color color)
        {
            this.Position = position;
            this.Color = color;
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
            return string.Format(CultureInfo.CurrentCulture, "{{Position:{0} Color:{1}}}", new object[] { this.Position, this.Color });
        }

        public static bool operator ==(VertexPositionColor left, VertexPositionColor right)
        {
            return ((left.Color == right.Color) && (left.Position == right.Position));
        }

        public static bool operator !=(VertexPositionColor left, VertexPositionColor right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            return (this == ((VertexPositionColor)obj));
        }

        static VertexPositionColor()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0), 
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 12, 0)
                                                            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }

}
