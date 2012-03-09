﻿using System;
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
using S33M3_DXEngine.Main;
using S33M3_DXEngine;
using S33M3_CoreComponents.Cameras;
using S33M3_CoreComponents.Cameras.Interfaces;
using S33M3_CoreComponents.States;
using S33M3_CoreComponents.WorldFocus;
using S33M3_CoreComponents.Maths;
using S33M3_Resources.Structs;
using S33M3_CoreComponents.Physics.Verlet;

namespace Utopia.Worlds.Chunks
{
    public enum ChunkState : byte
    {
        Empty,
        LandscapeCreated,
        LandscapeLightsSourceCreated,
        LandscapeLightsPropagated,
        MeshesChanged,
        DisplayInSyncWithMeshes,
        UserChanged
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
        private readonly object _counterLock = new object();

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
            var handler = ChunksArrayInitialized;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when all chunks is loaded
        /// </summary>
        public event EventHandler LoadComplete;

        private void OnInitialLoadComplete()
        {
            var handler = LoadComplete;
            if (handler != null) handler(this, EventArgs.Empty);
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
                           SharedFrameCB sharedFrameCB)
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

            //Self injecting inside components, to avoid circular dependency
            _chunkWrapper.WorldChunks = this;
            pickingManager.WorldChunks = this;
            lightingManager.WorldChunk = this;
            _playerManager.WorldChunks = this;

            //Subscribe to chunk modifications
            _cubesHolder.BlockDataChanged += ChunkCubes_BlockDataChanged;
            IsInitialLoadCompleted = false;

            DrawOrders.UpdateIndex(SOLID_DRAW, 11);
            TRANSPARENT_DRAW = DrawOrders.AddIndex(20, "TRANSPARENT_DRAW");
            ENTITIES_DRAW = DrawOrders.AddIndex(1050, "ENTITIES_DRAW");
        }

        #region Public methods

        public override void Initialize()
        {
            InitChunks();
            InitWrappingVariables();
        }

        public override void LoadContent()
        {
            IntilializeUpdateble();
            InitDrawComponents();
        }

