using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks.ChunkLandscape;
using S33M3Resources.Structs;
using S33M3DXEngine.Main.Interfaces;
using S33M3DXEngine.Debug.Interfaces;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Base interface for 2d and 3d chunk layouts managers
    /// </summary>
    public interface IWorldChunks : IDrawableComponent, IDebugInfo
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
        VisualChunkBase GetBaseChunk(Vector3I chunkPosition);

        bool ResyncChunk(Vector3I chunkPosition, bool forced);
    }

    /// <summary>
    /// Represents 2d chunk layout manager
    /// </summary>
    public interface IWorldChunks2D : IWorldChunks
    {
        /// <summary> The chunk collection </summary>
        VisualChunk2D[] Chunks { get; set; }

        //Chunks sorted from the near to far compared to the players
        VisualChunk2D[] SortedChunks { get; set; }

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

        void ResyncClientChunks();
        
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
        VisualChunk2D GetChunk(int X, int Z);

        /// <summary>
        /// Get a world's chunk from a chunk position
        /// </summary>
        /// <param name="chunkPos">chunk space coordinate</param>
        VisualChunk2D GetChunkFromChunkCoord(Vector3I chunkPos);

        /// <summary>
        /// Get a world's chunk from a chunk position with bound array check
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Z"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        bool GetSafeChunkFromChunkCoord(int X, int Z, out VisualChunk2D chunk);

        /// <summary>
        /// Get a world's chunk from a chunk position with bound array check
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Z"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        bool GetSafeChunkFromChunkCoord(Vector3I chunkPos, out VisualChunk2D chunk);

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <returns></returns>
        VisualChunk2D GetChunk(Vector3I cubePosition);

        VisualChunk2D GetPlayerChunk();

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate
        /// </summary>
        /// <param name="X">Chunk X coordinate</param>
        /// <param name="Z">Chunk Z coordinate</param>
        /// <returns></returns>
        VisualChunk2D GetChunkFromChunkCoord(int X, int Z);

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate with out of array check
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <param name="chunk">The VisualChunk2D return</param>
        /// <returns>True if the chunk was found</returns>
        bool GetSafeChunk(float X, float Z, out VisualChunk2D chunk);

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate with out of array check
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <param name="chunk">The VisualChunk2D return</param>
        /// <returns>True if the chunk was found</returns>
        bool GetSafeChunk(int X, int Z, out VisualChunk2D chunk);

        /// <summary>
        /// Get the list of chunks for a specific X world coordinate
        /// </summary>
        /// <param name="FixedX">The Fixed "line" of chunk to retrieve</param>
        /// <param name="WorldMinZ">Get All chunk From the WorldMinZ value to MaxLineZ-WorldMinZ (Excluded)</param>
        /// <returns></returns>
        IEnumerable<VisualChunk2D> GetChunksWithFixedX(int FixedX, int WorldMinZ);

        /// <summary>
        /// Get the list of chunks for a specific Z world coordinate
        /// </summary>
        /// <param name="FixedX">The Fixed "line" of chunk to retrieve</param>
        /// <param name="WorldMinZ">Get All chunk From the WorldMinX value to MaxLineX-WorldMinX (Excluded)</param>
        /// <returns></returns>
        IEnumerable<VisualChunk2D> GetChunksWithFixedZ(int FixedZ, int WorldMinX);

        /// <summary>
        /// Enumerates all visible chunks by player (i.e. not frustum culled)
        /// </summary>
        /// <returns></returns>
        IEnumerable<VisualChunk2D> VisibleChunks();
        
        void InitDrawComponents(DeviceContext context);

        bool ValidatePosition(ref Vector3D newPosition2Evaluate);

        bool IsEntityVisible(Vector3D pos);

        IEnumerable<VisualChunk2D> GetChunks(WorldChunks.GetChunksFilter filter);

        /// <summary>
        /// Enumerates chunks to draw
        /// </summary>
        /// <param name="sameSlice">in case of slice mode set this parameter to process only chunks with the same slice mesh, set this to false to process all visible chunks</param>
        /// <returns></returns>
        IEnumerable<VisualChunk2D> ChunksToDraw(bool sameSlice = true);

        void RebuildChunk(Vector3I position);
        void DrawStaticEntitiesShadow(DeviceContext context, VisualChunk2D chunk);
        //void PrepareVoxelDraw(DeviceContext context, Matrix viewProjection);
        //void DrawStaticEntities(DeviceContext context, VisualChunk2D chunk);
    }
}
