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

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Will contains world block landscape stored as Chunks.
    /// </summary>
    public partial class WorldChunks : IWorldChunks
    {
        #region Private variables
        private WorldParameters _worldParameters; //The current world parameters
        private int _chunkPOWsize;
        private bool _chunkNeed2BeSorted;
        private Game _game;
        private Location2<int> _worldStartUpPosition;
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
        public Range<int> WorldBorder { get; set; }

        /// <summary> Variable to track the world wrapping End</summary>
        public Location2<int> WrapEnd { get; set; }
        #endregion

        public WorldChunks(Game game, WorldParameters worldParameters, Location2<int> worldStartUpPosition, IClock gameClock)
        {
            _game = game;
            _worldStartUpPosition = worldStartUpPosition;
            WorldParameters = worldParameters;

            //Subscribe to chunk modifications
            SingleArrayDataProvider.ChunkCubes.BlockDataChanged += new EventHandler<ChunkDataProviderDataChangedEventArgs>(ChunkCubes_BlockDataChanged);

            Initialize();
        }

        #region Public methods

        public void Initialize()
        {
            InitChunks();
            InitWrappingVariables();
            InitDrawComponents();
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
            if (X < WorldBorder.Min.X || X > WorldBorder.Max.X || Z < WorldBorder.Min.Z || Z > WorldBorder.Max.Z)
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
                Z += WorldParameters.ChunkSize.Z;
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
                X += WorldParameters.ChunkSize.X;
            }
        }

        #endregion

        #region Private methods

        private void InitWorldParam(ref WorldParameters param)
        {
            VisibleWorldSize = new Location3<int>()
            {
                X = param.ChunkSize.X * param.WorldSize.X,
                Y = param.ChunkSize.Y,
                Z = param.ChunkSize.Z * param.WorldSize.Z,
            };

            _chunkPOWsize = (int)Math.Log(param.ChunkSize.X, 2);
        }

        /// <summary>
        /// Initiliaze the chunks array
        /// </summary>
        private void InitChunks()
        {
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
                        Min = new Location3<int>(_worldStartUpPosition.X + (chunkX * _worldParameters.ChunkSize.X), 0, _worldStartUpPosition.Z + (chunkZ * _worldParameters.ChunkSize.Z)),
                        Max = new Location3<int>(_worldStartUpPosition.X + ((chunkX + 1) * _worldParameters.ChunkSize.X), _worldParameters.ChunkSize.Y, _worldStartUpPosition.Z + ((chunkZ + 1) * _worldParameters.ChunkSize.Z))
                    };

                    arrayX = MathHelper.Mod(cubeRange.Min.X, VisibleWorldSize.X);
                    arrayZ = MathHelper.Mod(cubeRange.Min.Z, VisibleWorldSize.Z);

                    //Create the new VisualChunk
                    chunk = new VisualChunk(this, ref cubeRange);

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

        /// <summary>
        /// Sort the chunks array if needed
        /// </summary>
        private void SortChunks()
        {
            if (!_chunkNeed2BeSorted || _game.ActivCamera == null) return;
            int index = 0;

            foreach (var chunk in Chunks.OrderBy(x => MVector3.Distance(x.CubeRange.Min, _game.ActivCamera.WorldPosition)))
            {
                SortedChunks[index] = chunk;
                index++;
            }

            _chunkNeed2BeSorted = false;
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
