using System.IO;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

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
        public virtual Vector3 DefaultSize { get; set; }

        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        public virtual Vector3D Position { get; set; }
        
        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Loads current entity from a binaryReader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="factory"> </param>
        public virtual void Load(BinaryReader reader, EntityFactory factory)
        {
            // skipping entity class id
            reader.ReadUInt16();

            Type = (EntityType)reader.ReadByte();

            DefaultSize = reader.ReadVector3();
            Position = reader.ReadVector3D();
        }

        /// <summary>
        /// Saves(serializes) current entity instance to a binaryWriter
        /// </summary>
        /// <param name="writer"></param>
        public virtual void Save(BinaryWriter writer)
        {
            writer.Write(ClassId);

            writer.Write((byte)Type);

            writer.Write(DefaultSize);
            writer.Write(Position);
        }

        public abstract EntityLink GetLink();
    }
}
