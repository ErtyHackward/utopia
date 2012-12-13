using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using System.Globalization;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using S33M3Resources.VertexFormats.Interfaces;
using S33M3Resources.Structs.Helpers;
using S33M3Resources.Structs;
using S33M3DXEngine.VertexFormat;

namespace S33M3Resources.Structs.Vertex
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexCubeColor : IVertexType
    {

        public Vector4 Position; //Cube Local Position
        public Matrix Tranform;  //Cube Transformation (Scale, Rotation, Translation to World coord)
        public ByteColor Color;  //Cube Color
        public ByteColor AmbiantColor; //Color from environments

        public static readonly VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexType.VertexDeclaration {  get { return VertexDeclaration; }  }
        static VertexCubeColor()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0), 
                                                            new InputElement("TRANSFORM", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0), //Transform Matrix Row0
                                                            new InputElement("TRANSFORM", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0), //Transform Matrix Row1
                                                            new InputElement("TRANSFORM", 2, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0), //Transform Matrix Row2
                                                            new InputElement("TRANSFORM", 3, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0), //Transform Matrix Row3
                                                            new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0),
                                                            new InputElement("COLOR", 1, Format.R8G8B8A8_UNorm, InputElement.AppendAligned, 0)
                                                            };
            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexCubeColor(ref Vector4 position, ref ByteColor color, ref ByteColor ambiantColor, ref Matrix tranform)
        {
            this.Position = position;
            this.Color = color;
            this.AmbiantColor = ambiantColor;
            this.Tranform = Matrix.Transpose(tranform);
        }
    }

}
