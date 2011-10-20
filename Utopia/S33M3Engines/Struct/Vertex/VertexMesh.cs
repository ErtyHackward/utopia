using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3Engines.Struct.Vertex.Helper;
using S33M3Engines.Windows;
using System.Globalization;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace S33M3Engines.Struct.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexMesh : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexMesh(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
        {
            this.Position = position;
            this.Normal = normal;
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
            return string.Format(CultureInfo.CurrentCulture, "{{Position:{0} Normal:{1} TextureCoordinate:{2}}}", new object[] { this.Position, this.Normal, this.TextureCoordinate });
        }

        public static bool operator ==(VertexMesh left, VertexMesh right)
        {
            return (((left.Position == right.Position) && (left.Normal == right.Normal)) && (left.TextureCoordinate == right.TextureCoordinate));
        }

        public static bool operator !=(VertexMesh left, VertexMesh right)
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
            return (this == ((VertexMesh)obj));
        }

        static VertexMesh()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32_Float,0 , 0), 
                                                            new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                                                            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
                                                          };

            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
