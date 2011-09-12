using System;
using System.Collections.Generic;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Concrete;
using S33M3Engines.Shared.Math;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// VisualEntiy is a pure client side class that wraps a VoxelEntity.
    ///  it has all rendering related data, vertexBuffer / verticeList
    /// 
    /// Each item modification will not be sent to the server, only the final state so the altered property is on client side    
    /// 
    /// </summary>
    public class VisualEntity : IDisposable
    {
        public readonly VoxelEntity VoxelEntity;
        public VertexBuffer<VertexCubeSolid> VertexBuffer;
        public List<VertexCubeSolid> Vertice;
       
        /// <summary>
        /// Altered by server or user and needs vertice update (you need to call Update yourself)
        /// </summary>
        public Boolean Altered;

        private readonly VoxelMeshFactory _voxelMeshFactory;

        /// <summary>
        /// overlays are indices in a texture2dArray that will allow overdrawing a texture over the cube texture
        /// used for editors selection 
        /// </summary>
        private readonly byte[,,] _overlays;

        /// <summary>
        /// creates a VisualEntity ready to render with filled vertice List and vertexBuffer
        /// </summary>
        /// <param name="voxelMeshFactory">voxelMeshFactory responsible to create mesh</param>
        /// <param name="wrapped">wrapped VoxelEntity from server</param>
        /// <param name="overlays">array of texture id to overlay</param>
        public VisualEntity(VoxelMeshFactory voxelMeshFactory, VoxelEntity wrapped,byte[, ,] overlays=null)
        {
            VoxelEntity = wrapped;
            _voxelMeshFactory = voxelMeshFactory;
            _overlays = overlays;
            Vertice = _voxelMeshFactory.GenCubesFaces(VoxelEntity.Blocks, _overlays);
            VertexBuffer = _voxelMeshFactory.InitBuffer(Vertice);
            
            Altered = true;
            Update();
        }

        public DVector3 Position
        {
            get { return VoxelEntity.Position; }
            set { VoxelEntity.Position = value; }
        }

        public void Commit()
        {
            //send modified blocks back to server / disk storage
        }

        public void Dispose()
        {
            if (VertexBuffer != null) VertexBuffer.Dispose();
        }

        /// <summary>
        /// Update the mesh, regen cube faces, vertice, vertex buffer (dynamic resize happens if needed)
        /// No effect if Altered is false
        /// </summary>
        internal void Update()
        {
            if (!Altered) return; 

            Vertice = _voxelMeshFactory.GenCubesFaces(VoxelEntity.Blocks,_overlays);

            if (Vertice.Count != 0)
            {
                VertexBuffer.SetData(Vertice.ToArray());
            }

            Altered = false;
        }

        public void AlterOverlay(int x,int y, int z, byte overlay)
        {
            _overlays[x, y, z] = overlay;
            Altered = true;
        }
    }
}