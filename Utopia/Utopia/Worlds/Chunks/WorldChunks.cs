using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.World;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Structs;
using S33M3Engines.Shared.Math;
using S33M3Engines.D3D;
using S33M3Engines.Maths;
using Utopia.Worlds.GameClocks;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.GameStates;
using Utopia.Entities.Living;
using Utopia.Worlds.Chunks.ChunkLandscape;
using Utopia.Worlds.Chunks.ChunkMesh;
using Utopia.Worlds.Cubes;
using Ninject;
using S33M3Engines.WorldFocus;
using Utopia.Worlds.Chunks.ChunkWrapper;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Network;

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
    public partial class WorldChunks : IWorldChunks
    {
        #region Private variables
        private D3DEngine _d3dEngine;
        private CameraManager _camManager;
        private GameStatesManager _gameStates;
        private ILivingEntity _player;
        private SingleArrayChunkContainer _cubesHolder;
        private IClock _gameClock;
        private WorldFocusManager _worldFocusManager;
        private IChunksWrapper _chunkWrapper;
        private ILightingManager _lightingManager;
        private ILandscapeManager _landscapeManager;
        private IChunkMeshManager _chunkMeshManager;
        private Server _server;
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

        public WorldChunks(D3DEngine d3dEngine, 
                           CameraManager camManager,
                           VisualWorldParameters visualWorldParameters,
                           WorldFocusManager worldFocusManager,
                           GameStatesManager gameStates, 
                           IClock gameClock, 
                           ILivingEntity player,
                           SingleArrayChunkContainer cubesHolder,
                           ILandscapeManager landscapeManager,
                           IChunkMeshManager chunkMeshManager,
                           IChunksWrapper chunkWrapper,
                           ILightingManager lightingManager,
                           Server server)
        {
            _server = server;
            _d3dEngine = d3dEngine;
            _worldFocusManager = worldFocusManager;
            _gameStates = gameStates;
            _camManager = camManager;
            _gameClock = gameClock;
            _player = player;
            VisualWorldParameters = visualWorldParameters;
            _cubesHolder = cubesHolder;
            _chunkWrapper = chunkWrapper;
            _landscapeManager = landscapeManager;
            _chunkMeshManager = chunkMeshManager;
            _lightingManager = lightingManager;

            //Self injecting inside components
            _chunkWrapper.WorldChunks = this;

            //Subscribe to chunk modifications
            _cubesHolder.BlockDataChanged += new EventHandler<ChunkDataProviderDataChangedEventArgs>(ChunkCubes_BlockDataChanged);

            //Initialize();
        }

        #region Public methods

        public void Initialize()
        {
            InitChunks();
            InitWrappingVariables();
            InitDrawComponents();
            IntilializeUpdateble();
        }

        public void Dispose()
        {
            foreach (VisualChunk chunk in Chunks)
            {
                if (chunk != null) chunk.Dispose();
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

            int arrayX = MathHelper.Mod(X, VisualWorldParameters.WorldVisibleSize.X);
            int arrayZ = MathHelper.Mod(Z, VisualWorldParameters.WorldVisibleSize.Z);

            int chunkX = arrayX >> VisualWorldParameters.ChunkPOWsize;
            int chunkZ = arrayZ >> VisualWorldParameters.ChunkPOWsize;

            chunk = Chunks[chunkX + chunkZ * VisualWorldParameters.WorldParameters.WorldChunkSize.X];
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
            for (int chunkInd = 0; chunkInd < VisualWorldParameters.WorldParameters.WorldChunkSize.Z; chunkInd++)
            {
                yield return GetChunk(FixedX, Z);
                Z += AbstractChunk.ChunkSize.Z;
            }
        }

        /// <summary>
        /// Get the list of chunks for a specific Z world coordinate
        /// </summary>
        /// <param name="FixedX">The Fixed "line" of chunk to retrieve</param>
        /// <param name="WorldMinZ">Get All chunk From the WorldMinX value to MaxLineX-WorldMinX (Excluded)</param>
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
        /// <param name="X"></param>
        /// <param name="Z"></param>
        /// <param name="worldRange"></param>
        /// <returns></returns>
        public bool isBorderChunk(IntVector2 chunkPosition)
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
                Min = new Location3<int>(VisualWorldParameters.WorldChunkStartUpPosition.X, 0, VisualWorldParameters.WorldChunkStartUpPosition.Z),
                Max = new Location3<int>(VisualWorldParameters.WorldChunkStartUpPosition.X + VisualWorldParameters.WorldVisibleSize.X, VisualWorldParameters.WorldVisibleSize.Y, VisualWorldParameters.WorldChunkStartUpPosition.Z + VisualWorldParameters.WorldVisibleSize.Z)
            };

            //Create the chunks that will be used as "Rendering" array
            Chunks = new VisualChunk[VisualWorldParameters.WorldParameters.WorldChunkSize.X * VisualWorldParameters.WorldParameters.WorldChunkSize.Z];
            SortedChunks = new VisualChunk[VisualWorldParameters.WorldParameters.WorldChunkSize.X * VisualWorldParameters.WorldParameters.WorldChunkSize.Z];

            Range<int> cubeRange; //Used to define the blocks inside the chunks
            int arrayX, arrayZ;   //Chunk Array indexes
            VisualChunk chunk;

            for (int chunkX = 0; chunkX < VisualWorldParameters.WorldParameters.WorldChunkSize.X; chunkX++)
            {
                for (int chunkZ = 0; chunkZ < VisualWorldParameters.WorldParameters.WorldChunkSize.Z; chunkZ++)
                {
                    cubeRange = new Range<int>()
                    {
                        Min = new Location3<int>(VisualWorldParameters.WorldChunkStartUpPosition.X + (chunkX * AbstractChunk.ChunkSize.X), 0, VisualWorldParameters.WorldChunkStartUpPosition.Z + (chunkZ * AbstractChunk.ChunkSize.Z)),
                        Max = new Location3<int>(VisualWorldParameters.WorldChunkStartUpPosition.X + ((chunkX + 1) * AbstractChunk.ChunkSize.X), AbstractChunk.ChunkSize.Y, VisualWorldParameters.WorldChunkStartUpPosition.Z + ((chunkZ + 1) * AbstractChunk.ChunkSize.Z))
                    };

                    arrayX = MathHelper.Mod(cubeRange.Min.X, VisualWorldParameters.WorldVisibleSize.X);
                    arrayZ = MathHelper.Mod(cubeRange.Min.Z, VisualWorldParameters.WorldVisibleSize.Z);

                    //Create the new VisualChunk
                    chunk = new VisualChunk(_d3dEngine, _worldFocusManager, VisualWorldParameters, ref cubeRange, _cubesHolder);
                    if (_server.Connected) chunk.IsServerRequested = true;

                    //Store this chunk inside the arrays.
                    Chunks[(arrayX >> VisualWorldParameters.ChunkPOWsize) + (arrayZ >> VisualWorldParameters.ChunkPOWsize) * VisualWorldParameters.WorldParameters.WorldChunkSize.X] = chunk;
                    SortedChunks[(arrayX >> VisualWorldParameters.ChunkPOWsize) + (arrayZ >> VisualWorldParameters.ChunkPOWsize) * VisualWorldParameters.WorldParameters.WorldChunkSize.X] = chunk;
                }
            }

            //Request the Full landscape chunks if server connected
            if (_server.Connected)
            {
                _server.ServerConnection.SendAsync(new Net.Messages.GetChunksMessage
                                                       {
                                                           Range = new Range2(
                                                               new IntVector2(
                                                                   VisualWorldParameters.WorldChunkStartUpPosition.X/AbstractChunk.ChunkSize.X,
                                                                   VisualWorldParameters.WorldChunkStartUpPosition.Z/AbstractChunk.ChunkSize.Z
                                                                   ),
                                                               new IntVector2(
                                                                   VisualWorldParameters.WorldParameters.WorldChunkSize.X,
                                                                   VisualWorldParameters.WorldParameters.WorldChunkSize.Z
                                                                   )
                                                               ),
                                                           Flag = Net.Messages.GetChunksMessageFlag.AlwaysSendChunkData
                                                       });
            }

            ChunkNeed2BeSorted = true; // Will force the SortedChunks array to be sorted against the "camera position" (The player).
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

            VisualWorldParameters.WrapEnd = new Location2<int>(XWrap, ZWrap);
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
