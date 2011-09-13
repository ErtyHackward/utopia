using System;
using System.Collections.Generic;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks.Entities.Concrete;
using S33M3Engines.Shared.Math;
using S33M3Engines.D3D;
using Utopia.Shared.Chunks.Entities.Interfaces;
using S33M3Engines.Struct;
using S33M3Engines.StatesManager;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.WorldFocus;
using SharpDX.Direct3D11;
using UtopiaContent.Effects.Terran;

namespace Utopia.Entities
{
    /// <summary>
    ///  This Class is responsible for the Entity Rendering into the world 
    /// </summary>
    public class VisualEntity : DrawableGameComponent, IDisposable
    {
        #region Private variables
        //The helper for building body mesh
        private readonly VoxelMeshFactory _voxelMeshFactory;
        private Vector3 _boundingMinPoint, _boundingMaxPoint;

        private HLSLTerran _entityEffect;
        private D3DEngine _d3DEngine;
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;
        #endregion

        #region Public variables/Properties
        //Entity Body data holding collections
        public VertexBuffer<VertexCubeSolid> VertexBuffer;
        public List<VertexCubeSolid> Vertice;
        public FTSValue<DVector3> WorldPosition = new FTSValue<DVector3>(); //World Position
        public BoundingBox BoundingBox;
        public Vector3 EntityEyeOffset;                                     //Offset of the camera Placement inside the entity, from entity center point.

        public bool IsPlayerConstroled { get; set; }

        /// <summary>
        /// Voxel core data
        /// </summary>
        public readonly VoxelEntity VoxelEntity;

        public readonly IEntity Entity;

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
        public VisualEntity(D3DEngine d3DEngine, CameraManager camManager, WorldFocusManager worldFocusManager, VoxelMeshFactory voxelMeshFactory, VoxelEntity voxelEntity, IEntity entity)
        {
            VoxelEntity = voxelEntity;
            Entity = entity;
            _voxelMeshFactory = voxelMeshFactory;
            _d3DEngine = d3DEngine;
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;

            Vertice = _voxelMeshFactory.GenCubesFaces(VoxelEntity.Blocks);
            VertexBuffer = _voxelMeshFactory.InitBuffer(Vertice);

            Altered = true;

            Initialize();
            RefreshBodyMesh();
        }

        public void RefreshBodyMesh()
        {
            if (!Altered) return;

            Vertice = _voxelMeshFactory.GenCubesFaces(VoxelEntity.Blocks);

            if (Vertice.Count != 0)
            {
                VertexBuffer.SetData(Vertice.ToArray());
            }

            Altered = false;
        }
        #region Private Methods
        /// <summary>
        /// Update the mesh, regen cube faces, vertice, vertex buffer (dynamic resize happens if needed)
        /// No effect if Altered is false
        /// </summary>
        /// 
        public override sealed void Initialize()
        {
            //Will be used to update the bounding box with world coordinate when the entity is moving
            _boundingMinPoint = new Vector3(-(Entity.Size.X / 2.0f), 0, -(Entity.Size.Z / 2.0f));
            _boundingMaxPoint = new Vector3(+(Entity.Size.X / 2.0f), Entity.Size.Y, +(Entity.Size.Z / 2.0f));

            RefreshBoundingBox(ref WorldPosition.Value, out BoundingBox);

            EntityEyeOffset = new Vector3(0, Entity.Size.Y / 100 * 80, 0);

            _entityEffect = new HLSLTerran(_d3DEngine, @"Effects/Terran/Terran.hlsl", VertexCubeSolid.VertexDeclaration);
        }

        protected void RefreshBoundingBox(ref DVector3 worldPosition, out BoundingBox boundingBox)
        {
            boundingBox = new BoundingBox(_boundingMinPoint + worldPosition.AsVector3(),
                                          _boundingMaxPoint + worldPosition.AsVector3());
        }

        #endregion

        #region Public Methods
        public void Commit()
        {
            //send modified blocks back to server / disk storage
        }

        public override void Draw()
        {
            //Applying Correct Render States
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.CullNone, GameDXStates.DXStates.Blenders.Disabled , GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _entityEffect.Begin();

            _entityEffect.CBPerFrame.Values.ViewProjection  = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
            _entityEffect.CBPerFrame.IsDirty = true;

            if (WorldPosition.ActualValue.X == _camManager.ActiveCamera.WorldPosition.X && WorldPosition.ActualValue.Z == _camManager.ActiveCamera.WorldPosition.Z) return;

            Vector3 entityCenteredPosition = WorldPosition.ActualValue.AsVector3();
            entityCenteredPosition.X -= Entity.Size.X / 2;
            entityCenteredPosition.Z -= Entity.Size.Z / 2;

            Matrix world = Matrix.Scaling(Entity.Size) * Matrix.Translation(entityCenteredPosition);

            world = _worldFocusManager.CenterOnFocus(ref world);

            _entityEffect.CBPerDraw.Values.World = Matrix.Transpose(world);
            _entityEffect.CBPerDraw.IsDirty = true;
            _entityEffect.Apply();

            VertexBuffer.SetToDevice(0);
            _d3DEngine.Context.Draw(VertexBuffer.VertexCount, 0);
        }

        public override void Update(ref GameTime timeSpent)
        {
            //Refresh location and Rotations component with the new values
            RefreshBoundingBox(ref WorldPosition.Value, out BoundingBox);
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        public override void Dispose()
        {
            if (_entityEffect != null) _entityEffect.Dispose();
            if (VertexBuffer != null) VertexBuffer.Dispose();

        }
        #endregion
    }
}