using S33M3Resources.Structs;
using SharpDX;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Contains pick result information
    /// </summary>
    public struct EntityPickResult
    {
        /// <summary>
        /// True if entity is picked
        /// </summary>
        public bool Found;

        /// <summary>
        /// Distance to the picked entity
        /// </summary>
        public float Distance;

        /// <summary>
        /// Picked entity object
        /// </summary>
        public VisualEntity PickedEntity;

        /// <summary>
        /// Entity surface point
        /// </summary>
        public Vector3 PickPoint;

        /// <summary>
        /// Normal vector at the PickPoint
        /// </summary>
        public Vector3I PickNormal;
    }
}