        public override void Dispose()
        {
            foreach (var chunk in Chunks)
            {
                if (chunk != null)
                {
                    chunk.ReadyToDraw -= ChunkReadyToDraw;
                    chunk.Dispose();
                }
            }

            DisposeDrawComponents();
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

            return Chunks[chunkX + chunkZ * VisualWorldParameters.WorldParameters.WorldChunkSize.X];
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
            if (X < VisualWorldParameters.WorldRange.Min.X || X > VisualWorldParameters.WorldRange.Max.X || Z < VisualWorldParameters.WorldRange.Min.Z || Z > VisualWorldParameters.WorldRange.Max.Z)
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
            for (int chunkInd = 0; chunkInd < VisualWorldParameters.WorldParameters.WorldChunkSize.Y; chunkInd++)
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
            for (int chunkInd = 0; chunkInd < VisualWorldParameters.WorldParameters.WorldChunkSize.X; chunkInd++)
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
            if (chunkPosition.X == VisualWorldParameters.WorldRange.Min.X ||
               chunkPosition.Y == VisualWorldParameters.WorldRange.Min.Z ||
               chunkPosition.X == VisualWorldParameters.WorldRange.Max.X - AbstractChunk.ChunkSize.X ||
               chunkPosition.Y == VisualWorldParameters.WorldRange.Max.Z - AbstractChunk.ChunkSize.Z)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Validate player move against surrending landscape, if move not possible, it will be "rollbacked"
        /// It's used by the physic engine
        /// </summary>
        /// <param name="newPosition2Evaluate"></param>
        /// <param name="previousPosition"></param>
        public void isCollidingWithTerrain(VerletSimulator _physicSimu, ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {
            BoundingBox _boundingBox2Evaluate;
            Vector3D newPositionWithColliding = previousPosition;
            TerraCubeWithPosition _collidingCube;

            if (newPosition2Evaluate == previousPosition) return;

            //Create a Bounding box with my new suggested position, taking only the X that has been changed !
            //X Testing =====================================================
            newPositionWithColliding.X = newPosition2Evaluate.X;
            _boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());

            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate, true, out _collidingCube))
            {
                if ((_playerManager.PlayerOnOffsettedBlock && _collidingCube.Position.Y == MathHelper.Fastfloor(newPosition2Evaluate.Y)))
                {
                    newPositionWithColliding.Y = newPosition2Evaluate.Y + 0.6;
                    _boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());
                    if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate, true, out _collidingCube))
                    {
                        newPositionWithColliding.X = previousPosition.X;
                    }
                }
                else
                {
                    newPositionWithColliding.X = previousPosition.X;
                }
            }

            //Z Testing =========================================================
            newPositionWithColliding.Z = newPosition2Evaluate.Z;
            _boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate, true, out _collidingCube))
            {
                if ((_playerManager.PlayerOnOffsettedBlock && _collidingCube.Position.Y == MathHelper.Fastfloor(newPosition2Evaluate.Y)))
                {
                    newPositionWithColliding.Y = newPosition2Evaluate.Y + 0.6;
                    _boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());
                    if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate, true, out _collidingCube))
                    {
                        newPositionWithColliding.Z = previousPosition.Z;
                    }
                }
                else
                {
                    newPositionWithColliding.Z = previousPosition.Z;
                }
            }

            //Y Testing ======================================================
            newPositionWithColliding.Y = newPosition2Evaluate.Y;

            //My Position raise  ==> If I were on the ground, I'm no more
            if (previousPosition.Y < newPositionWithColliding.Y && _physicSimu.OnGround) _physicSimu.OnGround = false;

            _boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());

            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate, false, out _collidingCube))
            {
                //If Jummping
                if (previousPosition.Y < newPositionWithColliding.Y)
                {
                    if(GameSystemSettings.Current.Settings.CubesProfile[_collidingCube.Cube.Id].YBlockOffset == 0)
                    newPositionWithColliding.Y = previousPosition.Y;
                }
                else //Falling
                {
                    //The "ground level" will be the next ground below entity, if entity BB is inside a block, the ground level is the first above entity.
                    newPositionWithColliding.Y = _playerManager.GroundBelowEntity;
                    // ==> This way I stop the Y move ! If not done, its like applying "quickly" a force to the UP ! <= While I jsut want to follow the Ground.
                    previousPosition.Y = _playerManager.GroundBelowEntity;
                    _physicSimu.OnGround = true; // On ground ==> Activite the force that will counter the gravity !!
                }
            }

            newPosition2Evaluate = newPositionWithColliding;
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Initiliaze the chunks array
        /// </summary>
        private void InitChunks()
        {
            //Defining the World Offset, to be used to reference the 2d circular array of dim defined in chunk
            VisualWorldParameters.WorldRange = new Range<int>()
            {
                Min = new Location3<int>(VisualWorldParameters.WorldChunkStartUpPosition.X, 0, VisualWorldParameters.WorldChunkStartUpPosition.Y),
                Max = new Location3<int>(VisualWorldParameters.WorldChunkStartUpPosition.X + VisualWorldParameters.WorldVisibleSize.X, VisualWorldParameters.WorldVisibleSize.Y, VisualWorldParameters.WorldChunkStartUpPosition.Y + VisualWorldParameters.WorldVisibleSize.Z)
            };

            //Create the chunks that will be used as "Rendering" array
            Chunks = new VisualChunk[VisualWorldParameters.WorldParameters.WorldChunkSize.X * VisualWorldParameters.WorldParameters.WorldChunkSize.Y];
            SortedChunks = new VisualChunk[VisualWorldParameters.WorldParameters.WorldChunkSize.X * VisualWorldParameters.WorldParameters.WorldChunkSize.Y];

            Range<int> cubeRange; //Used to define the blocks inside the chunks
            int arrayX, arrayZ;   //Chunk Array indexes
            VisualChunk chunk;

            //Chunk Server request variables
            List<Vector2I> chunkPosition = new List<Vector2I>();
            List<Md5Hash> chunkHash = new List<Md5Hash>();
            Md5Hash chunkMD5;

            for (int chunkX = 0; chunkX < VisualWorldParameters.WorldParameters.WorldChunkSize.X; chunkX++)
            {
                for (int chunkZ = 0; chunkZ < VisualWorldParameters.WorldParameters.WorldChunkSize.Y; chunkZ++)
                {
                    cubeRange = new Range<int>()
                    {
                        Min = new Location3<int>(VisualWorldParameters.WorldChunkStartUpPosition.X + (chunkX * AbstractChunk.ChunkSize.X), 0, VisualWorldParameters.WorldChunkStartUpPosition.Y + (chunkZ * AbstractChunk.ChunkSize.Z)),
                        Max = new Location3<int>(VisualWorldParameters.WorldChunkStartUpPosition.X + ((chunkX + 1) * AbstractChunk.ChunkSize.X), AbstractChunk.ChunkSize.Y, VisualWorldParameters.WorldChunkStartUpPosition.Y + ((chunkZ + 1) * AbstractChunk.ChunkSize.Z))
                    };

                    arrayX = MathHelper.Mod(cubeRange.Min.X, VisualWorldParameters.WorldVisibleSize.X);
                    arrayZ = MathHelper.Mod(cubeRange.Min.Z, VisualWorldParameters.WorldVisibleSize.Z);

                    //Create the new VisualChunk
                    chunk = new VisualChunk(_d3dEngine, _worldFocusManager, VisualWorldParameters, ref cubeRange, _cubesHolder, _pickingManager);
                    chunk.IsServerRequested = true;
                    //Ask the chunk Data to the DB, in case my local MD5 is equal to the server one.
                    chunk.StorageRequestTicket = _chunkstorage.RequestDataTicket_async(chunk.ChunkID);

                    chunk.ReadyToDraw += ChunkReadyToDraw;

                    //Store this chunk inside the arrays.
                    Chunks[(arrayX >> VisualWorldParameters.ChunkPOWsize) + (arrayZ >> VisualWorldParameters.ChunkPOWsize) * VisualWorldParameters.WorldParameters.WorldChunkSize.X] = chunk;
                    SortedChunks[(arrayX >> VisualWorldParameters.ChunkPOWsize) + (arrayZ >> VisualWorldParameters.ChunkPOWsize) * VisualWorldParameters.WorldParameters.WorldChunkSize.X] = chunk;

                    //Is this chunk inside the Client storae manager ?
                    if (_chunkstorage.ChunkHashes.TryGetValue(chunk.ChunkID, out chunkMD5))
                    {
                        chunkPosition.Add(new Vector2I((VisualWorldParameters.WorldChunkStartUpPosition.X + (chunkX * AbstractChunk.ChunkSize.X)) / AbstractChunk.ChunkSize.X,
                                                       (VisualWorldParameters.WorldChunkStartUpPosition.Y + (chunkZ * AbstractChunk.ChunkSize.Z)) / AbstractChunk.ChunkSize.Z));
                        chunkHash.Add(chunkMD5);
                    }
                }
            }

            _server.ServerConnection.SendAsync(
            new GetChunksMessage()
            {
                Range = new Range2(
                    new Vector2I(
                        VisualWorldParameters.WorldChunkStartUpPosition.X / AbstractChunk.ChunkSize.X,
                        VisualWorldParameters.WorldChunkStartUpPosition.Y / AbstractChunk.ChunkSize.Z
                        ),
                    new Vector2I(
                        VisualWorldParameters.WorldParameters.WorldChunkSize.X,
                        VisualWorldParameters.WorldParameters.WorldChunkSize.Y
                        )
                    ),
                Md5Hashes = chunkHash.ToArray(),
                Positions = chunkPosition.ToArray(),
                HashesCount = chunkHash.Count,
                Flag = GetChunksMessageFlag.DontSendChunkDataIfNotModified
            }
            );

            ChunkNeed2BeSorted = true; // Will force the SortedChunks array to be sorted against the "camera position" (The player).

            OnChunksArrayInitialized();
        }

        void ChunkReadyToDraw(object sender, EventArgs e)
        {
            var chunk = (VisualChunk)sender;
            chunk.Opaque = 0f;
            _transparentChunks.Add(chunk);

            lock (_counterLock)
            {
                _readyToDrawCount++;
                if (_readyToDrawCount > (Chunks.Length * 5 /100) && IsInitialLoadCompleted == false)
                {
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
    }
}
