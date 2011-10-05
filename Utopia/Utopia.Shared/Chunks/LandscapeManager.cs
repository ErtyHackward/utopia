using System;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Base class for chunk landscape management with 2d layout
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LandscapeManager<T> : ILandscapeManager2D where T : AbstractChunk, IChunkLayout2D
    {
        /// <summary>
        /// Gets chunk from chunk global position
        /// </summary>
        /// <param name="globalPosition"></param>
        /// <returns></returns>
        public T GetChunk(Vector3D globalPosition)
        {
            return
                GetChunk(new Vector2I((int) Math.Floor(globalPosition.X/AbstractChunk.ChunkSize.X),
                                      (int) Math.Floor(globalPosition.Z/AbstractChunk.ChunkSize.Z)));
        }

        /// <summary>
        /// Gets the chunk at position specified
        /// </summary>
        /// <param name="position">chunk position</param>
        /// <returns></returns>
        public abstract T GetChunk(Vector2I position);

        IChunkLayout2D ILandscapeManager2D.GetChunk(Vector2I position)
        {
            return GetChunk(position);
        }

        /// <summary>
        /// Gets landscape cursor
        /// </summary>
        /// <param name="blockPosition">global block position</param>
        /// <returns></returns>
        public LandscapeCursor GetCursor(Vector3I blockPosition)
        {
            return new LandscapeCursor(this, blockPosition);
        }

        public LandscapeCursor GetCursor(Vector3D entityPosition)
        {
            return GetCursor(new Vector3I((int)Math.Floor(entityPosition.X), (int)entityPosition.Y, (int)Math.Floor(entityPosition.Z)));
        }

        /// <summary>
        /// Returns block position based on entity position
        /// </summary>
        /// <param name="entityPosition"></param>
        /// <returns></returns>
        public static Vector3I EntityToBlockPosition(Vector3D entityPosition)
        {
            return new Vector3I((int)Math.Floor(entityPosition.X), (int)entityPosition.Y, (int)Math.Floor(entityPosition.Z));
        }
    }
}
