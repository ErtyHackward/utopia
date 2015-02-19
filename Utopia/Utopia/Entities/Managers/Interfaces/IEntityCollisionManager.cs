using SharpDX;
using Utopia.Worlds.Chunks;
using S33M3CoreComponents.Physics.Verlet;
using S33M3Resources.Structs;

namespace Utopia.Entities.Managers.Interfaces
{
    public interface IEntityCollisionManager
    {
        void Update();

        bool IsDirty { get; set; }

        PlayerEntityManager Player { get; set; }

        IWorldChunks2D WorldChunks { get; set; }

        void IsCollidingWithEntity(VerletSimulator physicSimu,ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition, ref Vector3D originalPosition);
    }
}
