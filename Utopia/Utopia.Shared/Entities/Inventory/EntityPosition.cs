using SharpDX;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Contains entity position and rotation
    /// </summary>
    public struct EntityPosition
    {
        public Vector3 Position;

        public Quaternion Rotation;
    }
}