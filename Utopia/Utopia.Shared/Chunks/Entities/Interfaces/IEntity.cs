using System.IO;
using SharpDX;
using S33M3Engines.Shared.Math;

namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    public interface IEntity
    {
        /// <summary>
        /// Gets entity class id
        /// </summary>
        EntityClassId ClassId { get; }

        /// <summary>
        /// Entity maximum size
        /// </summary>
        Vector3 Size { get; set; }

        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        DVector3 Position { get; set; }

        /// <summary>
        /// Gets or sets entity rotation information
        /// </summary>
        Quaternion Rotation { get; set; }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets an unique entity identificator
        /// </summary>
        uint EntityId { get; }


        void Save(BinaryWriter writer);
        void Load(BinaryReader reader);
    }
}