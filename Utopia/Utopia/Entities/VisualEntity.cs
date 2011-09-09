using System;
using System.Collections.Generic;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Concrete;
using S33M3Engines.Shared.Math;
using S33M3Engines.D3D;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    ///  This Class is responsible for the Entity Rendering into the world 
    /// </summary>
    public class VisualEntity : IDrawable, IDisposable
    {
        #region Private variables
        //The helper for building body mesh
        private readonly VoxelMeshFactory _voxelMeshFactory;
        #endregion

        #region Public variables/Properties
        //Entity Body data holding collections
        public VertexBuffer<VertexPositionColorTexture> VertexBuffer;
        public List<VertexPositionColorTexture> Vertice;

        /// <summary>
        /// Voxel core data
        /// </summary>
        public readonly VoxelEntity Entity;
        /// <summary>
        /// Altered by server or user and needs vertice update (you need to call Update yourself)
        /// </summary>
        public Boolean Altered;
        #endregion

        /// <summary>
        /// creates a VisualEntity ready to render with filled vertice List and vertexBuffer
        /// </summary>
        /// <param name="voxelMeshFactory">voxelMeshFactory responsible to create mesh</param>
        /// <param name="wrapped">wrapped VoxelEntity from server</param>
        public VisualEntity(VoxelMeshFactory voxelMeshFactory, VoxelEntity wrapped)
        {
            Entity = wrapped;
            _voxelMeshFactory = voxelMeshFactory;

            //Vertice = _voxelMeshFactory.GenCubesFaces(Entity.Blocks);
            //VertexBuffer = _voxelMeshFactory.InitBuffer(Vertice);

            //Altered = true;
            RefreshBodyMesh();
        }

        public void RefreshBodyMesh()
        {
            //if (!Altered) return;

            //Vertice = _voxelMeshFactory.GenCubesFaces(Entity.Blocks);

            //if (Vertice.Count != 0)
            //{
            //    VertexBuffer.SetData(Vertice.ToArray());
            //}

            //Altered = false;
        }
        #region Private Methods
        /// <summary>
        /// Update the mesh, regen cube faces, vertice, vertex buffer (dynamic resize happens if needed)
        /// No effect if Altered is false
        /// </summary>
        /// 

        #endregion

        #region Public Methods
        public void Commit()
        {
            //send modified blocks back to server / disk storage
        }

        public virtual void Draw()
        {
        }

        public virtual void Update(ref GameTime timeSpent)
        {
        }

        public virtual void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        public virtual void Dispose()
        {
            if (VertexBuffer != null) VertexBuffer.Dispose();
        }
        #endregion
    }
}