using System;
using System.IO;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Concrete.Collectible;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Performs creation of entities objects
    /// </summary>
    public class EntityFactory
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static EntityFactory Instance;

        /// <summary>
        /// Gets landscape manager used to create new tools
        /// </summary>
        public ILandscapeManager2D LandscapeManager { get; private set; }

        public EntityFactory(ILandscapeManager2D landscapeManager)
        {
            LandscapeManager = landscapeManager;
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
                        entity = new Grass(LandscapeManager);
                        break;
                    case EntityClassId.Flower1:
                        entity = new Flower1(LandscapeManager);
                        break;
                    case EntityClassId.Flower2:
                        entity = new Flower2(LandscapeManager);
                        break;
                    case EntityClassId.Mushroom1:
                        entity = new Mushr1(LandscapeManager);
                        break;
                    case EntityClassId.Mushroom2:
                        entity = new Mushr2(LandscapeManager);
                        break;
                    case EntityClassId.Tree:
                        entity = new Tree();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("classId");
                }
            }

            // allow post produce prepare
            OnEntityCreated(new EntityFactoryEventArgs { Entity = entity });

            return entity;
        }

        /// <summary>
        /// Creates and loads entity from binary form
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
