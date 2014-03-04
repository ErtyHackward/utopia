using SharpDX;
using Utopia.Worlds.Chunks;
using S33M3CoreComponents.Physics.Verlet;
using S33M3Resources.Structs;

namespace Utopia.Entities.Managers.Interfaces
{
    public interface IEntityPickingManager
    {
        void Update();

        bool isDirty { get; set; }

        PlayerEntityManager Player { get; set; }

        IWorldChunks WorldChunks { get; set; }

        /// <summary>
        /// Checks nearby entities intersection with the pickingRay
        /// </summary>
        /// <param name="pickingRay">Ray to check intersection</param>
        /// <returns></returns>
        EntityPickResult CheckEntityPicking(Ray pickingRay);

        void isCollidingWithEntity(VerletSimulator physicSimu,ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition, ref Vector3D originalPosition);
    }
}
