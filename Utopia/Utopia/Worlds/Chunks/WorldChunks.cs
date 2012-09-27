using System;
using System.Collections.Generic;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.World;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.Chunks.ChunkLandscape;
using Utopia.Worlds.Chunks.ChunkMesh;
using Utopia.Worlds.Chunks.ChunkWrapper;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Network;
using Utopia.Entities.Managers;
using Utopia.Worlds.Storage;
using Utopia.Worlds.SkyDomes;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Worlds.Weather;
using Utopia.Effects.Shared;
using SharpDX;
using Utopia.Shared.Settings;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.States;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using S33M3CoreComponents.Physics.Verlet;
using SharpDX.Direct3D11;
using Utopia.Components;
using Utopia.Entities.Voxel;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;

namespace Utopia.Worlds.Chunks
{
    public enum ChunkState
    {
        Empty                            = 0,
        LandscapeCreated                 = 1,
        LightsSourceCreated              = 2,
        InnerLightsSourcePropagated      = 3,
        OuterLightSourcesProcessed       = 4,
        MeshesChanged                    = 5,
        DisplayInSyncWithMeshes          = 6,
        UserChanged                      = 7
    }

    public enum ChunksThreadSyncMode
    {
        UpdateReadyForLightPropagation,
        UpdateReadyForMeshCreation,
        HighPriorityReadyToBeSendToGraphicalCard,
        ReadyForWrapping
    }

    /// <summary>
    /// Will contains world block landscape stored as Chunks.
    /// </summary>
    public partial class WorldChunks : DrawableGameComponent, IWorldChunks
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly int SOLID_DRAW = 0;
        private readonly int TRANSPARENT_DRAW;
        private readonly int ENTITIES_DRAW;

        #region Private variables
        private D3DEngine _d3dEngine;
        private CameraManager<ICameraFocused> _camManager;
        private GameStatesManager _gameStates;
        private SingleArrayChunkContainer _cubesHolder;
        private IClock _gameClock;
        private WorldFocusManager _worldFocusManager;
        private IChunksWrapper _chunkWrapper;
        private ILightingManager _lightingManager;
        private ILandscapeManager _landscapeManager;
        private IChunkMeshManager _chunkMeshManager;
        private ServerComponent _server;
        private PlayerEntityManager _playerManager;
        private IChunkStorageManager _chunkstorage;
        private ISkyDome _skydome;
        private IWeather _weather;
        private SharedFrameCB _sharedFrameCB;
        private IEntityPickingManager _pickingManager;
        private int _readyToDrawCount;
        private StaggingBackBuffer _solidBackBuffer;
        private StaggingBackBuffer _skyBackBuffer;
        private readonly object _counterLock = new object();
        private VoxelModelManager _voxelModelManager;
        private IChunkEntityImpactManager _chunkEntityImpactManager;

        /// <summary>
        /// List of chunks that still _slowly_ appearing
        /// </summary>
        private List<VisualChunk> _transparentChunks = new List<VisualChunk>();

        #endregion

        #region Public Property/Variables
        /// <summary> The chunk collection </summary>
        public VisualChunk[] Chunks { get; set; }
        public VisualChunk[] SortedChunks { get; set; }

        public bool ChunkNeed2BeSorted { get; set; }
        public int StaticEntityViewRange { get; set; }

        /// <summary>
        /// Gets or sets value indicating if static entities should be drawn using instancing
        /// </summary>
        public bool DrawStaticInstanced { get; set; }

        /// <summary> World parameters </summary>
        public VisualWorldParameters VisualWorldParameters { get; set; }

        public ILandscapeManager LandscapeManager
        {
            get { return _landscapeManager; }
        }

        #endregion

