using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using S33M3Resources.VertexFormats.Interfaces;
using SharpDX;
using S33M3Resources.Structs;
using S33M3DXEngine.VertexFormat;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace S33M3Resources.VertexFormats
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexColumnChart : IVertexType
    {
        public Vector4 Tranform;
        public Color4 Color;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexColumnChart(Vector4 tranform, Color4 color)
        {
            this.Tranform = tranform;
            this.Color = color;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        static VertexColumnChart()
        {
            // !!! The VertexDeclaration must incorporate the Fixed vertex Part !!!!
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0), 
                                                            new InputElement("TRANSFORM", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1), //X, Y => Location, Z = Scale on Y
                                                            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                                                                                                            
            };

            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
