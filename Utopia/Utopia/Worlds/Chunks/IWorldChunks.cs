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
    public interface IWorldChunks : IDrawableComponent, IDebugInfo
    {
        /// <summary> The chunk collection </summary>
        VisualChunk[] Chunks { get; set; }

        //Chunks sorted from the near to far compared to the players
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

        void ResyncClientChunks();

        bool ResyncChunk(Vector3I chunkPosition, bool forced);

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
        /// Get a world's chunk from a chunk position with bound array check
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Z"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        bool GetSafeChunkFromChunkCoord(int X, int Z, out VisualChunk chunk);

        /// <summary>
        /// Get a world's chunk from a chunk position with bound array check
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Z"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        bool GetSafeChunkFromChunkCoord(Vector3I chunkPos, out VisualChunk chunk);

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <returns></returns>
        VisualChunk GetChunk(Vector3I cubePosition);

        VisualChunk GetPlayerChunk();

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

        IEnumerable<VisualChunk> GetChunks(WorldChunks.GetChunksFilter filter);

        /// <summary>
        /// Enumerates chunks to draw
        /// </summary>
        /// <param name="sameSlice">in case of slice mode set this parameter to process only chunks with the same slice mesh, set this to false to process all visible chunks</param>
        /// <returns></returns>
        IEnumerable<VisualChunk> ChunksToDraw(bool sameSlice = true);

        void RebuildChunk(Vector3I position);
        void DrawStaticEntities(DeviceContext context, VisualChunk chunk);
        void PrepareVoxelDraw(DeviceContext context, Matrix viewProjection);
    }
}