        /// <summary>
        /// Occurs when array of visual chunks get initialized
        /// </summary>
        public event EventHandler ChunksArrayInitialized;
        private void OnChunksArrayInitialized()
        {
            if (ChunksArrayInitialized != null) ChunksArrayInitialized(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when all chunks is loaded
        /// </summary>
        public event EventHandler LoadComplete;
        private void OnInitialLoadComplete()
        {
            if (LoadComplete != null) LoadComplete(this, EventArgs.Empty);
            _playerManager.LandscapeInitiazed = true;
        }
        public bool IsInitialLoadCompleted { get; set; }

        public WorldChunks(D3DEngine d3dEngine, 
                           CameraManager<ICameraFocused> camManager,
                           VisualWorldParameters visualWorldParameters,
                           WorldFocusManager worldFocusManager,
                           GameStatesManager gameStates, 
                           IClock gameClock, 
                           SingleArrayChunkContainer cubesHolder,
                           ILandscapeManager landscapeManager,
                           IChunkMeshManager chunkMeshManager,
                           IChunksWrapper chunkWrapper,
                           ILightingManager lightingManager,
                           IChunkStorageManager chunkstorage,
                           ServerComponent server,
                           PlayerEntityManager player,
                           ISkyDome skydome,
                           IEntityPickingManager pickingManager,
                           IWeather weather,
                           SharedFrameCB sharedFrameCB,
                           StaggingBackBuffer solidBackBuffer,
                           StaggingBackBuffer skyBackBuffer,
                           VoxelModelManager voxelModelManager,
                           IChunkEntityImpactManager chunkEntityImpactManager
            )
        {
            _server = server;
            _chunkstorage = chunkstorage;
            _d3dEngine = d3dEngine;
            _worldFocusManager = worldFocusManager;
            _gameStates = gameStates;
            _camManager = camManager;
            _gameClock = gameClock;
            VisualWorldParameters = visualWorldParameters;
            _cubesHolder = cubesHolder;
            _chunkWrapper = chunkWrapper;
            _landscapeManager = landscapeManager;
            _chunkMeshManager = chunkMeshManager;
            _lightingManager = lightingManager;
            _playerManager = player;
            _skydome = skydome;
            _weather = weather;
            _sharedFrameCB = sharedFrameCB;
            _pickingManager = pickingManager;
            _solidBackBuffer = solidBackBuffer;
            _skyBackBuffer = skyBackBuffer;
            _voxelModelManager = voxelModelManager;
            _chunkEntityImpactManager = chunkEntityImpactManager;

            DrawStaticInstanced = true;

            //Self injecting inside components, to avoid circular dependency
            _chunkWrapper.WorldChunks = this;
            pickingManager.WorldChunks = this;
            lightingManager.WorldChunk = this;
            _playerManager.WorldChunks = this;
            _chunkMeshManager.WorldChunks = this;

            DrawOrders.UpdateIndex(SOLID_DRAW, 100, "SOLID_DRAW");
            TRANSPARENT_DRAW = DrawOrders.AddIndex(1050, "TRANSPARENT_DRAW");
            ENTITIES_DRAW = DrawOrders.AddIndex(101, "ENTITIES_DRAW");

            //Subscribe to chunk modifications
            _cubesHolder.BlockDataChanged += ChunkCubes_BlockDataChanged;
        }

        #region Public methods

        public override void Initialize()
        {
            IsInitialLoadCompleted = false;
            _readyToDrawCount = 0;

            if (ClientSettings.Current.Settings.GraphicalParameters.StaticEntityViewSize > (ClientSettings.Current.Settings.GraphicalParameters.WorldSize / 2) - 2.5)
            {
                StaticEntityViewRange = (int)((ClientSettings.Current.Settings.GraphicalParameters.WorldSize / 2) - 2.5) * 16;
            }
            else
            {
                StaticEntityViewRange = ClientSettings.Current.Settings.GraphicalParameters.StaticEntityViewSize * 16;
            }

            InitChunks();
            InitWrappingVariables();
        }

        public override void LoadContent(DeviceContext context)
        {
            S33M3DXEngine.Threading.SmartThread.IsBoostMode = true;

            IntilializeUpdateble();
            InitDrawComponents(context);
        }

        public override void BeforeDispose()
        {
            foreach (var chunk in Chunks)
            {
                if (chunk != null)
                {
                    chunk.ReadyToDraw -= ChunkReadyToDraw;
                    chunk.Dispose();
                }
            }

            Chunks = null;
            SortedChunks = null;

            DisposeDrawComponents();

        }

        public override void UnloadContent()
        {
            this.DisableComponent();

            foreach (var chunk in Chunks)
            {
                if (chunk != null)
                {
                    chunk.ReadyToDraw -= ChunkReadyToDraw;
                    chunk.Dispose();
                }
            }

            this.IsInitialLoadCompleted = false;
            this.IsInitialized = false;
        }

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <returns></returns>
        public VisualChunk GetChunk(int X, int Z)
        {
            //From World Coord to Cube Array Coord
            int arrayX = MathHelper.Mod(X, VisualWorldParameters.WorldVisibleSize.X);
            int arrayZ = MathHelper.Mod(Z, VisualWorldParameters.WorldVisibleSize.Z);

            //From Cube Array coord to Chunk Array coord
            int chunkX = arrayX >> VisualWorldParameters.ChunkPOWsize;
            int chunkZ = arrayZ >> VisualWorldParameters.ChunkPOWsize;

            return Chunks[chunkX + chunkZ * VisualWorldParameters.VisibleChunkInWorld.X];
        }

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <returns></returns>
        public VisualChunk GetChunk(ref Vector3I cubePosition)
        {
            return GetChunk(cubePosition.X, cubePosition.Z);
        }

        /// <summary>
        /// Get a world's chunk from a chunk location in world coordinate
        /// </summary>
        /// <param name="X">Chunk X coordinate</param>
        /// <param name="Z">Chunk Z coordinate</param>
        /// <returns></returns>
        public VisualChunk GetChunkFromChunkCoord(int X, int Z)
        {
            //From Chunk coordinate to World Coordinate
            X *= AbstractChunk.ChunkSize.X;
            Z *= AbstractChunk.ChunkSize.Z;

            return GetChunk(X, Z);
        }

        /// <summary>
        /// Get a world's chunk from a chunk location in world coordinate
        /// </summary>
        /// <param name="X">Chunk X coordinate</param>
        /// <param name="Z">Chunk Z coordinate</param>
        /// <returns></returns>
        public VisualChunk[] GetsurroundingChunkFromChunkCoord(int X, int Z)
        {
            VisualChunk[] surroundingChunk = new VisualChunk[8];

            surroundingChunk[0] = GetChunkFromChunkCoord(X + 1, Z);
            surroundingChunk[1] = GetChunkFromChunkCoord(X + 1, Z + 1);
            surroundingChunk[2] = GetChunkFromChunkCoord(X, Z + 1);
            surroundingChunk[3] = GetChunkFromChunkCoord(X - 1, Z + 1);
            surroundingChunk[4] = GetChunkFromChunkCoord(X - 1, Z);
            surroundingChunk[5] = GetChunkFromChunkCoord(X - 1, Z - 1);
            surroundingChunk[6] = GetChunkFromChunkCoord(X, Z - 1);
            surroundingChunk[7] = GetChunkFromChunkCoord(X + 1, Z - 1);

            return surroundingChunk;
        }

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate with out of array check
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <param name="chunk">The VisualChunk return</param>
        /// <returns>True if the chunk was found</returns>
        public bool GetSafeChunk(float X, float Z, out VisualChunk chunk)
        {
            return GetSafeChunk((int)X, (int)Z, out chunk);
        }

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate with out of array check
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <param name="chunk">The VisualChunk return</param>
        /// <returns>True if the chunk was found</returns>
        public bool GetSafeChunk(int X, int Z, out VisualChunk chunk)
        {
            if (X < VisualWorldParameters.WorldRange.Position.X || X > VisualWorldParameters.WorldRange.Max.X || Z < VisualWorldParameters.WorldRange.Position.Z || Z > VisualWorldParameters.WorldRange.Max.Z)
            {
                chunk = null;
                return false;
            }

            chunk = GetChunk(X, Z);
            return true;
        }

        /// <summary>
        /// Get the list of chunks for a specific X world coordinate
        /// </summary>
        /// <param name="FixedX">The Fixed "line" of chunk to retrieve</param>
        /// <param name="WorldMinZ">Get All chunk From the WorldMinZ value to MaxLineZ-WorldMinZ (Excluded)</param>
        /// <returns></returns>
        public IEnumerable<VisualChunk> GetChunksWithFixedX(int FixedX, int WorldMinZ)
        {
            int Z;
            Z = WorldMinZ;
            for (int chunkInd = 0; chunkInd < VisualWorldParameters.VisibleChunkInWorld.Y; chunkInd++)
            {
                yield return GetChunk(FixedX, Z);
                Z += AbstractChunk.ChunkSize.Z;
            }
        }

        /// <summary>
        /// Get the list of chunks for a specific Z world coordinate
        /// </summary>
        /// <param name="FixedZ">The Fixed "line" of chunk to retrieve</param>
        /// <param name="WorldMinX">Get All chunk From the WorldMinX value to MaxLineX-WorldMinX (Excluded)</param>
        /// <returns></returns>
        public IEnumerable<VisualChunk> GetChunksWithFixedZ(int FixedZ, int WorldMinX)
        {
            int X;
            X = WorldMinX;
            for (int chunkInd = 0; chunkInd < VisualWorldParameters.VisibleChunkInWorld.X; chunkInd++)
            {
                yield return GetChunk(X, FixedZ);
                X += AbstractChunk.ChunkSize.X;
            }
        }

        /// <summary>
        /// indicate if the Chunk coordinate passed in is the border of the visible world
        /// </summary>
        /// <returns></returns>
        public bool isBorderChunk(Vector2I chunkPosition)
        {
            if (chunkPosition.X == VisualWorldParameters.WorldRange.Position.X ||
               chunkPosition.Y == VisualWorldParameters.WorldRange.Position.Z ||
               chunkPosition.X == VisualWorldParameters.WorldRange.Max.X - AbstractChunk.ChunkSize.X ||
               chunkPosition.Y == VisualWorldParameters.WorldRange.Max.Z - AbstractChunk.ChunkSize.Z)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Validate player move against surrounding landscape, if move not possible, it will be "rollbacked"
        /// It's used by the physic engine
        /// </summary>
        /// <param name="newPosition2Evaluate"></param>
        /// <param name="previousPosition"></param>
        public void isCollidingWithTerrain(VerletSimulator _physicSimu, ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {
            BoundingBox _boundingBox2Evaluate;
            Vector3D newPositionWithColliding = previousPosition;
            TerraCubeWithPosition _collidingCube;

            if (Vector3D.Distance(previousPosition, newPosition2Evaluate) < 0.0001) return;
            //Create a Bounding box with my new suggested position, taking only the X that has been changed !
            //X Testing =====================================================
            newPositionWithColliding.X = newPosition2Evaluate.X;
            _boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());

            //If my new X position, make me placed "inside" a block, then invalid the new position
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate, true, out _collidingCube))
            {
                newPositionWithColliding.X = previousPosition.X;
                if (_collidingCube.CubeProfile.YBlockOffset > 0 || _playerManager.PlayerOnOffsettedBlock > 0)
                {
                    float offsetValue = (float)((1 - _collidingCube.CubeProfile.YBlockOffset));
                    if (_playerManager.PlayerOnOffsettedBlock > 0) offsetValue -= (1 - _playerManager.PlayerOnOffsettedBlock);
                    if (offsetValue <= 0.5)
                    {
                        _playerManager.OffsetBlockHitted = offsetValue;
                    }
                }

                //if (previousPosition.X < newPosition2Evaluate.X)
                //{
                //    newPositionWithColliding.X = ((int)newPosition2Evaluate.X) + 1 - localEntityBoundingBox.Maximum.X - 0.01;
                //}
                //else
                //{
                //    newPositionWithColliding.X = ((int)newPosition2Evaluate.X) + localEntityBoundingBox.Maximum.X;
                //}
            }

            //Z Testing =========================================================
            newPositionWithColliding.Z = newPosition2Evaluate.Z;
            _boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());

            //If my new Z position, make me placed "inside" a block, then invalid the new position
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate, true, out _collidingCube))
            {
                newPositionWithColliding.Z = previousPosition.Z;
                if (_collidingCube.CubeProfile.YBlockOffset > 0 || _playerManager.PlayerOnOffsettedBlock > 0)
                {
                    float offsetValue = (float)((1 - _collidingCube.CubeProfile.YBlockOffset));
                    if (_playerManager.PlayerOnOffsettedBlock > 0) offsetValue -= (1 - _playerManager.PlayerOnOffsettedBlock);
                    if (offsetValue <= 0.5)
                    {
                        _playerManager.OffsetBlockHitted = offsetValue;
                    }
                }

                //if (previousPosition.Z < newPosition2Evaluate.Z)
                //{
                //    newPositionWithColliding.Z = ((int)newPosition2Evaluate.Z) + 1 - localEntityBoundingBox.Maximum.Z - 0.01;
                //}
                //else
                //{
                //    newPositionWithColliding.Z = ((int)newPosition2Evaluate.Z) + localEntityBoundingBox.Maximum.Z;
                //}
            }

            //Y Testing ======================================================
            newPositionWithColliding.Y = newPosition2Evaluate.Y;
            _boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());

            //If my new Y position, make me placed "inside" a block, then invalid the new position
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate, true, out _collidingCube))
            {
                //If was Jummping "before" entering inside the cube
                if (previousPosition.Y >= newPositionWithColliding.Y)
                {
                    //If the movement between 2 Y is too large, use the GroundBelowEntity value
                    if (Math.Abs(newPositionWithColliding.Y - previousPosition.Y) > 1)
                    {
                        previousPosition.Y = _playerManager.GroundBelowEntity;
                    }
                    else
                    {
                        //Raise Up until the Ground, next the previous position
                        if (_collidingCube.CubeProfile.YBlockOffset > 0)
                        {
                            previousPosition.Y = MathHelper.Fastfloor(previousPosition.Y + 1) - _collidingCube.CubeProfile.YBlockOffset;
                        }
                        else
                        {
                            previousPosition.Y = MathHelper.Fastfloor(previousPosition.Y);
                        }
                    }

                    _playerManager.OffsetBlockHitted = 0;
                    _physicSimu.OnGround = true; // On ground ==> Activite the force that will counter the gravity !!
                }

                newPositionWithColliding.Y = previousPosition.Y;

            }

            //Check to see if new destination is not blocking me
            _boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate, true, out _collidingCube))
            {
                newPositionWithColliding = previousPosition;
                newPositionWithColliding.Y += 0.1;
            }

            newPosition2Evaluate = newPositionWithColliding;
        }

        //Return true if the position is not solid to player
        public bool ValidatePosition(ref Vector3D newPosition2Evaluate)
        {
           return _cubesHolder.IsSolidToPlayer(ref newPosition2Evaluate) == false;
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Initiliaze the chunks array
        /// </summary>
        private void InitChunks()
        {
            //Defining the World Offset, to be used to reference the 2d circular array of dim defined in chunk
            VisualWorldParameters.WorldRange = new Range3I()
            {
                Position = new Vector3I(VisualWorldParameters.WorldChunkStartUpPosition.X, 0, VisualWorldParameters.WorldChunkStartUpPosition.Y),
                Size = VisualWorldParameters.WorldVisibleSize
            };

            //Create the chunks that will be used as "Rendering" array
            Chunks = new VisualChunk[VisualWorldParameters.VisibleChunkInWorld.X * VisualWorldParameters.VisibleChunkInWorld.Y];
            SortedChunks = new VisualChunk[VisualWorldParameters.VisibleChunkInWorld.X * VisualWorldParameters.VisibleChunkInWorld.Y];

            Range3I cubeRange; //Used to define the blocks inside the chunks
            int arrayX, arrayZ;   //Chunk Array indexes
            VisualChunk chunk;

            //Chunk Server request variables
            List<Vector2I> chunkPosition = new List<Vector2I>();
            List<Md5Hash> chunkHash = new List<Md5Hash>();
            Md5Hash chunkMD5;

            for (int chunkX = 0; chunkX < VisualWorldParameters.VisibleChunkInWorld.X; chunkX++)
            {
                for (int chunkZ = 0; chunkZ < VisualWorldParameters.VisibleChunkInWorld.Y; chunkZ++)
                {
                    cubeRange = new Range3I()
                    {
                        Position = new Vector3I(VisualWorldParameters.WorldChunkStartUpPosition.X + (chunkX * AbstractChunk.ChunkSize.X), 0, VisualWorldParameters.WorldChunkStartUpPosition.Y + (chunkZ * AbstractChunk.ChunkSize.Z)),
                        Size = AbstractChunk.ChunkSize
                        //Max = new Vector3I(VisualWorldParameters.WorldChunkStartUpPosition.X + ((chunkX + 1) * AbstractChunk.ChunkSize.X), AbstractChunk.ChunkSize.Y, VisualWorldParameters.WorldChunkStartUpPosition.Y + ((chunkZ + 1) * AbstractChunk.ChunkSize.Z))
                    };

                    arrayX = MathHelper.Mod(cubeRange.Position.X, VisualWorldParameters.WorldVisibleSize.X);
                    arrayZ = MathHelper.Mod(cubeRange.Position.Z, VisualWorldParameters.WorldVisibleSize.Z);

                    //Create the new VisualChunk
                    chunk = new VisualChunk(_d3dEngine, _worldFocusManager, VisualWorldParameters, ref cubeRange, _cubesHolder, _pickingManager, _camManager, this, _voxelModelManager, _chunkEntityImpactManager);
                    chunk.IsServerRequested = true;
                    //Ask the chunk Data to the DB, in case my local MD5 is equal to the server one.
                    chunk.StorageRequestTicket = _chunkstorage.RequestDataTicket_async(chunk.ChunkID);

                    chunk.ReadyToDraw += ChunkReadyToDraw;

                    //Store this chunk inside the arrays.
                    Chunks[(arrayX >> VisualWorldParameters.ChunkPOWsize) + (arrayZ >> VisualWorldParameters.ChunkPOWsize) * VisualWorldParameters.VisibleChunkInWorld.X] = chunk;
                    SortedChunks[(arrayX >> VisualWorldParameters.ChunkPOWsize) + (arrayZ >> VisualWorldParameters.ChunkPOWsize) * VisualWorldParameters.VisibleChunkInWorld.X] = chunk;

                    //Is this chunk inside the Client storae manager ?
                    if (_chunkstorage.ChunkHashes.TryGetValue(chunk.ChunkID, out chunkMD5))
                    {
                        chunkPosition.Add(new Vector2I((VisualWorldParameters.WorldChunkStartUpPosition.X + (chunkX * AbstractChunk.ChunkSize.X)) / AbstractChunk.ChunkSize.X,
                                                       (VisualWorldParameters.WorldChunkStartUpPosition.Y + (chunkZ * AbstractChunk.ChunkSize.Z)) / AbstractChunk.ChunkSize.Z));
                        chunkHash.Add(chunkMD5);
                    }
                }
            }


            Range2I chunkRange = new Range2I(
                    new Vector2I(
                        VisualWorldParameters.WorldChunkStartUpPosition.X / AbstractChunk.ChunkSize.X,
                        VisualWorldParameters.WorldChunkStartUpPosition.Y / AbstractChunk.ChunkSize.Z
                        ),
                    new Vector2I(
                        VisualWorldParameters.VisibleChunkInWorld.X,
                        VisualWorldParameters.VisibleChunkInWorld.Y
                        )
                    );

#if DEBUG
            logger.Trace("Chunk bulk request to server (Init Phases, data in chunk unit) position : {0} ; Size : {1}", chunkRange.Position, chunkRange.Size);
#endif

            _server.ServerConnection.SendAsync(
            new GetChunksMessage()
            {
                Range = chunkRange,
                Md5Hashes = chunkHash.ToArray(),
                Positions = chunkPosition.ToArray(),
                HashesCount = chunkHash.Count,
                Flag = GetChunksMessageFlag.DontSendChunkDataIfNotModified
            }
            );

            ChunkNeed2BeSorted = true; // Will force the SortedChunks array to be sorted against the "camera position" (The player).

            OnChunksArrayInitialized();
        }

        //Call everytime a chunk has been initialized (= New chunk rebuild form scratch).
        void ChunkReadyToDraw(object sender, EventArgs e)
        {
            var chunk = (VisualChunk)sender;
            chunk.PopUpValue.Initialize(0.5f);

            _transparentChunks.Add(chunk);

            if (IsInitialLoadCompleted) return;

            lock (_counterLock)
            {
                _readyToDrawCount++;
                if (_readyToDrawCount == Chunks.Length && IsInitialLoadCompleted == false)
                {
                    S33M3DXEngine.Threading.SmartThread.IsBoostMode = false;
                    IsInitialLoadCompleted = true;
                    OnInitialLoadComplete();
                }
            }
        }

        /// <summary>
        /// Initiliaze the WrapEnd variable. Is not needed if the starting world point is (0,X,0).
        /// </summary>
        private void InitWrappingVariables()
        {
            //Find the next number where mod == 0 !
            int XWrap = VisualWorldParameters.WorldChunkStartUpPosition.X;
            int ZWrap = VisualWorldParameters.WorldChunkStartUpPosition.Y;

            while (MathHelper.Mod(XWrap, VisualWorldParameters.WorldVisibleSize.X) != 0) XWrap++;
            while (MathHelper.Mod(ZWrap, VisualWorldParameters.WorldVisibleSize.Z) != 0) ZWrap++;

            VisualWorldParameters.WrapEnd = new Vector2I(XWrap, ZWrap);
        }


        #endregion

        #region Events Handling
        /// <summary>
        /// Fired when a block is change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ChunkCubes_BlockDataChanged(object sender, ChunkDataProviderDataChangedEventArgs e)
        {
            //Make the Chunk's block "Dirty"
        }
        #endregion

        public bool ShowDebugInfo { get; set; }
        public string GetDebugInfo()
        {
            if (ShowDebugInfo)
            {

                var c = GetChunk((int)_playerManager.CameraWorldPosition.X, (int)_playerManager.CameraWorldPosition.Z);
                            //From World Coord to Cube Array Coord
                int arrayX = MathHelper.Mod((int)_playerManager.CameraWorldPosition.X, AbstractChunk.ChunkSize.X);
                int arrayZ = MathHelper.Mod((int)_playerManager.CameraWorldPosition.Z, AbstractChunk.ChunkSize.Z);
                var columnInfo = c.BlockData.GetColumnInfo(new Vector2I(arrayX,arrayZ));

                int BprimitiveCount = 0;
                int VprimitiveCount = 0;
                VisualChunk chunk;
                //Run over all chunks to see their status, and take action accordingly.
                for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
                {
                    chunk = SortedChunks[chunkIndice];
                    if (chunk.isFrustumCulled == false)
                    {
                        if (chunk.SolidCubeIB != null) VprimitiveCount += chunk.SolidCubeIB.IndicesCount;
                        if (chunk.LiquidCubeIB != null) VprimitiveCount += chunk.LiquidCubeIB.IndicesCount;
                    }
                    if (chunk.SolidCubeIB != null) BprimitiveCount += chunk.SolidCubeIB.IndicesCount;
                    if (chunk.LiquidCubeIB != null) BprimitiveCount += chunk.LiquidCubeIB.IndicesCount;

                }

                var line0 = string.Format("Nbr chunks : {0:000}, Nbr Visible chunks : {1:000}, {2:0000000} Buffered indices, {3:0000000} Visible indices, Temperature {4:0.00}, Moisture {5:0.00}", SortedChunks.Length, _chunkDrawByFrame, BprimitiveCount, VprimitiveCount, columnInfo.Temperature / 255.0f, columnInfo.Moisture / 255.0f);

                var line1 = string.Format("Static entity draw calls {2}: {0}, time {1}", _staticEntityDrawCalls, _staticEntityDrawTime, DrawStaticInstanced ? "[INSTANCED]": "");

                return string.Join("\r\n", line0, line1);
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
