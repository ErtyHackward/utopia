using System;
using System.IO;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Performs creation of entities objects
    /// </summary>
    public class EntityFactory
    {
        private static EntityFactory _instance;

        /// <summary>
        /// Gets or sets instance of entity factory
        /// </summary>
        public static EntityFactory Instance
        {
            get { return _instance ?? (_instance = new EntityFactory()); }
            set { _instance = value; }
        }

        /// <summary>
        /// Returns new entity object by its classId
        /// </summary>
        /// <param name="classId">Entity class identificator</param>
        /// <returns></returns>
        public Entity CreateEntity(EntityId classId)
        {
            // todo: implement this method correctly

            switch (classId)
            {
                case EntityId.None:
                    break;
                case EntityId.Sword:
                    break;
                case EntityId.PickAxe:
                    break;
                case EntityId.Shovel:
                    break;
                case EntityId.Hoe:
                    break;
                case EntityId.Axe:
                    break;
                case EntityId.Chest:
                    break;
                case EntityId.Chair:
                    break;
                case EntityId.Door:
                    break;
                case EntityId.Bed:
                    break;
                case EntityId.ThinGlass:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("classId");
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates and loads entity from binary reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public Entity CreateFromBytes(BinaryReader reader)
        {
            var classId = reader.ReadUInt16();

            reader.BaseStream.Seek(-2, SeekOrigin.Current);

            var entity = CreateEntity((EntityId)classId);

            entity.Load(reader);

            return entity;
        }

    }
}
