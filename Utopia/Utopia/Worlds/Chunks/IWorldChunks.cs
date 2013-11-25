using System;
using System.Collections.Generic;
using SharpDX.Direct3D11;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks.ChunkLandscape;
using S33M3Resources.Structs;
using S33M3DXEngine.Main.Interfaces;
using S33M3DXEngine.Debug.Interfaces;

namespace Utopia.Worlds.Chunks
{
    public interface IWorldChunks<out T> : IDrawableComponent, IDebugInfo where T : VisualChunkBase
    {
        /// <summary>
        /// Indicates if inital chunks were loaded
        /// </summary>
        bool IsInitialized { get; set; }

        /// <summary>
        /// The visible world border in world coordinate
        /// </summary>
        VisualWorldParameters VisualWorldParameters { get; set; }
        
        /// <summary>
        /// Get a world's chunk from a chunk position
        /// </summary>
        /// <param name="chunkPosition"></param>
        /// <returns></returns>
        T GetChunk(Vector3I chunkPosition);

        /// <summary>
        /// Enumerates all visible chunks by player (i.e. not frustum culled)
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> VisibleChunks();
    }

    /// <summary>
    /// Represents 2d chunk layout manager
    /// </summary>
    public interface IWorldChunks2D : IDrawableComponent, IDebugInfo
    {
        /// <summary> The chunk collection </summary>
        VisualChunk[] Chunks { get; set; }

        VisualChunk[] SortedChunks { get; set; }

        ShaderResourceView Terra_View { get; }
        
        /// <summary> the visible world border in world coordinate </summary>
        VisualWorldParameters VisualWorldParameters { get; set; }

        /// <summary> indicate wether the chunks needs to be sorted</summary>
        bool ChunkNeed2BeSorted { get; set; }

        bool IsInitialLoadCompleted { get; set; }

        int StaticEntityViewRange { get; set; }

        /// <summary>
        /// Gets or sets value indicating if static entities should be drawn using instancing
        /// </summary>
        bool DrawStaticInstanced { get; set; }

        ILandscapeManager2D LandscapeManager { get; }

        /// <summary>
        /// Gets current leveling slice value
        /// </summary>
        int SliceValue { get; }

        /// <summary>
        /// Occurs when array of visual chunks get initialized
        /// </summary>
        event EventHandler ChunksArrayInitialized;

        /// <summary>
        /// Occurs when all chunks is loaded
        /// </summary>
        event EventHandler LoadComplete;

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <returns></returns>
        VisualChunk GetChunk(int X, int Z);

        /// <summary>
        /// Get a world's chunk from a chunk position
        /// </summary>
        /// <param name="chunkPos">chunk space coordinate</param>
        VisualChunk GetChunkFromChunkCoord(Vector3I chunkPos);

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <returns></returns>
        VisualChunk GetChunk(ref Vector3I cubePosition);

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate
        /// </summary>
        /// <param name="X">Chunk X coordinate</param>
        /// <param name="Z">Chunk Z coordinate</param>
        /// <returns></returns>
        VisualChunk GetChunkFromChunkCoord(int X, int Z);

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate with out of array check
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <param name="chunk">The VisualChunk return</param>
        /// <returns>True if the chunk was found</returns>
        bool GetSafeChunk(float X, float Z, out VisualChunk chunk);

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate with out of array check
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <param name="chunk">The VisualChunk return</param>
        /// <returns>True if the chunk was found</returns>
        bool GetSafeChunk(int X, int Z, out VisualChunk chunk);

        /// <summary>
        /// Get the list of chunks for a specific X world coordinate
        /// </summary>
        /// <param name="FixedX">The Fixed "line" of chunk to retrieve</param>
        /// <param name="WorldMinZ">Get All chunk From the WorldMinZ value to MaxLineZ-WorldMinZ (Excluded)</param>
        /// <returns></returns>
        IEnumerable<VisualChunk> GetChunksWithFixedX(int FixedX, int WorldMinZ);

        /// <summary>
        /// Get the list of chunks for a specific Z world coordinate
        /// </summary>
        /// <param name="FixedX">The Fixed "line" of chunk to retrieve</param>
        /// <param name="WorldMinZ">Get All chunk From the WorldMinX value to MaxLineX-WorldMinX (Excluded)</param>
        /// <returns></returns>
        IEnumerable<VisualChunk> GetChunksWithFixedZ(int FixedZ, int WorldMinX);

        /// <summary>
        /// Enumerates all visible chunks by player (i.e. not frustum culled)
        /// </summary>
        /// <returns></returns>
        IEnumerable<VisualChunk> VisibleChunks();
        
        void InitDrawComponents(DeviceContext context);

        bool ValidatePosition(ref Vector3D newPosition2Evaluate);

        bool IsEntityVisible(Vector3D pos);

        /// <summary>
        /// Enumerates chunks to draw
        /// </summary>
        /// <param name="sameSlice">in case of slice mode set this parameter to process only chunks with the same slice mesh, set this to false to process all visible chunks</param>
        /// <returns></returns>
        IEnumerable<VisualChunk> ChunksToDraw(bool sameSlice = true);
    }
}
