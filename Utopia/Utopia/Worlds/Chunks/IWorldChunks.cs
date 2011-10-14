using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Worlds.Chunks;
using Utopia.Shared.World;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks.ChunkLandscape;
using S33M3Engines.D3D.DebugTools;
using S33M3Physics.Verlet;
using SharpDX;
using S33M3Engines.Shared.Math;

namespace Utopia.Worlds.Chunks
{
    public interface IWorldChunks : IDrawableComponent, IGameComponent, IDebugInfo
    {
        /// <summary> The chunk collection </summary>
        VisualChunk[] Chunks { get; set; }
        VisualChunk[] SortedChunks { get; set; }

        /// <summary> the visible world border in world coordinate </summary>
        VisualWorldParameters VisualWorldParameters { get; set; }

        /// <summary> indicate wether the chunks needs to be sorted</summary>
        bool ChunkNeed2BeSorted { get; set; }
        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <returns></returns>
        VisualChunk GetChunk(int X, int Z);

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

        ILandscapeManager LandscapeManager { get; }

        void isCollidingWithTerrain(VerletSimulator _physicSimu, ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition);
    }
}
