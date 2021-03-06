﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX;
using S33M3Resources.VertexFormats.Interfaces;
using S33M3Resources.Structs;
using S33M3DXEngine.VertexFormat;

namespace S33M3Resources.Structs.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPointSprite3DTexCoord : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Matrix WorldTransform;
        public Vector4 Position; //XYZ : Position, W = Texture index Array
        public Vector4 TextCoordU;
        public Vector4 TextCoordV;
        public ByteColor Color;  //Color
        public Vector2 Size;     //XY = Size


        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexPointSprite3DTexCoord()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("TRANSFORM", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0), //World Matrix Row0
                                                            new InputElement("TRANSFORM", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0), //World Matrix Row1
                                                            new InputElement("TRANSFORM", 2, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0), //World Matrix Row2
                                                            new InputElement("TRANSFORM", 3, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0), //World Matrix Row3
                                                            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned , 0), 
                                                            new InputElement("TEXC", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned , 0),
                                                            new InputElement("TEXC", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned , 0),
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("SIZE", 0, Format.R32G32_Float, InputElement.AppendAligned, 0)
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexPointSprite3DTexCoord(ref Matrix worldTranform, ref Vector4 Position, ref ByteColor Color, ref Vector2 Size, ref Vector4 TextCoordU, ref Vector4 TextCoordV)
        {
            this.WorldTransform = worldTranform;
            this.Color = Color;
            this.Position = Position;
            this.Size = Size;
            this.TextCoordU = TextCoordU;
            this.TextCoordV = TextCoordV;
        }
    }
}
