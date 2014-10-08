using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX;
using S33M3Resources.Structs;
using S33M3Resources.VertexFormats.Interfaces;
using S33M3DXEngine.VertexFormat;

namespace Utopia.Resources.VertexFormats
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexCubeSolid : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector4B Position; // X = XPosi, Y = YPosi, Z = ZPosi
        public ByteColor Color;
        public Vector4B VertexInfo;  //(bool)x = is Upper vertex;  y = facetype, z = not used, w = Offset
        public Vector4B BiomeInfo;   //X = Moisture, Y = Temperature, Z = ArrayTextureID for Biome, W SideOffset multiplier
        public Vector4B Animation;   // X = Speed, Y = NbrFrames
        public ushort ArrayId;
        public ushort Dummy;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexCubeSolid()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned , 0),  
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("INFO", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned, 0),
                                                            new InputElement("BIOMEINFO", 0, Format.R8G8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("VARIOUS", 0, Format.R8G8_UInt, InputElement.AppendAligned, 0),
                                                            new InputElement("ANIMATION", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned, 0),
                                                            new InputElement("ARRAYID", 0, Format.R16_UInt, InputElement.AppendAligned, 0),
                                                            new InputElement("DUMMY", 0, Format.R16_UInt, InputElement.AppendAligned, 0)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }


        public VertexCubeSolid(ref Vector4B position, int textureArrayId, ref ByteColor lighting, ref Vector4B vertexInfo, ref Vector4B biomeInfo, byte animationSpeed, byte maxAnimationFrame)
        {
            this.VertexInfo = vertexInfo;
            this.Color = lighting;
            this.Position = position;
            this.ArrayId = (ushort)textureArrayId;
            this.BiomeInfo = biomeInfo;
            Dummy = 0;
            Animation = new Vector4B() { X = animationSpeed, Y = maxAnimationFrame };
        }
    }
}
