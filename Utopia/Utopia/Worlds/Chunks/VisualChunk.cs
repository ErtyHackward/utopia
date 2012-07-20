using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete.Collectible;
using Utopia.Shared.Structs;
using Utopia.Shared.Interfaces;
using Amib.Threading;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Shared.World;
using Utopia.Entities;
using Utopia.Resources.ModelComp;
using Utopia.Entities.Managers.Interfaces;
using S33M3Resources.Structs;
using S33M3DXEngine.Threading;
using S33M3DXEngine;
using S33M3Resources.Structs.Vertex;
using S33M3DXEngine.Buffers;
using S33M3CoreComponents.WorldFocus;
using S33M3Resources.Effects.Basics;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Maths;
using Utopia.Shared.Entities.Inventory;
using Utopia.Entities.Voxel;
using UtopiaContent.Effects.Entities;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Represents a chunk for 3d rendering
    /// </summary>
    public class VisualChunk : CompressibleChunk, ISingleArrayDataProviderUser, IThreadStatus, IChunkLayout2D, IDisposable
    {
        #region Private variables
        private VisualWorldParameters _visualWorldParameters;
        private WorldFocusManager _worldFocusManager;
        private Range3I _cubeRange;
        private D3DEngine _d3dEngine;
        private FastRandom _rnd = new FastRandom();

        private CameraManager<ICameraFocused> _cameraManager;
        private WorldChunks _worldChunkManager;
        
        private IEntityPickingManager _entityPickingManager;
        private VoxelModelManager _voxelModelManager;

        #endregion

        #region Public properties/Variable
        //List are use instead of standard array because it's not possible to know the number of vertices/indices that will be produced at cubes creation time.
        //After vertex/index buffer creation those collections are cleared.
        public Dictionary<long, int> CubeVerticeDico; // Dictionnary used in the mesh creation, to avoid to recreate a vertex that has already been used create for another cube.
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

        private ChunkState _s;

        public ChunkState State
        {
            get { return _s; }
            set { _s = value; }
        }

        //public ChunkState State { get; set; }                 // Chunk State
        
        
        public bool IsOutsideLightSourcePropagated { get; set; }
        public ThreadStatus ThreadStatus { get; set; }        // Thread status of the chunk, used for sync.
        public WorkItemPriority ThreadPriority { get; set; }  // Thread Priority value
        public int UpdateOrder { get; set; }              // Variable for sync drawing at rebuild time.
        public bool IsBorderChunk { get; set; }               // Set to true if the chunk is located at the border of the visible world !
        private bool _ready2Draw;
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
        public float Opaque { get; set; }
        
        public bool isFrustumCulled { get; set; }             // Chunk Frustum culled
        public Int64 ChunkID { get; set; }                    // Chunk ID

        public Matrix World;                                  // The chunk World matrix ==> Not a property, to be sure it will be direct variables acces !!
        public BoundingBox ChunkWorldBoundingBox;             // The chunk World BoundingBox ==> Not a property, to be sure it will be direct variables acces !!

        public bool IsServerRequested { get; set; }           //If the chunk has been requested to the server

        public Dictionary<string, List<VisualVoxelEntity>> VisualVoxelEntities;

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
                            IEntityPickingManager entityPickingManager,
                            CameraManager<ICameraFocused> cameraManager,
                            WorldChunks worldChunkManager,
                            VoxelModelManager voxelModelManager)
            : base(new SingleArrayDataProvider(singleArrayContainer))
        {
            ((SingleArrayDataProvider)base.BlockData).DataProviderUser = this; //Didn't find a way to pass it inside the constructor

            _d3dEngine = d3dEngine;
            _worldChunkManager = worldChunkManager;

#if DEBUG
            _blockpickedUPEffect = new HLSLVertexPositionColor(_d3dEngine.Device);
#endif

            _cameraManager = cameraManager;
            _worldFocusManager = worldFocusManager;
            _visualWorldParameters = visualWorldParameter;
            _voxelModelManager = voxelModelManager;
            VisualVoxelEntities = new Dictionary<string, List<VisualVoxelEntity>>();
            CubeRange = cubeRange;
            _entityPickingManager = entityPickingManager;
            State = ChunkState.Empty;
            isExistingMesh4Drawing = false;
            Entities.CollectionDirty += Entities_CollectionDirty;
            Entities.EntityAdded += Entities_EntityAdded;
            Entities.EntityRemoved += Entities_EntityRemoved;
            Entities.CollectionCleared += Entities_CollectionCleared;
        }

        #region Public methods
        

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

        //public void SetNewEntityCollection(EntityCollection newEntities)
        //{
        //    if (newEntities == Entities)
        //    {
        //        // TODO: Fabian, why it happens? it should not be
        //        //throw new InvalidOperationException();
        //        return;
        //    }

        //    Entities.Import(newEntities);
        //}

        //Graphical Part
        public void InitializeChunkBuffers()
        {
            CubeVerticeDico = new Dictionary<long, int>();
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
                SolidCubeVB = new VertexBuffer<VertexCubeSolid>(_d3dEngine.Device, SolidCubeVertices.Count, VertexCubeSolid.VertexDeclaration, PrimitiveTopology.TriangleList, "SolidCubeVB", ResourceUsage.Default, 10);
            }
            SolidCubeVB.SetData(_d3dEngine.ImmediateContext ,SolidCubeVertices.ToArray());
            SolidCubeVertices.Clear();

            if (SolidCubeIB == null)
            {
                SolidCubeIB = new IndexBuffer<ushort>(_d3dEngine.Device, SolidCubeIndices.Count, SharpDX.DXGI.Format.R16_UInt , "SolidCubeIB");
            }
            SolidCubeIB.SetData(_d3dEngine.ImmediateContext, SolidCubeIndices.ToArray());
            SolidCubeIndices.Clear();

            CubeVerticeDico.Clear();
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
                LiquidCubeVB = new VertexBuffer<VertexCubeLiquid>(_d3dEngine.Device, LiquidCubeVertices.Count, VertexCubeLiquid.VertexDeclaration, PrimitiveTopology.TriangleList, "LiquidCubeVB", ResourceUsage.Default, 10);
            }
            LiquidCubeVB.SetData(_d3dEngine.ImmediateContext, LiquidCubeVertices.ToArray());
            LiquidCubeVertices.Clear();

            if (LiquidCubeIB == null)
            {
                LiquidCubeIB = new IndexBuffer<ushort>(_d3dEngine.Device, LiquidCubeIndices.Count, SharpDX.DXGI.Format.R16_UInt,"LiquidCubeIB");
            }
            LiquidCubeIB.SetData(_d3dEngine.ImmediateContext, LiquidCubeIndices.ToArray());
            LiquidCubeIndices.Clear();
        }

        //private void SendStaticEntitiesToGraphicalCard()
        //{
        //    if (StaticSpritesVertices.Count == 0)
        //    {
        //        if (StaticSpritesVB != null) StaticSpritesVB.Dispose();
        //        StaticSpritesVB = null;
        //        return;
        //    }

        //    if (StaticSpritesVB == null)
        //    {
        //        StaticSpritesVB = new VertexBuffer<VertexSprite3D>(_d3dEngine.Device, StaticSpritesVertices.Count, VertexSprite3D.VertexDeclaration, PrimitiveTopology.TriangleList, "StaticEntityVB", ResourceUsage.Default, 5);
        //    }
        //    StaticSpritesVB.SetData(_d3dEngine.ImmediateContext, StaticSpritesVertices.ToArray());
        //    StaticSpritesVertices.Clear();

        //    if (StaticSpritesIB == null)
        //    {
        //        StaticSpritesIB = new IndexBuffer<ushort>(_d3dEngine.Device, StaticSpritesIndices.Count, SharpDX.DXGI.Format.R16_UInt, "StaticEntityIB");
        //    }
        //    StaticSpritesIB.SetData(_d3dEngine.ImmediateContext, StaticSpritesIndices.ToArray());
        //    StaticSpritesIndices.Clear();
        //}


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

       //public void DrawStaticEntities(DeviceContext context, HLSLVoxelModel effect)
        //{
        //    foreach (var staticItem in VisualVoxelEntities)
        //    {
        //        staticItem.VisualVoxelModel.Draw(context, effect, staticItem.VoxelEntity.ModelInstance);
        //    }
        //}

        //Ask the Graphical card to Draw the solid faces
        //public void DrawStaticEntities(DeviceContext context)
        //{
        //    if (StaticSpritesVB != null)
        //    {
        //        StaticSpritesVB.SetToDevice(context, 0);
        //        StaticSpritesIB.SetToDevice(context, 0);
        //        context.DrawIndexed(StaticSpritesIB.IndicesCount, 0, 0);
        //    }
        //}

