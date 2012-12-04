using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using S33M3DXEngine.VertexFormat;
using S33M3Resources.Structs;
using S33M3Resources.VertexFormats.Interfaces;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace S33M3CoreComponents.Particules
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexParticule : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector3 Position;
        public Vector4B Info; //x = index Array, Y = Size

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexParticule()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32_Float, InputElement.AppendAligned , 0),  
                                                            new InputElement("INFO", 0, Format.R8G8B8A8_UInt, InputElement.AppendAligned, 0),
                                                            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public VertexParticule(Vector3 Position, Vector4B Info)
        {
            this.Position = Position;
            this.Info = Info;
        }
    }
}
