using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.World;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Structs;
using Utopia.Planets.Terran.Chunk;
using S33M3Engines.Shared.Math;
using S33M3Engines.D3D;
using S33M3Engines.Maths;
using Utopia.GameClock;
using Utopia.Worlds.GameClocks;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.GameStates;
using Utopia.Entities.Living;
using Utopia.Worlds.Chunks.ChunkLandscape;
using Utopia.Worlds.Chunks.ChunkMesh;

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
        private WorldParameters _worldParameters; //The current world parameters
        private int _chunkPOWsize;
        private bool _chunkNeed2BeSorted;
        private D3DEngine _d3dEngine;
        private CameraManager _camManager;
        private Location2<int> _worldStartUpPosition;
        private GameStatesManager _gameStates;
        private ILivingEntity _player;
        private SingleArrayChunkContainer _cubesHolder;
        #endregion

        #region Public Property/Variables
        /// <summary> The chunk collection </summary>
        public VisualChunk[] Chunks { get; set; }
        public VisualChunk[] SortedChunks { get; set; }
        
        /// <summary> World parameters </summary>
        public WorldParameters WorldParameters
        {
            get { return _worldParameters; }
            set
            {
                _worldParameters = value;
                InitWorldParam(ref value);
            }
        }

        /// <summary> Visible World Size in Cubes unit </summary>
        public Location3<int> VisibleWorldSize { get; private set; }

        /// <summary> the visible world border in world coordinate </summary>
        public Range<int> WorldRange { get; set; }

        /// <summary> Variable to track the world wrapping End</summary>
        public Location2<int> WrapEnd { get; set; }

        public ILandscapeManager LandscapeManager { get; private set; }

        public IChunkMeshManager ChunkMeshManager { get; private set; }
        #endregion

        public WorldChunks(D3DEngine d3dEngine, 
                           CameraManager camManager, 
                           WorldParameters worldParameters, 
                           GameStatesManager gameStates, 
                           Location2<int> worldStartUpPosition, 
                           IClock gameClock, 
                           ILivingEntity player,
                           SingleArrayChunkContainer cubesHolder,
                           ILandscapeManager landscapeManager,
                           IChunkMeshManager chunkMeshManager)
        {
            _d3dEngine = d3dEngine;
            _gameStates = gameStates;
            _camManager = camManager;
            _worldStartUpPosition = worldStartUpPosition;
            _player = player;
            WorldParameters = worldParameters;
            _cubesHolder = cubesHolder;
            LandscapeManager = landscapeManager;
            ChunkMeshManager = chunkMeshManager;

            //Self injecting inside components
            landscapeManager.WorldChunks = this;
            chunkMeshManager.WorldChunks = this; 

            //Subscribe to chunk modifications
            _cubesHolder.BlockDataChanged += new EventHandler<ChunkDataProviderDataChangedEventArgs>(ChunkCubes_BlockDataChanged);

            Initialize();
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
            int arrayX = MathHelper.Mod(X, VisibleWorldSize.X);
            int arrayZ = MathHelper.Mod(Z, VisibleWorldSize.Z);

            //From Cube Array coord to Chunk Array coord
            int chunkX = arrayX >> _chunkPOWsize;
            int chunkZ = arrayZ >> _chunkPOWsize;

            return Chunks[chunkX + chunkZ * WorldParameters.WorldSize.X];
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
            if (X < WorldRange.Min.X || X > WorldRange.Max.X || Z < WorldRange.Min.Z || Z > WorldRange.Max.Z)
            {
                chunk = null;
                return false;
            }

            int arrayX = MathHelper.Mod(X, VisibleWorldSize.X);
            int arrayZ = MathHelper.Mod(Z, VisibleWorldSize.Z);

            int chunkX = arrayX >> _chunkPOWsize;
            int chunkZ = arrayZ >> _chunkPOWsize;

            chunk = Chunks[chunkX + chunkZ * WorldParameters.WorldSize.X];
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
            for (int chunkInd = 0; chunkInd < WorldParameters.WorldSize.Z; chunkInd++)
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
            for (int chunkInd = 0; chunkInd < WorldParameters.WorldSize.X; chunkInd++)
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
            if (chunkPosition.X == WorldRange.Min.X ||
               chunkPosition.Y == WorldRange.Min.Z ||
               chunkPosition.X == WorldRange.Max.X - AbstractChunk.ChunkSize.X ||
               chunkPosition.Y == WorldRange.Max.Z - AbstractChunk.ChunkSize.Z)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Private methods

        private void InitWorldParam(ref WorldParameters param)
        {
            VisibleWorldSize = new Location3<int>()
            {
                X = AbstractChunk.ChunkSize.X * param.WorldSize.X,
                Y = AbstractChunk.ChunkSize.Y,
                Z = AbstractChunk.ChunkSize.Z * param.WorldSize.Z,
            };

            _chunkPOWsize = (int)Math.Log(AbstractChunk.ChunkSize.X, 2);
        }

        /// <summary>
        /// Initiliaze the chunks array
        /// </summary>
        private void InitChunks()
        {
            //Defining the World Offset, to be used to reference the 2d circular array of dim defined in chunk
            WorldRange = new Range<int>()
            {
                Min = new Location3<int>(_worldStartUpPosition.X, 0, _worldStartUpPosition.Z),
                Max = new Location3<int>(_worldStartUpPosition.X + VisibleWorldSize.X, VisibleWorldSize.Y, _worldStartUpPosition.Z + VisibleWorldSize.Z)
            };

            //Create the chunks that will be used as "Rendering" array
            Chunks = new VisualChunk[_worldParameters.WorldSize.X * _worldParameters.WorldSize.Z];
            SortedChunks = new VisualChunk[_worldParameters.WorldSize.X * _worldParameters.WorldSize.Z];

            Range<int> cubeRange; //Used to define the blocks inside the chunks
            int arrayX, arrayZ;   //Chunk Array indexes
            VisualChunk chunk;

            for (int chunkX = 0; chunkX < _worldParameters.WorldSize.X; chunkX++)
            {
                for (int chunkZ = 0; chunkZ < _worldParameters.WorldSize.Z; chunkZ++)
                {
                    cubeRange = new Range<int>()
                    {
                        Min = new Location3<int>(_worldStartUpPosition.X + (chunkX * AbstractChunk.ChunkSize.X), 0, _worldStartUpPosition.Z + (chunkZ * AbstractChunk.ChunkSize.Z)),
                        Max = new Location3<int>(_worldStartUpPosition.X + ((chunkX + 1) * AbstractChunk.ChunkSize.X), AbstractChunk.ChunkSize.Y, _worldStartUpPosition.Z + ((chunkZ + 1) * AbstractChunk.ChunkSize.Z))
                    };

                    arrayX = MathHelper.Mod(cubeRange.Min.X, VisibleWorldSize.X);
                    arrayZ = MathHelper.Mod(cubeRange.Min.Z, VisibleWorldSize.Z);

                    //Create the new VisualChunk
                    chunk = new VisualChunk(this, ref cubeRange, _cubes);

                    //Store this chunk inside the arrays.
                    Chunks[(arrayX >> _chunkPOWsize) + (arrayZ >> _chunkPOWsize) * _worldParameters.WorldSize.X] = chunk;
                    SortedChunks[(arrayX >> _chunkPOWsize) + (arrayZ >> _chunkPOWsize) * _worldParameters.WorldSize.X] = chunk;
                }
            }

            _chunkNeed2BeSorted = true; // Will force the SortedChunks array to be sorted against the "camera position" (The player).
        }

        /// <summary>
        /// Initiliaze the WrapEnd variable. Is not needed if the starting world point is (0,X,0).
        /// </summary>
        private void InitWrappingVariables()
        {
            //Find the next number where mod == 0 !
            int XWrap = _worldStartUpPosition.X;
            int ZWrap = _worldStartUpPosition.Z;

            while (MathHelper.Mod(XWrap, VisibleWorldSize.X) != 0) XWrap++;
            while (MathHelper.Mod(ZWrap, VisibleWorldSize.Z) != 0) ZWrap++;

            WrapEnd = new Location2<int>(XWrap, ZWrap);
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
