using System;
using System.IO;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Concrete.Collectible;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Performs creation of entities objects
    /// </summary>
    public class EntityFactory
    {
        private static EntityFactory _instance;
        private readonly object _synObject = new object();

        /// <summary>
        /// Gets or sets instance of entity factory
        /// </summary>
        public static EntityFactory Instance
        {
            get { return _instance ?? (_instance = new EntityFactory()); }
            set { _instance = value; }
        }

        private uint _lastId;

        /// <summary>
        /// Sets the maximum id value to generate unique EntityId values
        /// </summary>
        /// <param name="id"></param>
        public void SetLastId(uint id)
        {
            lock (_synObject)
            {
                _lastId = id;
            }
        }

        public uint GetUniqueEntityId()
        {
            lock (_synObject)
            {
                return ++_lastId;
            }
        }

        /// <summary>
        /// Occurs when entity was created, this stage can be used to prepare entity for release
        /// </summary>
        public event EventHandler<EntityFactoryEventArgs> EntityCreated;

        protected void OnEntityCreated(EntityFactoryEventArgs e)
        {
            var handler = EntityCreated;
            if (handler != null) handler(this, e);
        }

        protected virtual Entity CreateCustomEntity(ushort classId)
        {
            return null;
        }

        /// <summary>
        /// Returns new entity object by its classId. New entity will have unique ID
        /// </summary>
        /// <param name="classId">Entity class identificator</param>
        /// <returns></returns>
        public Entity CreateEntity(ushort classId)
        {
            // todo: implement this method correctly, create appropriate class here

            var entity = CreateCustomEntity(classId);

            if (entity == null)
            {
                switch (classId)
                {
                    case EntityClassId.None:
                        entity = new NoEntity();
                        break;
                    case EntityClassId.PlayerCharacter:
                        entity = new PlayerCharacter();
                        break;
                    case EntityClassId.Zombie:
                        entity = new Zombie();
                        break;
                    case EntityClassId.Grass:
                        entity = new Grass();
                        break;
                    case EntityClassId.Flower1:
                        entity = new Flower1();
                        break;
                    case EntityClassId.Flower2:
                        entity = new Flower2();
                        break;
                    case EntityClassId.Mushroom1:
                        entity = new Mushr1();
                        break;
                    case EntityClassId.Mushroom2:
                        entity = new Mushr2();
                        break;
                    case EntityClassId.Tree:
                        entity = new Tree();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("classId");
                }
            }

            entity.EntityId = GetUniqueEntityId();

            // allow post produce prepare
            OnEntityCreated(new EntityFactoryEventArgs { Entity = entity });

            return entity;
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

            var entity = CreateEntity(classId);

            entity.Load(reader);

            return entity;
        }

        public Entity CreateFromBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var reader = new BinaryReader(ms);
                return CreateFromBytes(reader);
            }
        }

    }
}
