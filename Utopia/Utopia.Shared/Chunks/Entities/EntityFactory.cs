using System;
using System.IO;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;

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
        public Entity CreateEntity(EntityClassId classId)
        {
            // todo: implement this method correctly, create appropriate class here

            switch (classId)
            {
                case EntityClassId.None: throw new InvalidOperationException("Unable to create entity on none class.");
                case EntityClassId.Sword:
                    break;
                case EntityClassId.PickAxe: return new Pickaxe();
                case EntityClassId.Shovel:
                    break;
                case EntityClassId.Hoe:
                    break;
                case EntityClassId.Axe:
                    break;
                case EntityClassId.Chest:
                    break;
                case EntityClassId.Chair:
                    break;
                case EntityClassId.Door:
                    break;
                case EntityClassId.Bed:
                    break;
                case EntityClassId.ThinGlass:
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

            var entity = CreateEntity((EntityClassId)classId);

            entity.Load(reader);

            return entity;
        }

    }
}
