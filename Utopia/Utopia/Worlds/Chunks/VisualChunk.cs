﻿using System;
using System.Collections.Generic;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Structs;
using Utopia.Shared.Interfaces;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Shared.World;
using Utopia.Resources.ModelComp;
using S33M3Resources.Structs;
using S33M3DXEngine.Threading;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3CoreComponents.WorldFocus;
using S33M3Resources.Effects.Basics;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Maths;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Resources.VertexFormats;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Entities.Events;
using Utopia.Entities;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Represents a chunk for 3d rendering
    /// </summary>
    public class VisualChunk : CompressibleChunk, ISingleArrayDataProviderUser, IChunkLayout2D, IDisposable
    {
        #region Private variables
        private readonly object _syncRoot = new object();

        private VisualWorldParameters _visualWorldParameters;
        private readonly SingleArrayChunkContainer _singleArrayContainer;
        private WorldFocusManager _worldFocusManager;
        private Range3I _cubeRange;
        private D3DEngine _d3dEngine;
        private FastRandom _rnd = new FastRandom();

        private CameraManager<ICameraFocused> _cameraManager;
        private WorldChunks _worldChunkManager;
        
        private VoxelModelManager _voxelModelManager;

        private IChunkEntityImpactManager _chunkEntityImpactManager;

        #endregion

        #region Public properties/Variable
        //List are use instead of standard array because it's not possible to know the number of vertices/indices that will be produced at cubes creation time.
        //After vertex/index buffer creation those collections are cleared.
        public List<VertexCubeSolid> SolidCubeVertices;      // Collection use to collect the vertices at the solid cube creation time
        public List<ushort> SolidCubeIndices;                // Collection use to collect the indices at the solid cube creation time
        public List<VertexCubeLiquid> LiquidCubeVertices;    // Collection use to collect the vertices at the liquid cube creation time
        public List<ushort> LiquidCubeIndices;               // Collection use to collect the indices at the liquid cube creation time

        //public List<VertexSprite3D> StaticSpritesVertices;
        //public List<ushort> StaticSpritesIndices;

        //Graphical chunk components Exposed VB and IB ==> Called a lot, so direct acces without property bounding
        public VertexBuffer<VertexCubeSolid> SolidCubeVB;   //Solid cube vertex Buffer
        public IndexBuffer<ushort> SolidCubeIB;             //Solid cube index buffer
        public VertexBuffer<VertexCubeLiquid> LiquidCubeVB; //Liquid cube vertex Buffer
        public IndexBuffer<ushort> LiquidCubeIB;            //Liquid cube index Buffer

        //public VertexBuffer<VertexSprite3D> StaticSpritesVB;
        //public IndexBuffer<ushort> StaticSpritesIB;

        public Double DistanceFromPlayer { get; set; }
        public Vector3D ChunkCenter { get; set; } 
        public Vector2I ChunkPositionBlockUnit { get; private set; } // Gets or sets current chunk position in Block Unit
        public Vector2I ChunkPosition { get; private set; } // Gets or sets current chunk position in Chunk Unit

        public ChunkState State;
        
        public bool IsOutsideLightSourcePropagated { get; set; }
        public ThreadsManager.ThreadStatus ThreadStatus { get; set; }        // Thread status of the chunk, used for sync.
        public string ThreadLockedBy { get; set; }

        public int UpdateOrder { get; set; }              // Variable for sync drawing at rebuild time.
        public bool IsBorderChunk { get; set; }               // Set to true if the chunk is located at the border of the visible world !
        private bool _ready2Draw;

        private bool _isServerRequested;
        private DateTime _serverRequestTime;

        public VisualChunk[] SurroundingChunks;
        /// <summary>
        /// Whenever the chunk mesh are ready to be rendered to screen
        /// </summary>
        public bool isExistingMesh4Drawing
        {
            get { return _ready2Draw; }
            internal set 
            {
                if (_ready2Draw != value)
                {
                    _ready2Draw = value;
                    if (_ready2Draw) OnReadyToDraw(); //Event raised when the chunk is full ready to be rendered
                }
            }
        }

        public static Int64 ComputeChunkId(int PosiX, int PosiY)
        {
            MathHelper.IntsToLong hashLong = new MathHelper.IntsToLong();
            hashLong.LeftInt32 = PosiX;
            hashLong.RightInt32 = PosiY;
            return hashLong.LongValue;
        }

        /// <summary>
        /// Gets or sets the value of chunk opaque. Allows to create slowly appearing effect
        /// </summary>
        public FTSValue<float> PopUpValue = new FTSValue<float>(0.0f);
        
        public bool isFrustumCulled { get; set; }             // Chunk Frustum culled
        public Int64 ChunkID { get; set; }                    // Chunk ID

        public Matrix World;                                  // The chunk World matrix ==> Not a property, to be sure it will be direct variables acces !!
        public BoundingBox ChunkWorldBoundingBox;             // The chunk World BoundingBox ==> Not a property, to be sure it will be direct variables acces !!

        public bool IsServerRequested
        {
            get { return _isServerRequested; }
            set { _isServerRequested = value; if (value) _serverRequestTime = DateTime.Now; }
        }
        public DateTime ServerRequestTime
        {
            get { return _serverRequestTime; }
        }

        /// <summary>
        /// Dictionary by the model name of the entity
        /// </summary>
        private Dictionary<string, List<VisualVoxelEntity>> VisualVoxelEntities;

        public List<EntityMetaData> EmitterStaticEntities;
        public List<IItem> SoundStaticEntities;

        public int StorageRequestTicket { get; set; }

        public new SingleArrayDataProvider BlockData
        {
            get { return (SingleArrayDataProvider)base.BlockData; }
        }

        public Range3I CubeRange
        {
            get { return _cubeRange; }
            set
            {
                _cubeRange = value;
                RangeChanged();
            }
        }

        public Vector2I Position
        {
            get
            {
                return ChunkPosition;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        
        /// <summary>
        /// Desired slice of the mesh
        /// </summary>
        public int SliceValue { get; set; }

        /// <summary>
        /// Actual slice value of the chunk mesh
        /// </summary>
        public int SliceOfMesh { get; set; }

        //Use to display bounding box around chunk in debug mode only (Quite slow and not optimized)
#if DEBUG
        public BoundingBox3D ChunkBoundingBoxDisplay;
        private HLSLVertexPositionColor _blockpickedUPEffect;
#endif

        #endregion

        /// <summary>
        /// Occurs when chunk mesh updated
        /// </summary>
        public event EventHandler ChunkMeshUpdated;
        internal void OnChunkMeshUpdated()
        {
            var handler = ChunkMeshUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when chunk is ready to draw ;)
        /// </summary>
        public event EventHandler ReadyToDraw;
        private void OnReadyToDraw()
        {
            var handler = ReadyToDraw;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public VisualChunk(
                            D3DEngine d3dEngine, 
                            WorldFocusManager worldFocusManager, 
                            VisualWorldParameters visualWorldParameter, 
                            ref Range3I cubeRange, 
                            SingleArrayChunkContainer singleArrayContainer,
                            CameraManager<ICameraFocused> cameraManager,
                            WorldChunks worldChunkManager,
                            VoxelModelManager voxelModelManager,
                            IChunkEntityImpactManager chunkEntityImpactManager)
            : base(new SingleArrayDataProvider(singleArrayContainer))
        {
            ((SingleArrayDataProvider)base.BlockData).DataProviderUser = this; //Didn't find a way to pass it inside the constructor


            SliceValue = -1;
            _d3dEngine = d3dEngine;
            _worldChunkManager = worldChunkManager;
            _chunkEntityImpactManager = chunkEntityImpactManager;
#if DEBUG
            _blockpickedUPEffect = new HLSLVertexPositionColor(_d3dEngine.Device);
#endif

            _cameraManager = cameraManager;
            _worldFocusManager = worldFocusManager;
            _visualWorldParameters = visualWorldParameter;
            _singleArrayContainer = singleArrayContainer;
            _voxelModelManager = voxelModelManager;
            VisualVoxelEntities = new Dictionary<string, List<VisualVoxelEntity>>();
            EmitterStaticEntities = new List<EntityMetaData>();
            SoundStaticEntities = new List<IItem>();
            CubeRange = cubeRange;
            State = ChunkState.Empty;
            isExistingMesh4Drawing = false;
            Entities.EntityAdded += Entities_EntityAdded;
            Entities.EntityRemoved += Entities_EntityRemoved;
            Entities.CollectionCleared += Entities_CollectionCleared;
        }

        #region Public methods

        public void CreateVisualEntities()
        {
            foreach (var entity in Entities.EnumerateFast())
            {
                Entities_EntityAdded(null,
                                     new Shared.Entities.Events.EntityCollectionEventArgs
                                     {
                                         Chunk = this,
                                         Entity = entity
                                     });
            }
        }

        public void RefreshBorderChunk()
        {
            IsBorderChunk = isBorderChunk(ChunkPositionBlockUnit.X, ChunkPositionBlockUnit.Y);

            //Get the surrounding chunks if BorderChunk is null
            if (IsBorderChunk == false)
            {
                SurroundingChunks = _worldChunkManager.GetsurroundingChunkFromChunkCoord(ChunkPosition.X, ChunkPosition.Y);
            }
            else
            {
                SurroundingChunks = new VisualChunk[0];
            }
        }

        public bool SurroundingChunksMinimumState(ChunkState minimumState)
        {
            foreach (var chunk in SurroundingChunks)
            {
                if (chunk.State < minimumState) return false;
            }
            return true;
        }

        //Graphical Part
        public void InitializeChunkBuffers()
        {
            SolidCubeVertices = new List<VertexCubeSolid>();
            SolidCubeIndices = new List<ushort>();
            LiquidCubeVertices = new List<VertexCubeLiquid>();
            LiquidCubeIndices = new List<ushort>();

            //StaticSpritesIndices = new List<ushort>();
            //StaticSpritesVertices = new List<VertexSprite3D>();
        }

        public void SendCubeMeshesToBuffers()
        {
            SendSolidCubeMeshToGraphicCard();       //Solid Cubes
            SendLiquidCubeMeshToGraphicCard();      //See Through Cubes
            //SendStaticEntitiesToGraphicalCard();    //Static Entities Sprite + Voxel
            State = ChunkState.DisplayInSyncWithMeshes;
            isExistingMesh4Drawing = true;
            SliceOfMesh = SliceValue;
        }

        //Solid Cube
        //Create the VBuffer + IBuffer from the List, then clear the list
        //The Buffers are pushed to the graphic card. (SetData());
        private void SendSolidCubeMeshToGraphicCard()
        {
            if (SolidCubeVertices.Count == 0)
            {
                if (SolidCubeVB != null) SolidCubeVB.Dispose();
                SolidCubeVB = null;
                return;
            }

            if (SolidCubeVB == null)
            {
                SolidCubeVB = new VertexBuffer<VertexCubeSolid>(_d3dEngine.Device, SolidCubeVertices.Count, PrimitiveTopology.TriangleList, "SolidCubeVB", ResourceUsage.Default, 10);
            }
            SolidCubeVB.SetData(_d3dEngine.ImmediateContext ,SolidCubeVertices.ToArray());
            SolidCubeVertices.Clear();

            if (SolidCubeIB == null)
            {
                SolidCubeIB = new IndexBuffer<ushort>(_d3dEngine.Device, SolidCubeIndices.Count, "SolidCubeIB");
            }
            SolidCubeIB.SetData(_d3dEngine.ImmediateContext, SolidCubeIndices.ToArray());
            SolidCubeIndices.Clear();
        }

        //Liquid Cube
        //Create the VBuffer + IBuffer from the List, then clear the list
        //The Buffers are pushed to the graphic card. (SetData());
        private void SendLiquidCubeMeshToGraphicCard()
        {
            if (LiquidCubeVertices.Count == 0)
            {
                if (LiquidCubeVB != null) LiquidCubeVB.Dispose();
                LiquidCubeVB = null;
                return;
            }

            if (LiquidCubeVB == null)
            {
                LiquidCubeVB = new VertexBuffer<VertexCubeLiquid>(_d3dEngine.Device, LiquidCubeVertices.Count, PrimitiveTopology.TriangleList, "LiquidCubeVB", ResourceUsage.Default, 10);
            }
            LiquidCubeVB.SetData(_d3dEngine.ImmediateContext, LiquidCubeVertices.ToArray());
            LiquidCubeVertices.Clear();

            if (LiquidCubeIB == null)
            {
                LiquidCubeIB = new IndexBuffer<ushort>(_d3dEngine.Device, LiquidCubeIndices.Count,"LiquidCubeIB");
            }
            LiquidCubeIB.SetData(_d3dEngine.ImmediateContext, LiquidCubeIndices.ToArray());
            LiquidCubeIndices.Clear();
        }

        //Ask the Graphical card to Draw the solid faces
        public void DrawSolidFaces(DeviceContext context)
        {
            if (SolidCubeVB != null)
            {
                SolidCubeVB.SetToDevice(context, 0);
                SolidCubeIB.SetToDevice(context, 0);
                context.DrawIndexed(SolidCubeIB.IndicesCount, 0, 0);
            }
        }

        //Ask the Graphical card to Draw the solid faces
        public void DrawLiquidFaces(DeviceContext context)
        {
            if (LiquidCubeVB != null)
            {
                LiquidCubeVB.SetToDevice(context, 0);
                LiquidCubeIB.SetToDevice(context, 0);
                context.DrawIndexed(LiquidCubeIB.IndicesCount, 0, 0);
            }
        }

#if DEBUG
        public void DrawDebugBoundingBox(DeviceContext context)
        {
            ChunkBoundingBoxDisplay.Draw(context, _cameraManager.ActiveCamera);
        }
#endif

        void Entities_CollectionCleared(object sender, EventArgs e)
        {
            foreach (var entityList in VisualVoxelEntities.Values)
            {
                foreach (IDisposable i in entityList)
                {
                    i.Dispose();
                }    
            }
            
            VisualVoxelEntities.Clear();
            EmitterStaticEntities.Clear();
            SoundStaticEntities.Clear();
        }

        void Entities_EntityRemoved(object sender, EntityCollectionEventArgs e)
        {
            RemoveVoxelEntity(e);
            RemoveParticuleEmitterEntity(e);
            RemoveSoundEntity(e);
        }

        /// <summary>
        /// New Static Entity added to the chunk
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Entities_EntityAdded(object sender, Shared.Entities.Events.EntityCollectionEventArgs e)
        {
            AddVoxelEntity(e);
            AddParticuleEmitterEntity(e);
            AddSoundEntity(e);
        }

        private void AddVoxelEntity(EntityCollectionEventArgs e)
        {
            var voxelEntity = e.Entity as IVoxelEntity;
            if (voxelEntity == null) return; //My static entity is not a Voxel Entity => Not possible to render it so !!!

            //Create the Voxel Model Instance for the Item
            VisualVoxelModel model = null;
            if (!string.IsNullOrEmpty(voxelEntity.ModelName)) model = _voxelModelManager.GetModel(voxelEntity.ModelName, false);

            if (model != null && voxelEntity.ModelInstance == null) //The model blueprint is existing, and I need to create an instance of it !
            {
                voxelEntity.ModelInstance = new VoxelModelInstance(model.VoxelModel);
                var visualVoxelEntity = new VisualVoxelEntity(voxelEntity, _voxelModelManager);

                //Get default world translation
                Matrix instanceTranslation = Matrix.Translation(voxelEntity.Position.AsVector3());

                //Apply special rotation to the creation instance
                Quaternion instanceRotation = Quaternion.Identity;
                if (voxelEntity is IRndYRotation && ((IRndYRotation)voxelEntity).RndRotationAroundY)
                {
                    instanceRotation = Quaternion.RotationAxis(Vector3.UnitY, (float)(_rnd.NextDouble() * MathHelper.TwoPi));
                }
                else if (voxelEntity is IItem)
                {
                    var item = voxelEntity as IItem;
                    instanceRotation = item.Rotation;
                }

                //Apply special scaling to created entity (By default all blue print are 16 times too big.
                Matrix instanceScaling = Matrix.Scaling(1.0f / 16.0f);

                //Create the World transformation matrix for the instance.
                //We take the Model instance world matrix where we add a Rotation and scaling proper to the instance
                visualVoxelEntity.VoxelEntity.ModelInstance.World = instanceScaling * instanceTranslation;
                visualVoxelEntity.VoxelEntity.ModelInstance.Rotation = instanceRotation;

                TerraCubeResult result;
                if (visualVoxelEntity.Entity is BlockLinkedItem)
                {
                    result = _singleArrayContainer.GetCube(((BlockLinkedItem)visualVoxelEntity.Entity).BlockLocationRoot);
                }
                else
                {
                    //Find the Cube where the entity is placed, and assign its color to the entity
                    result = _singleArrayContainer.GetCube(visualVoxelEntity.VoxelEntity.Position);
                }

                if (result.isValid)
                {
                    visualVoxelEntity.BlockLight = result.Cube.EmissiveColor;
                }
                else
                {
                    visualVoxelEntity.BlockLight = new ByteColor(255, 255, 255, 255);
                }


                if (visualVoxelEntity.VisualVoxelModel.Initialized == false)
                {
                    visualVoxelEntity.VisualVoxelModel.BuildMesh();
                }

                if (voxelEntity.ModelInstance.CanPlay("Idle"))
                {
                    voxelEntity.ModelInstance.Play("Idle", true);
                }

                lock (_syncRoot)
                {
                    List<VisualVoxelEntity> list;
                    if (VisualVoxelEntities.TryGetValue(voxelEntity.ModelName, out list))
                    {
                        list.Add(visualVoxelEntity);
                    }
                    else
                    {
                        VisualVoxelEntities.Add(voxelEntity.ModelName, new List<VisualVoxelEntity> { visualVoxelEntity });
                    }
                }

                ILightEmitterEntity lightEntity = e.Entity as ILightEmitterEntity;
                if (e.AtChunkCreationTime == false && lightEntity != null)
                {
                    //Get the Cube where is located the entity
                    Vector3D entityWorldPosition = ((IEntity)lightEntity).Position;
                    Vector3I entityBlockPosition = new Vector3I(MathHelper.Floor(entityWorldPosition.X),
                                                                MathHelper.Floor(entityWorldPosition.Y),
                                                                MathHelper.Floor(entityWorldPosition.Z));
                    _chunkEntityImpactManager.CheckImpact(new TerraCubeWithPosition(entityBlockPosition, WorldConfiguration.CubeId.Air, _visualWorldParameters.WorldParameters.Configuration), this);
                }
            }
        }

        private void RemoveVoxelEntity(EntityCollectionEventArgs e)
        {
            //Remove the entity from Visual Model
            foreach (var pair in VisualVoxelEntities)
            {
                pair.Value.RemoveAll(x => x.Entity == e.Entity);
            }

            ILightEmitterEntity lightEntity = e.Entity as ILightEmitterEntity;
            if (e.AtChunkCreationTime == false && lightEntity != null)
            {
                //Get the Cube where is located the entity
                Vector3D entityWorldPosition = ((IEntity)lightEntity).Position;
                Vector3I entityBlockPosition = new Vector3I(MathHelper.Floor(entityWorldPosition.X),
                                                            MathHelper.Floor(entityWorldPosition.Y),
                                                            MathHelper.Floor(entityWorldPosition.Z));
                _chunkEntityImpactManager.CheckImpact(new TerraCubeWithPosition(entityBlockPosition, WorldConfiguration.CubeId.Air, _visualWorldParameters.WorldParameters.Configuration), this);
            }
        }

        private void AddSoundEntity(EntityCollectionEventArgs e)
        {
            IItem item = e.Entity as IItem;
            if (item == null || item.EmittedSound == null || item.EmittedSound.FilePath == null) return;
            SoundStaticEntities.Add(item);
        }

        private void RemoveSoundEntity(EntityCollectionEventArgs e)
        {
            IItem item = e.Entity as IItem;
            if (item == null || item.EmittedSound == null || item.EmittedSound.FilePath == null) return;
            SoundStaticEntities.Remove(item);
        }

        private void AddParticuleEmitterEntity(EntityCollectionEventArgs e)
        {
            if (e.Entity.Particules == null) return;
            foreach (var entityParticules in e.Entity.Particules)
            {
                EmitterStaticEntities.Add(new EntityMetaData() { Entity = e.Entity, Particule = entityParticules, EntityLastEmitTime = DateTime.Now });
            }
        }

        private void RemoveParticuleEmitterEntity(EntityCollectionEventArgs e)
        {
            if (e.Entity.Particules == null) return;
            EmitterStaticEntities.RemoveAll(x => x.Entity == e.Entity);
        }

        /// <summary>
        /// Allows to enumerate entities in threadsafe way
        /// </summary>
        /// <returns></returns>
        public IEnumerable<VisualVoxelEntity> AllEntities()
        {
            lock (_syncRoot)
            {
                foreach (var pair in VisualVoxelEntities)
                {
                    foreach (var entity in pair.Value)
                    {
                        yield return entity;
                    }
                }
            }
        }

        /// <summary>
        /// Allows to enumerate a group of entity with the given model name in a threadsafe way
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public IEnumerable<VisualVoxelEntity> AllEntities(string modelName)
        {
            lock (_syncRoot)
            {
                foreach (var entity in VisualVoxelEntities[modelName])
                {
                    yield return entity;
                }
            }
        }

        /// <summary>
        /// Allows to enumerate entities grouped by model in a threadsafe way
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, List<VisualVoxelEntity>>> AllPairs()
        {
            lock (_syncRoot)
            {
                return VisualVoxelEntities;
            }
        }

        #endregion

        #region Privates Methods

        /// <summary>
        /// Is my chunk at the edge of the visible world ?
        /// </summary>
        /// <param name="X">Chunk world X position</param>
        /// <param name="Z">Chunk world Z position</param>
        /// <returns>True if the chunk is at border</returns>
        private bool isBorderChunk(int X, int Z)
        {
            if (X == _visualWorldParameters.WorldRange.Position.X ||
               Z == _visualWorldParameters.WorldRange.Position.Z ||
               X == _visualWorldParameters.WorldRange.Max.X - AbstractChunk.ChunkSize.X ||
               Z == _visualWorldParameters.WorldRange.Max.Z - AbstractChunk.ChunkSize.Z)
            {
                return true;
            }
            return false;
        }

        private void RefreshWorldMatrix()
        {
            Matrix.Translation(_cubeRange.Position.X, _cubeRange.Position.Y, _cubeRange.Position.Z, out World); //Create a matrix for world translation

            //Refresh the bounding Box to make it in world coord.
            ChunkWorldBoundingBox.Minimum = new Vector3(_cubeRange.Position.X, _cubeRange.Position.Y, _cubeRange.Position.Z);
            ChunkWorldBoundingBox.Maximum = new Vector3(_cubeRange.Max.X, _cubeRange.Max.Y, _cubeRange.Max.Z);
        }

        private void RangeChanged() // Start it also if the World offset Change !!!
        {
            ChunkPositionBlockUnit = new Vector2I() { X = _cubeRange.Position.X, Y = _cubeRange.Position.Z };

            ChunkPosition = new Vector2I() { X = _cubeRange.Position.X / AbstractChunk.ChunkSize.X, Y = _cubeRange.Position.Z / AbstractChunk.ChunkSize.Z };

            ChunkID = VisualChunk.ComputeChunkId(ChunkPosition.X, ChunkPosition.Y);

            ChunkCenter = new Vector3D(_cubeRange.Position.X + (_cubeRange.Max.X - _cubeRange.Position.X) / 2.0,
                           _cubeRange.Position.Y + (_cubeRange.Max.Y - _cubeRange.Position.Y) / 2.0,
                           _cubeRange.Position.Z + (_cubeRange.Max.Z - _cubeRange.Position.Z) / 2.0);

#if DEBUG
            ChunkBoundingBoxDisplay = new BoundingBox3D(_d3dEngine, _worldFocusManager, new Vector3((float)(CubeRange.Max.X - CubeRange.Position.X), (float)(CubeRange.Max.Y - CubeRange.Position.Y), (float)(CubeRange.Max.Z - CubeRange.Position.Z)), _blockpickedUPEffect, Color.Tomato);
            ChunkBoundingBoxDisplay.Update(ChunkCenter.AsVector3(), Vector3.One, 0);
#endif

            RefreshWorldMatrix();

            SoundStaticEntities.Clear();
            lock (_syncRoot)
                VisualVoxelEntities.Clear();
            EmitterStaticEntities.Clear();
        }

        #endregion

        public void Dispose()
        {
            if (SolidCubeVB != null) SolidCubeVB.Dispose();
            if (SolidCubeIB != null) SolidCubeIB.Dispose();
            if (LiquidCubeVB != null) LiquidCubeVB.Dispose();
            if (LiquidCubeIB != null) LiquidCubeIB.Dispose();
            Entities.EntityAdded -= Entities_EntityAdded;
            Entities.EntityRemoved -= Entities_EntityRemoved;
#if DEBUG
            _blockpickedUPEffect.Dispose();
            ChunkBoundingBoxDisplay.Dispose();
#endif

        }
    }
}
