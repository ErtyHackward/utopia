using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Buffers;
using S33M3Resources.VertexFormats.Interfaces;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Models.ModelMesh
{
    public class ModelMesh
    {
        [Flags]
        public enum ModelMeshComponents
        {
            P = 0x0,
            N = 0x1,
            T = 0x2,
            C = 0x4
        }
    }

    public class ModelMesh<VertexFormat, IndexFormat> : IModelMesh
        where VertexFormat : struct, IModelMeshComponents
        where IndexFormat : struct
    {
        public VertexBuffer<VertexFormat> VertexBuffer;
        public IndexBuffer<IndexFormat> IndexBuffer;
        public ModelMeshTexture Texture { get; set; }

        public void Set2Device(DeviceContext context)
        {
            VertexBuffer.SetToDevice(context, 0);
            IndexBuffer.SetToDevice(context, 0);
        }


        public int IndiceCount
        {
            get { return IndexBuffer.IndicesCount; }
        }
    }

    public interface IModelMesh
    {
        /// <summary>
        /// Push the mesh buffers to the device
        /// </summary>
        void Set2Device(DeviceContext context);
        ModelMeshTexture Texture { get; set; }
        int IndiceCount { get; }
    }
}