#if DEBUG
        public void DrawDebugBoundingBox(DeviceContext context)
        {
            ChunkBoundingBoxDisplay.Draw(context, _cameraManager.ActiveCamera);
        }
#endif

        private void Entities_CollectionDirty(object sender, EventArgs e)
        {
            _entityPickingManager.isDirty = true; //Tell the Picking manager that it must force the picking entity list !
        }

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
        }

        void Entities_EntityRemoved(object sender, Shared.Entities.Events.EntityCollectionEventArgs e)
        {
            //Remove the entity from Visual Model
            foreach (var pair in VisualVoxelEntities)
            {
                pair.Value.RemoveAll(x => x.Entity == e.Entity);
            }
        }

        void Entities_EntityAdded(object sender, Shared.Entities.Events.EntityCollectionEventArgs e)
        {
            IVoxelEntity voxelEntity = e.Entity as IVoxelEntity;

            if (voxelEntity == null) return; //My entity is not a Voxel Entity => Not possible to render it so !!!

            //Create the Voxel Model Instance for the Item
            VisualVoxelModel model = _voxelModelManager.GetModel(voxelEntity.ModelName, false);
            if (model != null && voxelEntity.ModelInstance == null)
            {
                voxelEntity.ModelInstance = new Shared.Entities.Models.VoxelModelInstance(model.VoxelModel);
                var visualVoxelEntity = new VisualVoxelEntity(voxelEntity, _voxelModelManager);

                //By default the entity is 1/16 if its world size.

                Matrix rotation;
                if (voxelEntity is Plant)
                {
                    Matrix.RotationY((float)(_rnd.NextDouble() * MathHelper.TwoPi), out rotation);
                }
                else
                {
                    rotation = Matrix.Identity;
                }

                visualVoxelEntity.VoxelEntity.ModelInstance.World = rotation * Matrix.Scaling(1f / 16) * visualVoxelEntity.World;

                if (visualVoxelEntity.VisualVoxelModel.Initialized == false)
                {
                    visualVoxelEntity.VisualVoxelModel.BuildMesh();
                }
                
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
            ChunkBoundingBoxDisplay = new BoundingBox3D(_d3dEngine, _worldFocusManager, new Vector3((float)(CubeRange.Max.X - CubeRange.Position.X), (float)(CubeRange.Max.Y - CubeRange.Position.Y), (float)(CubeRange.Max.Z - CubeRange.Position.Z)), _blockpickedUPEffect, Colors.Tomato);
            ChunkBoundingBoxDisplay.Update(ChunkCenter.AsVector3(), Vector3.One, 0);
#endif

            RefreshWorldMatrix();

            VisualVoxelEntities.Clear();

        }

        #endregion

        protected override void BlockBufferChanged(object sender, ChunkDataProviderBufferChangedEventArgs e)
        {
            State = ChunkState.LandscapeCreated;
            base.BlockBufferChanged(sender, e);
        }

        protected override void BlockDataChanged(object sender, ChunkDataProviderDataChangedEventArgs e)
        {
            State = ChunkState.LandscapeCreated;
            base.BlockDataChanged(sender, e);
        }

        public void Dispose()
        {
            if (SolidCubeVB != null) SolidCubeVB.Dispose();
            if (SolidCubeIB != null) SolidCubeIB.Dispose();
            if (LiquidCubeVB != null) LiquidCubeVB.Dispose();
            if (LiquidCubeIB != null) LiquidCubeIB.Dispose();
            Entities.CollectionDirty -= Entities_CollectionDirty;
            Entities.EntityAdded -= Entities_EntityAdded;
            Entities.EntityRemoved -= Entities_EntityRemoved;
#if DEBUG
            _blockpickedUPEffect.Dispose();
            ChunkBoundingBoxDisplay.Dispose();
#endif

        }
    }
}
