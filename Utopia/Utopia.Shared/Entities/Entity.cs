using System.ComponentModel;
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
        private string _name = "No name";

        /// <summary>
        /// Pickable entity Property
        /// </summary>
        [Category("Entity")]
        public bool IsPickable { get; set; }

        /// <summary>
        /// Player Collision checked entity Property
        /// </summary>
        [Category("Entity")]
        public bool IsPlayerCollidable { get; set; }

        /// <summary>
        /// Gets entity class id
        /// </summary>
        [Category("Entity")]
        public abstract ushort ClassId { get; }

        /// <summary>
        /// Gets current entity type
        /// </summary>
        [Category("Entity")]
        public EntityType Type { get; protected set; }

        /// <summary>
        /// Entity maximum size
        /// </summary>
        [Category("Entity")]
        public Vector3 DefaultSize { get; set; }

        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        [Browsable(false)]
        public virtual Vector3D Position { get; set; }
        
        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        [Category("Entity")]
        public abstract string DisplayName { get; }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets Entity ID
        /// </summary>
        [Browsable(false)]
        public ushort Id { get; set; }

        /// <summary>
        /// Loads current entity from a binaryReader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="factory"> </param>
        public virtual void Load(BinaryReader reader, EntityFactory factory)
        {
            // we no need to read class id because it is read by entity factory
            // to find the final type of the class

            Id = reader.ReadUInt16();

            Type = (EntityType)reader.ReadByte();

            DefaultSize = reader.ReadVector3();
            Position = reader.ReadVector3D();

            IsPickable = reader.ReadBoolean();
            IsPlayerCollidable = reader.ReadBoolean();

        }

        /// <summary>
        /// Saves(serializes) current entity instance to a binaryWriter
        /// </summary>
        /// <param name="writer"></param>
        public virtual void Save(BinaryWriter writer)
        {
            writer.Write(ClassId);

            writer.Write(Id);
            writer.Write((byte)Type);

            writer.Write(DefaultSize);
            writer.Write(Position);

            writer.Write(IsPickable);
            writer.Write(IsPlayerCollidable);
        }

        public abstract EntityLink GetLink();


        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
