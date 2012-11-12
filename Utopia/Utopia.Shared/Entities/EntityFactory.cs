using System;
using System.IO;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Concrete.Collectible;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Chunks.Tags;
using System.Collections.Generic;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Performs creation of entities objects
    /// </summary>
    public class EntityFactory
    {

        public WorldConfiguration Config { get; set; }

        /// <summary>
        /// Gets landscape manager used to create new tools
        /// </summary>
        public ILandscapeManager2D LandscapeManager { get; set; }

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
        /// Creates an entity by its type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateEntity<T>() where T: Entity, new()
        {
            var entity = new T();

            InjectFields(entity);

            // allow post produce prepare
            OnEntityCreated(new EntityFactoryEventArgs { Entity = entity });

            return entity;
        }

        /// <summary>
        /// Returns new entity object by its classId. New entity will have unique ID
        /// </summary>
        /// <param name="classId">Entity class identificator</param>
        /// <returns></returns>
        public Entity CreateFromClassId(ushort classId)
        {
            // todo: implement this method correctly, create appropriate class here
            var entity = CreateCustomEntity(classId); // External implementation of the entity creation.

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
                    case EntityClassId.Plant:
                        entity = new Plant();
                        break;
                    case EntityClassId.CubeResource:
                        entity = new CubeResource();
                        break;
                    case EntityClassId.SideLightSource:
                        entity = new SideLightSource();
                        break;
                    case EntityClassId.Container:
                        entity = new Container();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("classId");
                }
            }

            InjectFields(entity);

            // allow post produce prepare
            OnEntityCreated(new EntityFactoryEventArgs { Entity = entity });

            return entity;
        }

        public Entity CreateFromBluePrint(IEntity entity)
        {
            return CreateFromBluePrint(entity.BluePrintId);
        }

        public Entity CreateFromBluePrint(ushort bluePrintID)
        {
            if (bluePrintID == 0)
            {
                //The bluePrintID 0 means not linked to a blueprint !
                throw new ArgumentOutOfRangeException("bluePrintID");
            }

            Entity entity = null;
            if (Config.BluePrints.TryGetValue(bluePrintID, out entity) == false)
            {
                throw new ArgumentOutOfRangeException("bluePrintID");
            }

            //Create a clone of this entity.
            entity = (Entity)entity.Clone();

            InjectFields(entity);

            // allow post produce prepare
            OnEntityCreated(new EntityFactoryEventArgs { Entity = entity });

            return entity;
        }

        /// <summary>
        /// Sets required field for special types of entities
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void InjectFields(Entity entity)
        {
            if (entity is IWorldIntercatingEntity)
            {
                var item = entity as IWorldIntercatingEntity;
                item.LandscapeManager = LandscapeManager;
                item.entityFactory = this;
            }
        }

        /// <summary>
        /// Creates and loads blueprint entity from binary form
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public Entity CreateFromBytes(BinaryReader reader)
        {
            var classId = reader.ReadUInt16();
            
            var entity = CreateFromClassId(classId);

            entity.Load(reader, this);

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

        public static BlockTag CreateTagFromBytes(BinaryReader reader)
        {
            var tagId = reader.ReadByte();

            if (tagId == 0) return null;

            if (tagId != 1)
                throw new InvalidDataException();

            var tag = new LiquidTag();
            tag.Load(reader);

            return tag;
        }

        /// <summary>
        /// Initializes specifed container with a set
        /// </summary>
        /// <param name="setName"></param>
        /// <param name="container"></param>
        public void FillContainer(string setName, SlotContainer<ContainedSlot> container)
        {
            SlotContainer<BlueprintSlot> set;
            if (Config.ContainerSets.TryGetValue(setName, out set))
            {
                if (container.GridSize.X < set.GridSize.X || container.GridSize.Y < set.GridSize.Y)
                    throw new InvalidOperationException("Destination container is smaller than the set");

                container.Clear();

                foreach (var blueprintSlot in set)
                {
                    IItem item = null;
                    if (blueprintSlot.BlueprintId < 256)
                    {
                        var res = CreateEntity<CubeResource>();
                        //var res = new CubeResource();
                        var profile = Config.CubeProfiles[blueprintSlot.BlueprintId];
                        res.SetCube((byte)blueprintSlot.BlueprintId, profile.Name);
                        item = res;
                    }
                    else
                    {
                        item = (Item)CreateFromBluePrint(blueprintSlot.BlueprintId);
                    }

                    container.PutItem(item, blueprintSlot.GridPosition, blueprintSlot.ItemsCount);
                }
            }
        }

    }
}
