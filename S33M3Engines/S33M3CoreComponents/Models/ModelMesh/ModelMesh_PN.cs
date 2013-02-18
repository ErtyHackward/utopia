using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using S33M3DXEngine.VertexFormat;
using S33M3Resources.Structs.Helpers;
using S33M3Resources.VertexFormats.Interfaces;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace S33M3CoreComponents.Models.ModelMesh
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct ModelMesh_PN : IModelMeshComponents
    {
        //Data Layout
        public Vector3 Position;
        public Vector3 Normal;

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
        public static readonly VertexDeclaration VertexDeclaration;
        ModelMesh.ModelMeshComponents IModelMeshComponents.Components { get { return Components; } }
        public static readonly ModelMesh.ModelMeshComponents Components = ModelMesh.ModelMeshComponents.P | ModelMesh.ModelMeshComponents.N;

        //Input elements definition
        static ModelMesh_PN()
        {
            InputElement[] elements = new InputElement[] { 
                                                            new InputElement("POSITION", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0), 
                                                            new InputElement("NORMAL", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0), 
                                                         };
            VertexDeclaration = new VertexDeclaration(elements);
        }

    }
}
