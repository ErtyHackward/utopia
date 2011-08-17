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

namespace Utopia.Worlds
{
    //= Visible chunk container !
    public class World
    {
        #region Private variables
        private WorldParameters _worldParameters;
        private int _chunkPOWsize;
        #endregion

        #region Public Property/Variables
        //The chunk collection
        public VisualChunk[] Chunks { get; set; }
        
        //World parameters
        public WorldParameters WorldParameters
        {
            get { return _worldParameters; }
            set
            {
                _worldParameters = value;
                InitWorldParam(ref value);
            }
        }

        //Visible World Size in Cubes unit
        public Location3<int> VisibleWorldSize { get; private set; }

        //The visible world border in world coordinate
        public Range<int> WorldBorder { get; set; }
        #endregion

        public World(WorldParameters worldParameters)
        {
            WorldParameters = worldParameters;

            //Subscribe to chunk modifications
            SingleArrayDataProvider.ChunkCubes.BlockDataChanged += new EventHandler<ChunkDataProviderDataChangedEventArgs>(ChunkCubes_BlockDataChanged);
        }

        #region Private methods
        private void InitChunks()
        {

        }

        private void InitWorldParam(ref WorldParameters param)
        {
            VisibleWorldSize = new Location3<int>()
            {
                X = param.ChunkSize.X * param.WorldSize.X,
                Y = param.ChunkSize.Y,
                Z = param.ChunkSize.Z * param.WorldSize.Z,
            };

            _chunkPOWsize = MathHelper.Bitcount(param.ChunkSize.X);
        }

        #endregion

        #region Public methods
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
