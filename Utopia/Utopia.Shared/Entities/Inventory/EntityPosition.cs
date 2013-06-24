using ProtoBuf;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Entities.Concrete;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Contains entity position and rotation
    /// </summary>
    [ProtoContract]
    public struct EntityPosition
    {
        /// <summary>
        /// Position of the entity
        /// </summary>
        [ProtoMember(1)]
        public Vector3D Position;

        /// <summary>
        /// Rotation of the entity
        /// </summary>
        [ProtoMember(2)]
        public Quaternion Rotation;

        /// <summary>
        /// Item orientation
        /// </summary>
        [ProtoMember(3)]
        public ItemOrientation Orientation;

        /// <summary>
        /// Indicates if entity can be placed or not
        /// </summary>
        public bool Valid;
    }
}