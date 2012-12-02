using System;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Chunks.Tags;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Performs creation of entities objects
    /// </summary>
    public class EntityFactory
    {
        private RuntimeTypeModel _protoTypeModel;


        public WorldConfiguration Config { get; set; }

        /// <summary>
        /// Gets landscape manager used to create new tools
        /// </summary>
        public ILandscapeManager2D LandscapeManager { get; set; }

        public EntityFactory(ILandscapeManager2D landscapeManager)
        {
            _protoTypeModel = RuntimeTypeModel.Default;

            // type hierarhy should be described here

            var entityType = _protoTypeModel.Add(typeof(Entity), true);
            var dynEntityType = _protoTypeModel.Add(typeof(DynamicEntity), true);
            var staticEntityType = _protoTypeModel.Add(typeof(StaticEntity), true);
            var charEntityType = _protoTypeModel.Add(typeof(CharacterEntity), true);
            var rpgCharType = _protoTypeModel.Add(typeof(RpgCharacterEntity), true);
            var itemType = _protoTypeModel.Add(typeof(Item), true);
            var slotType = _protoTypeModel.Add(typeof(Slot), true);
            var containedSlotType = _protoTypeModel.Add(typeof(ContainedSlot), true);
            var blockItem = _protoTypeModel.Add(typeof(BlockItem), true);
            var orientedBlockItem = _protoTypeModel.Add(typeof(OrientedBlockItem), true);
            var blockLinkedItem = _protoTypeModel.Add(typeof(BlockLinkedItem), true);
            var orientedBlockLinkedItem = _protoTypeModel.Add(typeof(OrientedBlockLinkedItem), true);
            var resourceCollector = _protoTypeModel.Add(typeof(ResourcesCollector), true);

            // entities hierarchy

            entityType.AddSubType(100, typeof(DynamicEntity));
            entityType.AddSubType(101, typeof(StaticEntity));

            dynEntityType.AddSubType(100, typeof(CharacterEntity));

            charEntityType.AddSubType(100, typeof(RpgCharacterEntity));
            charEntityType.AddSubType(101, typeof(Zombie));

            rpgCharType.AddSubType(100, typeof(PlayerCharacter));

            staticEntityType.AddSubType(100, typeof(Item));

            itemType.AddSubType(100, typeof(BlockItem));
            itemType.AddSubType(101, typeof(BlockLinkedItem));
            itemType.AddSubType(102, typeof(ResourcesCollector));
            itemType.AddSubType(103, typeof(CubeResource));

            blockItem.AddSubType(100, typeof(OrientedBlockItem));

            orientedBlockItem.AddSubType(100, typeof(Door));

            blockLinkedItem.AddSubType(100, typeof(OrientedBlockLinkedItem));
            blockLinkedItem.AddSubType(101, typeof(Plant));
            blockLinkedItem.AddSubType(102, typeof(SideLightSource));

            orientedBlockLinkedItem.AddSubType(100, typeof(Container));

            resourceCollector.AddSubType(100, typeof(BasicCollector));

            // slots hierarchy

            slotType.AddSubType(100, typeof(ContainedSlot));

            containedSlotType.AddSubType(100, typeof(BlueprintSlot));



            // add mappings for 3rd party objects

            var vector2I = _protoTypeModel.Add(typeof(Vector2I), true);
            vector2I.AddField(1, "X");
            vector2I.AddField(2, "Y");

            var byteColor = _protoTypeModel.Add(typeof(ByteColor), true);
            byteColor.AddField(1, "R");
            byteColor.AddField(2, "G");
            byteColor.AddField(3, "B");
            byteColor.AddField(4, "A");

            var quaternion = _protoTypeModel.Add(typeof(Quaternion), true);
            quaternion.AddField(1, "X");
            quaternion.AddField(2, "Y");
            quaternion.AddField(3, "Z");
            quaternion.AddField(4, "W");

            var vector4 = _protoTypeModel.Add(typeof(Vector4), true);
            vector4.AddField(1, "X");
            vector4.AddField(2, "Y");
            vector4.AddField(3, "Z");
            vector4.AddField(4, "W");

            var matrix = _protoTypeModel.Add(typeof(Matrix), true);
            matrix.AddField(1, "Row1");
            matrix.AddField(2, "Row2");
            matrix.AddField(3, "Row3");
            matrix.AddField(4, "Row4");

            var vector3 = _protoTypeModel.Add(typeof(Vector3), true);
            vector3.AddField(1, "X");
            vector3.AddField(2, "Y");
            vector3.AddField(3, "Z");

            var vector3d = _protoTypeModel.Add(typeof(Vector3D), true);
            vector3d.AddField(1, "X");
            vector3d.AddField(2, "Y");
            vector3d.AddField(3, "Z");

            var vector3i = _protoTypeModel.Add(typeof(Vector3I), true);
            vector3i.AddField(1, "X");
            vector3i.AddField(2, "Y");
            vector3i.AddField(3, "Z");

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
            var entity = CreateCustomEntity(classId); // External implementation of the entity creation.

            if (entity == null)
            {
                switch (classId)
                {
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
                    case EntityClassId.OrientedBlockLinkedItem:
                        entity = new OrientedBlockLinkedItem();
                        break;
                    case EntityClassId.OrientedBlockItem:
                        entity = new OrientedBlockItem();
                        break;
                    case EntityClassId.Container:
                        entity = new Container();
                        break;
                    case EntityClassId.Door:
                        entity = new Door();
                        break;
                    case EntityClassId.BasicCollector:
                        entity = new BasicCollector();
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

            return (Entity)_protoTypeModel.Deserialize(reader.BaseStream, entity, entity.GetType());
        }

        public void Serialize(Entity entity, Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
                writer.Write(entity.ClassId);

            _protoTypeModel.Serialize(stream, entity);
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
            
            return (BlockTag)RuntimeTypeModel.Default.Deserialize(reader.BaseStream, tag, tag.GetType());
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
                    Item item;
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
