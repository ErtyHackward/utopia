using System.IO;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Represents a base entity 
    /// </summary>
    public abstract class Entity : IBinaryStorable, IEntity
    {
        /// <summary>
        /// Gets entity class id
        /// </summary>
        public abstract EntityClassId ClassId { get; }

        /// <summary>
        /// Gets current entity type
        /// </summary>
        public EntityType Type { get; protected set; }

        /// <summary>
        /// Unique entity identification number
        /// </summary>
        public uint EntityId { get; set; }
        
        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        public virtual Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets entity rotation information
        /// </summary>
        public virtual Quaternion Rotation { get; set; }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        public abstract string DisplayName { get; }
        
        /// <summary>
        /// Loads current entity from a binaryReader
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Load(BinaryReader reader)
        {
            // skipping entity class id
            reader.ReadUInt16();

            Type = (EntityType)reader.ReadByte();
            EntityId = reader.ReadUInt32();

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
            writer.Write(EntityId);

            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);

            writer.Write(Rotation.X);
            writer.Write(Rotation.Y);
            writer.Write(Rotation.Z);
            writer.Write(Rotation.W);

        }

        public override int GetHashCode()
        {
            return (int)EntityId;
        }

        public override bool Equals(object obj)
        {
            if(obj == null) return false;
            if(obj.GetType() != GetType()) return false;

            return (obj as Entity).EntityId == EntityId;
        }

    }
}
