using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX;
using S33M3_Resources.VertexFormats.Interfaces;
using S33M3_DXEngine.VertexFormat;

namespace S33M3_Resources.Struct.Vertex
{

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexCubeCloud : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public float LayerNbr;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexCubeCloud()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                                                            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0),
                                                            new InputElement("VARIOUS", 0, Format.R32_Float, 20, 0)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexCubeCloud(ref Vector3 position, ref Vector2 textCoord, float LayerNbr)
        {
            this.Position = position;
            this.TextureCoordinate = textCoord;
            this.LayerNbr = LayerNbr;
        }

    }
}
