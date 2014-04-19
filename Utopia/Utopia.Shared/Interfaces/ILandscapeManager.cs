using System.Collections.Generic;
using S33M3CoreComponents.Physics.Verlet;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Interfaces
{
    /// <summary>
    /// Contains all chunks
    /// </summary>
    public interface ILandscapeManager
    {
        /// <summary>
        /// Gets the chunk at position specified
        /// </summary>
        /// <param name="chunkPosition">chunk position in World coordinate</param>
        /// <returns></returns>
        IAbstractChunk GetChunk(Vector3I chunkPosition);

        /// <summary>
        /// Gets the chunk at block position specified
        /// </summary>
        /// <param name="blockPosition">block position in World coordinate</param>
        /// <returns></returns>
        IAbstractChunk GetChunkFromBlock(Vector3I blockPosition);

        /// <summary>
        /// Returns block cursor
        /// </summary>
        /// <param name="blockPosition"></param>
        /// <returns></returns>
        ILandscapeCursor GetCursor(Vector3I blockPosition);

        /// <summary>
        /// Returns block cursor
        /// </summary>
        /// <param name="entityPosition"></param>
        /// <returns></returns>
        ILandscapeCursor GetCursor(Vector3D entityPosition);

        /// <summary>
        /// "Simple" collision detection check against landscape, send back the cube being collided
        /// </summary>
        /// <param name="localEntityBoundingBox"></param>
        /// <param name="newPosition2Evaluate"></param>
        byte IsCollidingWithTerrain(ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate);

        /// <summary>
        /// Validate player move against surrounding landscape, if move not possible, it will be "rollbacked"
        /// It's used by the physic engine
        /// </summary>
        /// <param name="physicSimu"></param>
        /// <param name="localEntityBoundingBox"></param>
        /// <param name="newPosition2Evaluate"></param>
        void IsCollidingWithTerrain(VerletSimulator physicSimu, ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition, ref Vector3D originalPosition);

        Vector3D GetHighestPoint(Vector3D vector2);
        IEnumerable<IAbstractChunk> AroundChunks(Vector3D vector3D, float radius = 10);
        IEnumerable<IStaticEntity> AroundEntities(Vector3D position, float radius);
    }
}