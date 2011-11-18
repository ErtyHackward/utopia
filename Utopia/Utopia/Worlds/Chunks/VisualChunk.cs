using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Structs;
using Utopia.Shared.Interfaces;
using S33M3Engines.Threading;
using Amib.Threading;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using S33M3Engines.Buffers;
using S33M3Engines;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using S33M3Engines.WorldFocus;
using Utopia.Shared.World;
using Utopia.Entities;
using Utopia.Resources.ModelComp;
using S33M3Engines.Shared.Math;
using Utopia.Entities.Sprites;
using Utopia.Entities.Managers.Interfaces;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Represents a chunk for 3d rendering
    /// </summary>
    public class VisualChunk : CompressibleChunk, ISingleArrayDataProviderUser, IThreadStatus, IChunkLayout2D, IDisposable
    {
        #region Private variables
        private VisualWorldParameters _visualWorldParameters;
        private Range<int> _cubeRange;
        private D3DEngine _d3dEngine;

        private object Lock_DrawChunksSolidFaces = new object();       //Multithread Locker
        private object Lock_DrawChunksSeeThrough1Faces = new object(); //Multithread Locker
        private object Lock_Draw = new object(); //Multithread Locker

        private IEntityPickingManager _entityPickingManager;
        #endregion

        #region Public properties/Variable
        //List are use instead of standard array because it's not possible to know the number of vertices/indices that will be produced at cubes creation time.
        //After vertex/index buffer creation those collections are cleared.
        public Dictionary<long, int> CubeVerticeDico; // Dictionnary used in the mesh creation, to avoid to recreate a vertex that has already been used create for another cube.
        public List<VertexCubeSolid> SolidCubeVertices;      // Collection use to collect the vertices at the solid cube creation time
        public List<ushort> SolidCubeIndices;                // Collection use to collect the indices at the solid cube creation time
        public List<VertexCubeLiquid> LiquidCubeVertices;    // Collection use to collect the vertices at the liquid cube creation time
        public List<ushort> LiquidCubeIndices;               // Collection use to collect the indices at the liquid cube creation time

        public List<VertexSprite3D> StaticSpritesVertices;
        public List<ushort> StaticSpritesIndices;

        //Graphical chunk components Exposed VB and IB ==> Called a lot, so direct acces without property bounding
        public VertexBuffer<VertexCubeSolid> SolidCubeVB;   //Solid cube vertex Buffer
        public IndexBuffer<ushort> SolidCubeIB;             //Solid cube index buffer
        public VertexBuffer<VertexCubeLiquid> LiquidCubeVB; //Liquid cube vertex Buffer
        public IndexBuffer<ushort> LiquidCubeIB;            //Liquid cube index Buffer

        public VertexBuffer<VertexSprite3D> StaticSpritesVB;
        public IndexBuffer<ushort> StaticSpritesIB;

        public Vector3D ChunkCenter { get; set; } 
        public Vector2I ChunkPositionBlockUnit { get; private set; } // Gets or sets current chunk position in Block Unit
        public Vector2I ChunkPosition { get; private set; } // Gets or sets current chunk position in Chunk Unit
        public ChunkState State { get; set; }                 // Chunk State
        public ThreadStatus ThreadStatus { get; set; }        // Thread status of the chunk, used for sync.
        public WorkItemPriority ThreadPriority { get; set; }  // Thread Priority value
        public int UserChangeOrder { get; set; }              // Variable for sync drawing at rebuild time.
        public bool BorderChunk { get; set; }                 // Set to true if the chunk is located at the border of the visible world !
        public bool Ready2Draw { get; set; }                  // Whenever the chunk mesh are ready to be rendered to screen
        public bool isFrustumCulled { get; set; }             // Chunk Frustum culled
        public Int64 ChunkID { get; set; }                    // Chunk ID

        public Matrix World;                                  // The chunk World matrix ==> Not a property, to be sure it will be direct variables acces !!
        public BoundingBox ChunkWorldBoundingBox;             // The chunk World BoundingBox ==> Not a property, to be sure it will be direct variables acces !!

        public bool IsServerRequested { get; set; }           //If the chunk has been requested to the server

        public Location2<int> LightPropagateBorderOffset;

        public List<VisualEntity> VisualSpriteEntities;

        public int StorageRequestTicket { get; set; }

        public Range<int> CubeRange
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

#if DEBUG
        private BoundingBox3D _chunkBoundingBoxDisplay;
        public BoundingBox3D ChunkBoundingBoxDisplay { get { return _chunkBoundingBoxDisplay; } }
#endif

        #endregion

        public VisualChunk(
                            D3DEngine d3dEngine, 
                            WorldFocusManager worldFocusManager, 
                            VisualWorldParameters visualWorldParameter, 
                            ref Range<int> cubeRange, 
                            SingleArrayChunkContainer singleArrayContainer,
                            IEntityPickingManager entityPickingManager)
            : base(new SingleArrayDataProvider(singleArrayContainer))
        {
            ((SingleArrayDataProvider)base.BlockData).DataProviderUser = this; //Didn't find a way to pass it inside the constructor

#if DEBUG
            _chunkBoundingBoxDisplay = new BoundingBox3D(d3dEngine, worldFocusManager, new Vector3((float)(cubeRange.Max.X - cubeRange.Min.X), (float)(cubeRange.Max.Y - cubeRange.Min.Y), (float)(cubeRange.Max.Z - cubeRange.Min.Z)), S33M3Engines.D3D.Effects.Basics.DebugEffect.DebugEffectVPC, Color.Tomato);
#endif
            _d3dEngine = d3dEngine;
            _visualWorldParameters = visualWorldParameter;
            VisualSpriteEntities = new List<VisualEntity>();
            CubeRange = cubeRange;
            _entityPickingManager = entityPickingManager;
            State = ChunkState.Empty;
            Ready2Draw = false;
            LightPropagateBorderOffset = new Location2<int>(0, 0);
        }
        #region Public methods
        

        public void RefreshBorderChunk()
        {
            BorderChunk = isBorderChunk(ChunkPositionBlockUnit.X, ChunkPositionBlockUnit.Y);
        }

        public void SetNewEntityCollection(EntityCollection newEntities)
        {
            if(base.Entities != null) base.Entities.CollectionDirty -= Entities_CollectionDirty;

            base.Entities.Import(newEntities, this);

            base.Entities.CollectionDirty += Entities_CollectionDirty;
        }

        //Graphical Part
        public void InitializeChunkBuffers()
        {
            CubeVerticeDico = new Dictionary<long, int>();
            SolidCubeVertices = new List<VertexCubeSolid>();
            SolidCubeIndices = new List<ushort>();
            LiquidCubeVertices = new List<VertexCubeLiquid>();
            LiquidCubeIndices = new List<ushort>();

            StaticSpritesIndices = new List<ushort>();
            StaticSpritesVertices = new List<VertexSprite3D>();
        }

        public void SendCubeMeshesToBuffers()
        {
            SendSolidCubeMeshToGraphicCard();       //Solid Cubes
            SendLiquidCubeMeshToGraphicCard();      //See Through Cubes
            SendStaticEntitiesToGraphicalCard();    //Static Entities Sprite + Voxel
            State = ChunkState.DisplayInSyncWithMeshes;
            Ready2Draw = true;
        }

        //Solid Cube
        //Create the VBuffer + IBuffer from the List, then clear the list
        //The Buffers are pushed to the graphic card. (SetData());
        private void SendSolidCubeMeshToGraphicCard()
        {
            lock (Lock_DrawChunksSolidFaces)
            {
                if (SolidCubeVertices.Count == 0)
                {
                    if (SolidCubeVB != null) SolidCubeVB.Dispose();
                    SolidCubeVB = null;
                    return;
                }

                if (SolidCubeVB == null)
                {
                    SolidCubeVB = new VertexBuffer<VertexCubeSolid>(_d3dEngine, SolidCubeVertices.Count, VertexCubeSolid.VertexDeclaration, PrimitiveTopology.TriangleList, "SolidCubeVB", ResourceUsage.Default, 10);
                }
                SolidCubeVB.SetData(SolidCubeVertices.ToArray());
                SolidCubeVertices.Clear();

                if (SolidCubeIB == null)
                {
                    SolidCubeIB = new IndexBuffer<ushort>(_d3dEngine, SolidCubeIndices.Count, SharpDX.DXGI.Format.R16_UInt , "SolidCubeIB");
                }
                SolidCubeIB.SetData(SolidCubeIndices.ToArray());
                SolidCubeIndices.Clear();

                CubeVerticeDico.Clear();
            }
        }

        //Liquid Cube
        //Create the VBuffer + IBuffer from the List, then clear the list
        //The Buffers are pushed to the graphic card. (SetData());
        private void SendLiquidCubeMeshToGraphicCard()
        {
            lock (Lock_DrawChunksSeeThrough1Faces)
            {
                if (LiquidCubeVertices.Count == 0)
                {
                    if (LiquidCubeVB != null) LiquidCubeVB.Dispose();
                    LiquidCubeVB = null;
                    return;
                }

                if (LiquidCubeVB == null)
                {
                    LiquidCubeVB = new VertexBuffer<VertexCubeLiquid>(_d3dEngine, LiquidCubeVertices.Count, VertexCubeLiquid.VertexDeclaration, PrimitiveTopology.TriangleList, "LiquidCubeVB", ResourceUsage.Default, 10);
                }
                LiquidCubeVB.SetData(LiquidCubeVertices.ToArray());
                LiquidCubeVertices.Clear();

                if (LiquidCubeIB == null)
                {
                    LiquidCubeIB = new IndexBuffer<ushort>(_d3dEngine, LiquidCubeIndices.Count, SharpDX.DXGI.Format.R16_UInt,"LiquidCubeIB");
                }
                LiquidCubeIB.SetData(LiquidCubeIndices.ToArray());
                LiquidCubeIndices.Clear();
            }
        }

        private void SendStaticEntitiesToGraphicalCard()
        {
            lock (Lock_Draw)
            {
                if (StaticSpritesVertices.Count == 0)
                {
                    if (StaticSpritesVB != null) StaticSpritesVB.Dispose();
                    StaticSpritesVB = null;
                    return;
                }

                if (StaticSpritesVB == null)
                {
                    StaticSpritesVB = new VertexBuffer<VertexSprite3D>(_d3dEngine, StaticSpritesVertices.Count, VertexSprite3D.VertexDeclaration, PrimitiveTopology.TriangleList, "StaticEntityVB", ResourceUsage.Default, 5);
                }
                StaticSpritesVB.SetData(StaticSpritesVertices.ToArray());
                StaticSpritesVertices.Clear();

                if (StaticSpritesIB == null)
                {
                    StaticSpritesIB = new IndexBuffer<ushort>(_d3dEngine, StaticSpritesIndices.Count, SharpDX.DXGI.Format.R16_UInt, "StaticEntityIB");
                }
                StaticSpritesIB.SetData(StaticSpritesIndices.ToArray());
                StaticSpritesIndices.Clear();
            }
        }


        //Ask the Graphical card to Draw the solid faces
        public void DrawSolidFaces()
        {
            lock (Lock_DrawChunksSolidFaces)
            {
                if (SolidCubeVB != null)
                {
                    SolidCubeVB.SetToDevice(0);
                    SolidCubeIB.SetToDevice(0);
                    _d3dEngine.Context.DrawIndexed(SolidCubeIB.IndicesCount, 0, 0);
                }
            }
        }

        //Ask the Graphical card to Draw the solid faces
        public void DrawLiquidFaces()
        {
            lock (Lock_DrawChunksSeeThrough1Faces)
            {
                if (LiquidCubeVB != null)
                {
                    LiquidCubeVB.SetToDevice(0);
                    LiquidCubeIB.SetToDevice(0);
                    _d3dEngine.Context.DrawIndexed(LiquidCubeIB.IndicesCount, 0, 0);
                }
            }
        }

        //Ask the Graphical card to Draw the solid faces
        public void DrawStaticEntities()
        {
            lock (Lock_Draw)
            {
                if (StaticSpritesVB != null)
                {
                    StaticSpritesVB.SetToDevice(0);
                    StaticSpritesIB.SetToDevice(0);
                    _d3dEngine.Context.DrawIndexed(StaticSpritesIB.IndicesCount, 0, 0);
                }
            }
        }

        public void RefreshVisualEntities()
        {
            //Create the Sprite Entities
            VisualSpriteEntities.Clear();

            foreach (var spriteEntity in Entities.Enumerate<SpriteEntity>())
            {
                VisualSpriteEntities.Add(new VisualSpriteEntity(spriteEntity));
            }

            Entities.IsDirty = false;
        }

        private void Entities_CollectionDirty(object sender, EventArgs e)
        {
            RefreshVisualEntities();
            _entityPickingManager.isDirty = true; //Tell the Picking manager that it must force the picking entity list !
            //Change chunk state in order to rebuild the Entity collections change
            State = ChunkState.LandscapeCreated;
            ThreadPriority = Amib.Threading.WorkItemPriority.Highest;
            UserChangeOrder = 1;
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
            if (X == _visualWorldParameters.WorldRange.Min.X ||
               Z == _visualWorldParameters.WorldRange.Min.Z ||
               X == _visualWorldParameters.WorldRange.Max.X - AbstractChunk.ChunkSize.X ||
               Z == _visualWorldParameters.WorldRange.Max.Z - AbstractChunk.ChunkSize.Z)
            {
                return true;
            }
            return false;
        }

        private void RefreshWorldMatrix()
        {
            Matrix.Translation(_cubeRange.Min.X, _cubeRange.Min.Y, _cubeRange.Min.Z, out World); //Create a matrix for world translation

            //Refresh the bounding Box to make it in world coord.
            ChunkWorldBoundingBox.Minimum = new Vector3(_cubeRange.Min.X, _cubeRange.Min.Y, _cubeRange.Min.Z);
            ChunkWorldBoundingBox.Maximum = new Vector3(_cubeRange.Max.X, _cubeRange.Max.Y, _cubeRange.Max.Z);

#if DEBUG
            //ChunkBoundingBoxDisplay.Update(ref ChunkWorldBoundingBox);
#endif
        }

        private void RangeChanged() // Start it also if the World offset Change !!!
        {
            ChunkID = (((Int64)_cubeRange.Min.X) << 32) + _cubeRange.Min.Z;

            ChunkPositionBlockUnit = new Vector2I() { X = _cubeRange.Min.X, Y = _cubeRange.Min.Z };
            ChunkPosition = new Vector2I() { X = _cubeRange.Min.X / AbstractChunk.ChunkSize.X, Y = _cubeRange.Min.Z / AbstractChunk.ChunkSize.Z };
            RefreshWorldMatrix();

            ChunkCenter = new Vector3D(_cubeRange.Min.X + (_cubeRange.Max.X - _cubeRange.Min.X) / 2.0,
                                       _cubeRange.Min.Y + (_cubeRange.Max.Y - _cubeRange.Min.Y) / 2.0,
                                       _cubeRange.Min.Z + (_cubeRange.Max.Z - _cubeRange.Min.Z) / 2.0);

            VisualSpriteEntities.Clear();
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
#if DEBUG
            ChunkBoundingBoxDisplay.Dispose();
#endif
            if (SolidCubeVB != null) SolidCubeVB.Dispose();
            if (SolidCubeIB != null) SolidCubeIB.Dispose();
            if (LiquidCubeVB != null) LiquidCubeVB.Dispose();
            if (LiquidCubeIB != null) LiquidCubeIB.Dispose();
            base.Entities.CollectionDirty -= Entities_CollectionDirty;

        }
    }
}
