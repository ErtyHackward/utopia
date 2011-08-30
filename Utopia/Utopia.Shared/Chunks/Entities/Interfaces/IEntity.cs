using SharpDX;

namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    public interface IEntity
    {
        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets entity rotation information
        /// </summary>
        Quaternion Rotation { get; set; }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        string DisplayName { get; }
    }
}