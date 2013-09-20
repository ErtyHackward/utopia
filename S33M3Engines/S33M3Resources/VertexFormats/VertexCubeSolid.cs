﻿using System;
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

namespace S33M3Resources.Structs.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexCubeSolid : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector4B Position;
        public ByteColor Color;
        public Vector4B VertexInfo;  //X = Vertex Y offset; Y = texture index;Z=Additional Texture Index
        public Vector4B BiomeInfo;   //X = Temperature, Y = Humidity, Z = TextureArray

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
                                                            new InputElement("VARIOUS", 0, Format.R8G8_UInt, InputElement.AppendAligned, 0)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexCubeSolid(ref Vector4B position, Byte textureArrayId, ref ByteColor lighting, ref Vector4B biomeInfo)
        {
            this.VertexInfo = new Vector4B();
            this.Color = lighting;
            this.Position = position;
            this.Position.W = textureArrayId;
            this.BiomeInfo = biomeInfo;
        }

        public VertexCubeSolid(Vector4B position, Byte textureArrayId, ref ByteColor lighting, ref Vector4B vertexInfo, ref Vector4B biomeInfo)
        {
            this.VertexInfo = vertexInfo;
            this.Color = lighting;
            this.Position = position;
            this.Position.W = textureArrayId;
            this.BiomeInfo = biomeInfo;
        }

        public VertexCubeSolid(ref Vector4B position, Byte textureArrayId, ref ByteColor lighting, ref Vector4B vertexInfo, ref Vector4B biomeInfo)
        {
            this.VertexInfo = vertexInfo;
            this.Color = lighting;
            this.Position = position;
            this.Position.W = textureArrayId;
            this.BiomeInfo = biomeInfo;
        }
    }
}