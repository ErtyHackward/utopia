using System.IO;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Represents a base entity 
    /// </summary>
    public abstract class Entity : IEntity
    {
        /// <summary>
        /// Pickable entity Property
        /// </summary>
        public virtual bool IsPickable { get { return true; } }

        /// <summary>
        /// Player Collision checked entity Property
        /// </summary>
        public virtual bool IsPlayerCollidable { get { return false; } }

        /// <summary>
        /// Gets entity class id
        /// </summary>
        public abstract ushort ClassId { get; }

        /// <summary>
        /// Gets current entity type
        /// </summary>
        public EntityType Type { get; protected set; }

        /// <summary>
        /// Entity maximum size
        /// </summary>
        public virtual Vector3 Size { get; set; }

        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        public virtual Vector3D Position { get; set; }

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

            Vector3 entitySize;
            entitySize.X = reader.ReadSingle();
            entitySize.Y = reader.ReadSingle();
            entitySize.Z = reader.ReadSingle();
            Size = entitySize;

            Vector3D position;
            position.X = reader.ReadDouble();
            position.Y = reader.ReadDouble();
            position.Z = reader.ReadDouble();
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
            writer.Write(ClassId);

            writer.Write((byte)Type);

            writer.Write(Size.X);
            writer.Write(Size.Y);
            writer.Write(Size.Z);

            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);

            writer.Write(Rotation.X);
            writer.Write(Rotation.Y);
            writer.Write(Rotation.Z);
            writer.Write(Rotation.W);

        }

        public abstract EntityLink GetLink();
    }
}
