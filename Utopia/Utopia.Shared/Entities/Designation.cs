using ProtoBuf;
using S33M3Resources.Structs;
using SharpDX;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Contains information about placing an item somewhere
    /// </summary>
    [ProtoContract]
    public class Designation
    {
        /// <summary>
        /// An id for an entity to be placed
        /// </summary>
        public ushort BlueprintId { get; set; }

        /// <summary>
        /// Desired entity position
        /// </summary>
        public Vector3D Position { get; set; }

        /// <summary>
        /// Desired entity rotation
        /// </summary>
        public Quaternion Rotation { get; set; }
    }
}