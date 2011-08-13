using System.IO;
using SharpDX;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Represents a base entity 
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Gets entity class id
        /// </summary>
        public EntityId ClassId { get; protected set; }

        /// <summary>
        /// Gets current entity type
        /// </summary>
        public EntityType Type { get; protected set; }

        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets entity rotation information
        /// </summary>
        public Quaternion Rotation { get; set; }
        
        /// <summary>
        /// Loads current entity from a binaryReader
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Load(BinaryReader reader)
        {
            ClassId = (EntityId)reader.ReadUInt16();

            Type = (EntityType)reader.ReadByte();

            Vector3 position;
            position.X = reader.ReadSingle();
            position.Y = reader.ReadSingle();
            position.Z = reader.ReadSingle();
            Position = position;

            Quaternion quaternion;
            quaternion.X = reader.ReadSingle();
            quaternion.Y = reader.ReadSingle();
            quaternion.Z = reader.ReadSingle();
            quaternion.W = reader.ReadSingle();
            Rotation = quaternion;
        }

        /// <summary>
        /// Saves(serializes) current entity instance to a binaryWriter
        /// </summary>
        /// <param name="writer"></param>
        public virtual void Save(BinaryWriter writer)
        {
            writer.Write((ushort)ClassId);

            writer.Write((byte)Type);

            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);

            writer.Write(Rotation.X);
            writer.Write(Rotation.Y);
            writer.Write(Rotation.Z);
            writer.Write(Rotation.W);

        }
    }
}
