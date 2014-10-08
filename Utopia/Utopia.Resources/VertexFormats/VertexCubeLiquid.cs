using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX;
using S33M3Resources.VertexFormats.Interfaces;
using S33M3DXEngine.VertexFormat;
using S33M3Resources.Structs;

namespace Utopia.Resources.VertexFormats
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexCubeLiquid : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector4B Position;     // X = XPosi, Y = YPosi, Z = ZPosi,  W =  Y Modified block Height modificator
        public ByteColor Color;
        public Vector4B VertexInfo1;  // x = FaceType, (bool)y = is Upper vertex, Z = Biome Texture Id,
        public Vector4B VertexInfo2;  // x = Moisture, y = Temperature, z = animation Speed, w = Animation NbrFrames
        public ushort ArrayId;
        public ushort Dummy;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexCubeLiquid()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned , 0),  
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("INFO", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned, 0),
                                                            new InputElement("BIOMEINFO", 0, Format.R8G8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("ANIMATION", 0, Format.R8G8_UInt, InputElement.AppendAligned, 0),
                                                            new InputElement("ARRAYID", 0, Format.R16_UInt, InputElement.AppendAligned, 0),
                                                            new InputElement("DUMMY", 0, Format.R16_UInt, InputElement.AppendAligned, 0)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexCubeLiquid(ref Vector4B position, int textureArrayId, ref ByteColor lighting, ref Vector4B vertexInfo2, ref Vector4B VertexInfo1)
        {
            this.VertexInfo1 = VertexInfo1;
            this.VertexInfo2 = vertexInfo2;
            this.Color = lighting;
            this.Position = position;
            Dummy = 0;
            this.ArrayId = (ushort)textureArrayId; 
        }
    }
}
