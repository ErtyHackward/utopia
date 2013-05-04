﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using Utopia.Worlds.Chunks;
using Utopia.Shared.World;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks.ChunkLandscape;
using SharpDX;
using S33M3Resources.Structs;
using S33M3DXEngine.Main.Interfaces;
using S33M3DXEngine.Debug.Interfaces;
using S33M3CoreComponents.Physics.Verlet;

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

        bool IsInitialLoadCompleted { get; set; }

        int StaticEntityViewRange { get; set; }

        /// <summary>
        /// Gets or sets value indicating if static entities should be drawn using instancing
        /// </summary>
        bool DrawStaticInstanced { get; set; }

        ILandscapeManager LandscapeManager { get; }

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
        VisualChunk GetChunkFromChunkCoord(Vector2I chunkPos);

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

        void isCollidingWithTerrain(VerletSimulator _physicSimu, ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition);

        byte isCollidingWithTerrain(ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate);
        
        void InitDrawComponents(DeviceContext context);

        bool ValidatePosition(ref Vector3D newPosition2Evaluate);


    }
}
