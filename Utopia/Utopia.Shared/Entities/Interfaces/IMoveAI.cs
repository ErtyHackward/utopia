using S33M3CoreComponents.Physics.Verlet;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Interfaces
{
    public interface IMoveAI
    {
        VerletSimulator VerletSimulator { get; }
        bool IsMooving { get; }
        Path3D CurrentPath { get; }

        /// <summary>
        /// Gets or sets the entity to follow
        /// </summary>
        IDynamicEntity Leader { get; set; }

        Vector3D MoveVector { get; set; }
        void Goto(Vector3I location);
    }
}