using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Worlds.Chunks;
using Utopia.Shared.World;
using Utopia.Shared.Structs;

namespace Utopia.Worlds.Chunks
{
    public interface IWorldChunks : IDrawableComponent
    {
        /// <summary> The chunk collection </summary>
        VisualChunk[] Chunks { get; set; }
        VisualChunk[] SortedChunks { get; set; }

        /// <summary> World parameters </summary>
        WorldParameters WorldParameters { get; set; }

        /// <summary> Visible World Size in Cubes unit </summary>
        Location3<int> VisibleWorldSize { get; }

        /// <summary> the visible world border in world coordinate </summary>
        Range<int> WorldBorder { get; set; }

        /// <summary> Variable to track the world wrapping End</summary>
        Location2<int> WrapEnd { get; set; }

        /// <summary>
        /// Get a world's chunk from a Cube location in world coordinate
        /// </summary>
        /// <param name="X">Cube X coordinate in world coordinate</param>
        /// <param name="Z">Cube Z coordinate in world coordinate</param>
        /// <returns></returns>
        VisualChunk GetChunk(int X, int Z);

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

    }
}
