using System;
using System.Collections.Generic;
using System.Linq;
using S33M3CoreComponents.Sound;
using Utopia.Entities.Managers;
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
using Utopia.Worlds.Storage;
using Utopia.Worlds.SkyDomes;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Worlds.Weather;
using Utopia.Shared.Settings;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.States;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using SharpDX.Direct3D11;
using Utopia.Components;
using Utopia.Entities.Voxel;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Ninject;
using Utopia.Shared.Configuration;
using S33M3CoreComponents.Inputs;
using Utopia.Worlds.Shadows;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Worlds.Chunks
{
    public enum ChunkState
    {
        Empty = 0,
        LandscapeCreated = 1,
        LightsSourceCreated = 2,
        InnerLightsSourcePropagated = 3,
        OuterLightSourcesProcessed = 4,
        MeshesChanged = 5,
        DisplayInSyncWithMeshes = 6,
        UserChanged = 7
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
        private ILandscapeManager2D _landscapeManager;
        private IChunkMeshManager _chunkMeshManager;
        private ServerComponent _server;
        private IChunkStorageManager _chunkstorage;
        private IWeather _weather;
        private int _readyToDrawCount;
        private StaggingBackBuffer _skyBackBuffer;
        private readonly object _counterLock = new object();
        private VoxelModelManager _voxelModelManager;
        private IChunkEntityImpactManager _chunkEntityImpactManager;
        private UtopiaProcessorParams _utopiaProcessorParam;
        private InputsManager _inputsManager;
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

        public ILandscapeManager2D LandscapeManager
        {
            get { return _landscapeManager; }
        }

        public bool IsInitialLoadCompleted { get; set; }

        /// <summary>
        /// Gets or sets amount of chunks to display in leveled mode
        /// </summary>
        public int SliceViewChunks { get; set; }

        /// <summary>
        /// Gets current slice value
        /// </summary>
        public int SliceValue { get { return _sliceValue; } }

        #endregion

        /// <summary>
        /// Occurs when array of visual chunks get initialized
        /// </summary>
        public event EventHandler ChunksArrayInitialized;
        private void OnChunksArrayInitialized()
        {
            if (ChunksArrayInitialized != null) 
                ChunksArrayInitialized(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when all chunks is loaded
        /// </summary>
        public event EventHandler LoadComplete;
        private void OnInitialLoadComplete()
        {
            if (LoadComplete != null) 
                LoadComplete(this, EventArgs.Empty);          
        }

        [Inject]
        public ISkyDome Skydome { get; set; }

        [Inject]
        public ISoundEngine SoundEngine { get; set; }

        [Inject]
        public WorldShadowMap ShadowMap { get; set; }

        [Inject]
        public IPlayerManager PlayerManager { get; set; }

        [Inject]
        public SharedFrameCB SharedFrameCb { get; set; }

        public WorldChunks(D3DEngine d3dEngine,
                           CameraManager<ICameraFocused> camManager,
                           VisualWorldParameters visualWorldParameters,
                           WorldFocusManager worldFocusManager,
                           GameStatesManager gameStates,
                           IClock gameClock,
                           SingleArrayChunkContainer cubesHolder,
                           ILandscapeManager2D landscapeManager,
                           IChunkMeshManager chunkMeshManager,
                           IChunksWrapper chunkWrapper,
                           ILightingManager lightingManager,
                           IChunkStorageManager chunkstorage,
                           ServerComponent server,
                           IWeather weather,
                           [Named("SkyBuffer")] StaggingBackBuffer skyBackBuffer,
                           VoxelModelManager voxelModelManager,
                           IChunkEntityImpactManager chunkEntityImpactManager,
                           InputsManager inputsManager
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
            _weather = weather;
            _skyBackBuffer = skyBackBuffer;
            _voxelModelManager = voxelModelManager;
            _chunkEntityImpactManager = chunkEntityImpactManager;
            _inputsManager = inputsManager;

            _skyBackBuffer.OnStaggingBackBufferChanged += _skyBackBuffer_OnStaggingBackBufferChanged;

            SliceViewChunks = 25;

            DrawStaticInstanced = true;

            if (visualWorldParameters.WorldParameters.Configuration is UtopiaWorldConfiguration)
            {
                _utopiaProcessorParam = ((UtopiaWorldConfiguration)visualWorldParameters.WorldParameters.Configuration).ProcessorParam;
            }

            //Self injecting inside components, to avoid circular dependency
            _chunkWrapper.WorldChunks = this;
            lightingManager.WorldChunk = this;
            _chunkMeshManager.WorldChunks = this;
            landscapeManager.WorldChunks = this;
            
            DrawOrders.UpdateIndex(SOLID_DRAW, 100, "SOLID_DRAW");
            TRANSPARENT_DRAW = DrawOrders.AddIndex(1050, "TRANSPARENT_DRAW");
            ENTITIES_DRAW = DrawOrders.AddIndex(101, "ENTITIES_DRAW");

            this.IsDefferedLoadContent = true;
        }

        #region Public methods

        public override void Initialize()
        {
            S33M3DXEngine.Threading.ThreadsManager.IsBoostMode = true;

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

            _skyBackBuffer.OnStaggingBackBufferChanged -= _skyBackBuffer_OnStaggingBackBufferChanged;
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

            Chunks = null;
            SortedChunks = null;

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
        public VisualChunk GetChunk(Vector3I cubePosition)
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

        public VisualChunk GetPlayerChunk()
        {
            return GetChunk(BlockHelper.EntityToBlock(_camManager.ActiveCamera.WorldPosition.Value));
        }

        /// <summary>
        /// Get a world's chunk from a chunk location in world coordinate with array bound test
        /// </summary>
        /// <param name="X">Chunk X coordinate</param>
        /// <param name="Z">Chunk Z coordinate</param>
        /// <returns></returns>
        public bool GetSafeChunkFromChunkCoord(Vector3I chunkPos, out VisualChunk chunk)
        {
            return GetSafeChunkFromChunkCoord(chunkPos.X, chunkPos.Z, out chunk);
        }

        /// <summary>
        /// Get a world's chunk from a chunk location in world coordinate with array bound test
        /// </summary>
        /// <param name="X">Chunk X coordinate</param>
        /// <param name="Z">Chunk Z coordinate</param>
        /// <returns></returns>
        public bool GetSafeChunkFromChunkCoord(int X, int Z, out VisualChunk chunk)
        {
            //From Chunk coordinate to World Coordinate
            X *= AbstractChunk.ChunkSize.X;
            Z *= AbstractChunk.ChunkSize.Z;

            return GetSafeChunk(X, Z, out chunk);
        }

        /// <summary>
        /// Get a world's chunk from a chunk position
        /// </summary>
        /// <param name="chunkPos">chunk space coordinate</param>
        public VisualChunk GetChunkFromChunkCoord(Vector3I chunkPos)
        {
            return GetChunkFromChunkCoord(chunkPos.X, chunkPos.Z);
        }

        /// <summary>
        /// Get a world's chunk from a chunk location in world coordinate
        /// </summary>
        /// <param name="X">Chunk X coordinate</param>
        /// <param name="Z">Chunk Z coordinate</param>
        /// <returns></returns>
        public VisualChunk[] GetEightSurroundingChunkFromChunkCoord(int X, int Z)
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
        /// Get a world's chunk from a chunk location in world coordinate
        /// </summary>
        /// <param name="X">Chunk X coordinate</param>
        /// <param name="Z">Chunk Z coordinate</param>
        /// <returns></returns>
        public VisualChunk[] GetFourSurroundingChunkFromChunkCoord(int X, int Z)
        {
            VisualChunk[] surroundingChunk = new VisualChunk[4];

            surroundingChunk[0] = GetChunkFromChunkCoord(X + 1, Z);
            surroundingChunk[1] = GetChunkFromChunkCoord(X, Z + 1);
            surroundingChunk[2] = GetChunkFromChunkCoord(X - 1, Z);
            surroundingChunk[3] = GetChunkFromChunkCoord(X, Z - 1);

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
            if (X < VisualWorldParameters.WorldRange.Position.X || X >= VisualWorldParameters.WorldRange.Max.X || Z < VisualWorldParameters.WorldRange.Position.Z || Z >= VisualWorldParameters.WorldRange.Max.Z)
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
        /// Enumerates all visible chunks by player (i.e. not frustum culled)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<VisualChunk> VisibleChunks()
        {
            return SortedChunks.Where(chunk => !chunk.Graphics.IsFrustumCulled);
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

        
        //Return true if the position is not solid to player
        public bool ValidatePosition(ref Vector3D newPosition2Evaluate)
        {
            return _cubesHolder.IsSolidToPlayer(ref newPosition2Evaluate) == false;
        }

        public enum GetChunksFilter
        {
            All,                      //Return all chunks
            Visibles,                  //Return all chunks that are visible to the player
            VisibleWithinStaticObjectRange  //Return all chunks that are visible to the player & in the max range of the static object drawing
        }

        public IEnumerable<VisualChunk> GetChunks(GetChunksFilter filter)
        {
            switch (filter)
            {
                case GetChunksFilter.All:
                    for (int i = 0; i < Chunks.Length; i++)
                    {
                        yield return Chunks[i];
                    }
                    break;
                case GetChunksFilter.Visibles:
                    foreach(var chunk in Chunks.Where(x => !x.Graphics.IsFrustumCulled && x.Graphics.IsExistingMesh4Drawing))
                    {
                        yield return chunk;
                    }
                    break;
                case GetChunksFilter.VisibleWithinStaticObjectRange:
                    for (int i = 0; i < SortedChunks.Length; i++)
                    {
                        //Sorted chunk are sorted by DistanceFromPlayer
                        if (SortedChunks[i].DistanceFromPlayer > StaticEntityViewRange) yield break;
                        yield return SortedChunks[i];
                    }
                    break;
            }
            yield break;
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Initiliaze the chunks array
        /// </summary>
        private void InitChunks()
        {
            //Init serverEventArea around player
            var areaSize = _server.GameInformations.AreaSize;

            var notificationAreaSize = new Vector3I(areaSize.X / AbstractChunk.ChunkSize.X, 0, areaSize.Y / AbstractChunk.ChunkSize.Z) + Vector3I.One;
            _eventNotificationArea = new Range3I
            {
                Position = BlockHelper.EntityToChunkPosition(PlayerManager.Player.Position) - notificationAreaSize / 2,
                Size = notificationAreaSize
            };

            //Defining the World Offset, to be used to reference the 2d circular array of dim defined in chunk
            VisualWorldParameters.WorldRange = new Range3I()
            {
                Size = VisualWorldParameters.WorldVisibleSize,
                Position = VisualWorldParameters.WorldChunkStartUpPosition
            };

            //Create the chunks that will be used as "Rendering" array
            Chunks = new VisualChunk[VisualWorldParameters.VisibleChunkInWorld.X * VisualWorldParameters.VisibleChunkInWorld.Y];
            SortedChunks = new VisualChunk[VisualWorldParameters.VisibleChunkInWorld.X * VisualWorldParameters.VisibleChunkInWorld.Y];

            Range3I cubeRange; //Used to define the blocks inside the chunks
            int arrayX, arrayZ;   //Chunk Array indexes
            VisualChunk chunk;

            //Chunk Server request variables
            List<Vector3I> chunkPosition = new List<Vector3I>();
            List<Md5Hash> chunkHash = new List<Md5Hash>();
            Md5Hash chunkMD5;

            for (int chunkX = 0; chunkX < VisualWorldParameters.VisibleChunkInWorld.X; chunkX++)
            {
                for (int chunkZ = 0; chunkZ < VisualWorldParameters.VisibleChunkInWorld.Y; chunkZ++)
                {
                    cubeRange = new Range3I()
                    {
                        Position = new Vector3I(VisualWorldParameters.WorldChunkStartUpPosition.X + (chunkX * AbstractChunk.ChunkSize.X), 0, VisualWorldParameters.WorldChunkStartUpPosition.Z + (chunkZ * AbstractChunk.ChunkSize.Z)),
                        Size = AbstractChunk.ChunkSize
                        //Max = new Vector3I(VisualWorldParameters.WorldChunkStartUpPosition.X + ((chunkX + 1) * AbstractChunk.ChunkSize.X), AbstractChunk.ChunkSize.Y, VisualWorldParameters.WorldChunkStartUpPosition.Y + ((chunkZ + 1) * AbstractChunk.ChunkSize.Z))
                    };

                    arrayX = MathHelper.Mod(cubeRange.Position.X, VisualWorldParameters.WorldVisibleSize.X);
                    arrayZ = MathHelper.Mod(cubeRange.Position.Z, VisualWorldParameters.WorldVisibleSize.Z);

                    //Create the new VisualChunk
                    chunk = new VisualChunk(_d3dEngine, _worldFocusManager, VisualWorldParameters, ref cubeRange, _cubesHolder, _camManager, this, _voxelModelManager, _chunkEntityImpactManager);
                    chunk.IsServerRequested = true;
                    //Ask the chunk Data to the DB, in case my local MD5 is equal to the server one.
                    chunk.StorageRequestTicket = _chunkstorage.RequestDataTicket_async(chunk.Position);

                    chunk.ReadyToDraw += ChunkReadyToDraw;

                    //Store this chunk inside the arrays.
                    Chunks[(arrayX >> VisualWorldParameters.ChunkPOWsize) + (arrayZ >> VisualWorldParameters.ChunkPOWsize) * VisualWorldParameters.VisibleChunkInWorld.X] = chunk;
                    SortedChunks[(arrayX >> VisualWorldParameters.ChunkPOWsize) + (arrayZ >> VisualWorldParameters.ChunkPOWsize) * VisualWorldParameters.VisibleChunkInWorld.X] = chunk;

                    //Is this chunk inside the Client storage manager ?
                    if (_chunkstorage.ChunkHashes.TryGetValue(chunk.Position, out chunkMD5))
                    {
                        chunkPosition.Add(new Vector3I((VisualWorldParameters.WorldChunkStartUpPosition.X + (chunkX * AbstractChunk.ChunkSize.X)) / AbstractChunk.ChunkSize.X, 0,
                                                       (VisualWorldParameters.WorldChunkStartUpPosition.Z + (chunkZ * AbstractChunk.ChunkSize.Z)) / AbstractChunk.ChunkSize.Z));
                        chunkHash.Add(chunkMD5);
                    }

                }
            }


            var chunkRange = new Range3I(
                    new Vector3I(
                        VisualWorldParameters.WorldChunkStartUpPosition.X / AbstractChunk.ChunkSize.X, 
                        0,
                        VisualWorldParameters.WorldChunkStartUpPosition.Z / AbstractChunk.ChunkSize.Z
                        ),
                    new Vector3I(
                        VisualWorldParameters.VisibleChunkInWorld.X,
                        1,
                        VisualWorldParameters.VisibleChunkInWorld.Y
                        )
                    );

#if DEBUG
            logger.Trace("Chunk bulk request to server (Init Phases, data in chunk unit) position : {0} ; Size : {1}", chunkRange.Position, chunkRange.Size);
#endif

            _server.ServerConnection.Send(
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

        /// <summary>
        /// Will start a full client chunk sync with server !
        /// </summary>
        public void ResyncClientChunks()
        {
            List<Vector3I> chunkPosition = new List<Vector3I>();
            List<Md5Hash> chunkHash = new List<Md5Hash>();

            foreach (var chunk in GetChunks(GetChunksFilter.All))
            {
                //Requesting server chunks, for validation

                var md5Hash = chunk.GetMd5Hash();

                chunkPosition.Add(chunk.Position);
                chunkHash.Add(md5Hash);
                chunk.IsServerRequested = true;
                chunk.IsServerResyncMode = true;
            }

            var chunkRange = new Range3I(
                            new Vector3I(
                                VisualWorldParameters.WorldChunkStartUpPosition.X / AbstractChunk.ChunkSize.X,
                                0,
                                VisualWorldParameters.WorldChunkStartUpPosition.Z / AbstractChunk.ChunkSize.Z
                                ),
                            new Vector3I(
                                VisualWorldParameters.VisibleChunkInWorld.X,
                                1,
                                VisualWorldParameters.VisibleChunkInWorld.Y
                                )
                 );

            _server.ServerConnection.Send(
                    new GetChunksMessage()
                    {
                        Range = chunkRange,
                        Md5Hashes = chunkHash.ToArray(),
                        Positions = chunkPosition.ToArray(),
                        HashesCount = chunkHash.Count,
                        Flag = GetChunksMessageFlag.DontSendChunkDataIfNotModified
                    }
            );

        }

        /// <summary>
        /// Will start a chunk resync cycle (Request to server)
        /// </summary>
        /// <param name="chunkPosition">The requested chunk</param>
        /// <param name="forced">If true, will skip some safety checks</param>
        /// <returns></returns>
        public bool ResyncChunk(Vector3I chunkPosition, bool forced)
        {
            var chunk = GetChunkFromChunkCoord(chunkPosition.X, chunkPosition.Z);

            if (forced == false && (chunk.ThreadStatus != S33M3DXEngine.Threading.ThreadsManager.ThreadStatus.Locked || chunk.State != ChunkState.DisplayInSyncWithMeshes)) 
                return false;

            var md5Hash = chunk.GetMd5Hash();
            
            chunk.IsServerRequested = true;
            chunk.IsServerResyncMode = true;

            _server.ServerConnection.Send(
                new GetChunksMessage
                {
                    Range = new Range3I(chunkPosition, Vector3I.One),
                    Md5Hashes = new[] { md5Hash },
                    Positions = new[] { chunkPosition },
                    HashesCount = 1,
                    Flag = GetChunksMessageFlag.DontSendChunkDataIfNotModified
                });

            return true;
        }

        public void RebuildChunk(Vector3I chunkPosition)
        {
            var chunk = GetChunkFromChunkCoord(chunkPosition.X, chunkPosition.Z);
            chunk.State = ChunkState.MeshesChanged;
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
                    S33M3DXEngine.Threading.ThreadsManager.IsBoostMode = false;
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
            int ZWrap = VisualWorldParameters.WorldChunkStartUpPosition.Z;

            while (MathHelper.Mod(XWrap, VisualWorldParameters.WorldVisibleSize.X) != 0) XWrap++;
            while (MathHelper.Mod(ZWrap, VisualWorldParameters.WorldVisibleSize.Z) != 0) ZWrap++;

            VisualWorldParameters.WrapEnd = new Vector2I(XWrap, ZWrap);
        }


        #endregion

        public bool ShowDebugInfo { get; set; }

        public string GetDebugInfo()
        {
            if (ShowDebugInfo)
            {

                var c = GetChunk((int)PlayerManager.CameraWorldPosition.X, (int)PlayerManager.CameraWorldPosition.Z);
                //From World Coord to Cube Array Coord
                int arrayX = MathHelper.Mod((int)PlayerManager.CameraWorldPosition.X, AbstractChunk.ChunkSize.X);
                int arrayZ = MathHelper.Mod((int)PlayerManager.CameraWorldPosition.Z, AbstractChunk.ChunkSize.Z);
                var columnInfo = c.BlockData.GetColumnInfo(new Vector2I(arrayX, arrayZ));

                int BprimitiveCount = 0;
                int VprimitiveCount = 0;
                VisualChunk chunk;
                //Run over all chunks to see their status, and take action accordingly.
                for (var chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
                {
                    chunk = SortedChunks[chunkIndice];
                    if (!chunk.Graphics.IsFrustumCulled)
                    {
                        if (chunk.Graphics.SolidCubeIB != null) VprimitiveCount += chunk.Graphics.SolidCubeIB.IndicesCount;
                        if (chunk.Graphics.LiquidCubeIB != null) VprimitiveCount += chunk.Graphics.LiquidCubeIB.IndicesCount;
                    }
                    if (chunk.Graphics.SolidCubeIB != null) BprimitiveCount += chunk.Graphics.SolidCubeIB.IndicesCount;
                    if (chunk.Graphics.LiquidCubeIB != null) BprimitiveCount += chunk.Graphics.LiquidCubeIB.IndicesCount;
                }

                var line0 = string.Format("Nbr chunks : {0:000}, Nbr Visible chunks : {1:000}, {2:0000000} Buffered indices, {3:0000000} Visible indices", SortedChunks.Length, _chunkDrawByFrame, BprimitiveCount, VprimitiveCount);
                var line1 = string.Format("Static entity draw calls {2}: {0}, time {1}", _staticEntityDrawCalls, _staticEntityDrawTime, DrawStaticInstanced ? "[INSTANCED]" : "");
                var line2 = string.Format("Biomes MetaData : Temperature {0:0.00}, Moisture {1:0.00}, ColumnMaxHeight : {2}, ChunkID : {3}", columnInfo.Temperature / 255.0f, columnInfo.Moisture / 255.0f, columnInfo.MaxHeight, c.Position);
                var line3 = string.Format("Zone id : {0}", columnInfo.Zone);
                string line4 = string.Empty;
                if (_utopiaProcessorParam != null)
                {
                    line4 = string.Format("Biomes MetaData : Chunk Biome Type {0}, Column Biome Type {1}", _utopiaProcessorParam.Biomes[c.BlockData.ChunkMetaData.ChunkMasterBiomeType].Name, _utopiaProcessorParam.Biomes[columnInfo.Biome].Name);
                }

                return string.Join("\r\n", line0, line1, line2, line3, line4);
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
