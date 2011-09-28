using System;
using System.Collections.Generic;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Concrete;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Structs;

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
        public readonly IVoxelEntity VoxelEntity;
        public VertexBuffer<VertexCubeSolid> VertexBuffer;
        public List<VertexCubeSolid> Vertice;
        /// <summary>
        /// The BBox surrending the Entity, it will be used for collision detections mainly !
        /// </summary>
        public BoundingBox WorldBBox;
        public BoundingBox LocalBBox;

        private readonly bool _isColorOnly;

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
        /// TODO refactor the way overlay array is allocated, too much copy paste
        /// </summary>
        /// <param name="voxelMeshFactory">voxelMeshFactory responsible to create mesh</param>
        /// <param name="wrapped">wrapped VoxelEntity from server</param>
        /// <param name="overlays">array of texture id to overlay</param>
        /// <param name="isColorOnly">for an entity made of colored cubes not textures</param>
        public VisualEntity(VoxelMeshFactory voxelMeshFactory, IVoxelEntity wrapped,byte[, ,] overlays=null,bool isColorOnly=false)
        {
            VoxelEntity = wrapped;
            _isColorOnly = isColorOnly;

            _voxelMeshFactory = voxelMeshFactory;
            _overlays = overlays;

            Vertice = _voxelMeshFactory.GenCubesFaces(VoxelEntity.Model.Blocks, _overlays, _isColorOnly);
            VertexBuffer = _voxelMeshFactory.InitBuffer(Vertice);

            LocalBBox = new BoundingBox();
            WorldBBox = new BoundingBox();

            //Will be used to update the bounding box with world coordinate when the entity is moving
            LocalBBox.Minimum = new Vector3(-(VoxelEntity.Size.X / 2.0f), 0, -(VoxelEntity.Size.Z / 2.0f));
            LocalBBox.Maximum = new Vector3(+(VoxelEntity.Size.X / 2.0f), VoxelEntity.Size.Y, +(VoxelEntity.Size.Z / 2.0f));

            Altered = true;
            Update();
        }

        public Vector3D Position
        {
            get { return VoxelEntity.Position; }
            set { VoxelEntity.Position = value; }
        }

        /// <summary>
        /// The World Matrix of the entity
        /// It needs to be updated when the entity workd position is changing ! (Translation)
        /// When the entity is rotating (Rotation)
        /// When the entity is changing size (Scaling)
        /// This world Matrix should only be used at Drawing time, it means it could come from interpolated value !
        /// </summary>
        public Matrix World;

        public void Commit()
        {
            //send modified blocks back to server / disk storage
            VoxelEntity.CommitModel();
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

            Vertice = _voxelMeshFactory.GenCubesFaces(VoxelEntity.Model.Blocks, _overlays, _isColorOnly);

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
        public void AlterOverlay(Vector3I loc, byte overlay)
        {
            _overlays[loc.X, loc.Y, loc.Z] = overlay;
            Altered = true;
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="boundingBox"></param>
        public void RefreshWorldBoundingBox(ref Vector3D worldPosition)
        {
            WorldBBox = new BoundingBox(LocalBBox.Minimum + worldPosition.AsVector3(),
                                          LocalBBox.Maximum + worldPosition.AsVector3());
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="boundingBox"></param>
        public BoundingBox ComputeWorldBoundingBox(ref Vector3D worldPosition)
        {
            return new BoundingBox(LocalBBox.Minimum + worldPosition.AsVector3(),
                                          LocalBBox.Maximum + worldPosition.AsVector3());
        }

        public void ComputeWorldBoundingBox(ref Vector3D worldPosition, out BoundingBox worldBB)
        {
            worldBB = new BoundingBox(LocalBBox.Minimum + worldPosition.AsVector3(),
                                          LocalBBox.Maximum + worldPosition.AsVector3());
        }
    }
}