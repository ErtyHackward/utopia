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
using Utopia.Shared.Settings;
using Utopia.Shared.World.Processors.Utopia.Biomes;

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

        public static void InitializeProtobufInheritanceHierarchy()
        {
            var protoTypeModel = RuntimeTypeModel.Default;

            var entityType = protoTypeModel.Add(typeof(Entity), true);
            var dynEntityType = protoTypeModel.Add(typeof(DynamicEntity), true);
            var staticEntityType = protoTypeModel.Add(typeof(StaticEntity), true);
            var charEntityType = protoTypeModel.Add(typeof(CharacterEntity), true);
            var rpgCharType = protoTypeModel.Add(typeof(RpgCharacterEntity), true);
            var itemType = protoTypeModel.Add(typeof(Item), true);
            var slotType = protoTypeModel.Add(typeof(Slot), true);
            var containedSlotType = protoTypeModel.Add(typeof(ContainedSlot), true);
            var blockItem = protoTypeModel.Add(typeof(BlockItem), true);
            var orientedBlockItem = protoTypeModel.Add(typeof(OrientedBlockItem), true);
            var blockLinkedItem = protoTypeModel.Add(typeof(BlockLinkedItem), true);
            var orientedBlockLinkedItem = protoTypeModel.Add(typeof(OrientedBlockLinkedItem), true);
            var resourceCollector = protoTypeModel.Add(typeof(ResourcesCollector), true);
            var worldConfig = protoTypeModel.Add(typeof(WorldConfiguration), true);
            var soundSource = protoTypeModel.Add(typeof(SoundSource), true);


            soundSource.AddSubType(100, typeof(BiomeSoundSource));

            // world configs

            worldConfig.AddSubType(100, typeof(UtopiaWorldConfiguration));

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

            var vector2I = protoTypeModel.Add(typeof(Vector2I), true);
            vector2I.AddField(1, "X");
            vector2I.AddField(2, "Y");

            var byteColor = protoTypeModel.Add(typeof(ByteColor), true);
            byteColor.AddField(1, "R");
            byteColor.AddField(2, "G");
            byteColor.AddField(3, "B");
            byteColor.AddField(4, "A");

            var quaternion = protoTypeModel.Add(typeof(Quaternion), true);
            quaternion.AddField(1, "X");
            quaternion.AddField(2, "Y");
            quaternion.AddField(3, "Z");
            quaternion.AddField(4, "W");

            var vector4 = protoTypeModel.Add(typeof(Vector4), true);
            vector4.AddField(1, "X");
            vector4.AddField(2, "Y");
            vector4.AddField(3, "Z");
            vector4.AddField(4, "W");

            var matrix = protoTypeModel.Add(typeof(Matrix), true);
            matrix.AddField(1, "Row1");
            matrix.AddField(2, "Row2");
            matrix.AddField(3, "Row3");
            matrix.AddField(4, "Row4");

            var vector3 = protoTypeModel.Add(typeof(Vector3), true);
            vector3.AddField(1, "X");
            vector3.AddField(2, "Y");
            vector3.AddField(3, "Z");

            var vector3d = protoTypeModel.Add(typeof(Vector3D), true);
            vector3d.AddField(1, "X");
            vector3d.AddField(2, "Y");
            vector3d.AddField(3, "Z");

            var vector3i = protoTypeModel.Add(typeof(Vector3I), true);
            vector3i.AddField(1, "X");
            vector3i.AddField(2, "Y");
            vector3i.AddField(3, "Z");

            var rangeI = protoTypeModel.Add(typeof(RangeI), true);
            rangeI.AddField(1, "Min");
            rangeI.AddField(2, "Max");

            var rangeD = protoTypeModel.Add(typeof(RangeD), true);
            rangeD.AddField(1, "Min");
            rangeD.AddField(2, "Max");

            var rangeB = protoTypeModel.Add(typeof(RangeB), true);
            rangeB.AddField(1, "Min");
            rangeB.AddField(2, "Max");

            var color4 = protoTypeModel.Add(typeof(Color4), true);
            color4.AddField(1, "Alpha");
            color4.AddField(2, "Red");
            color4.AddField(3, "Green");
            color4.AddField(4, "Blue");

            var boundingbox = protoTypeModel.Add(typeof(BoundingBox), true);
            boundingbox.AddField(1, "Minimum");
            boundingbox.AddField(2, "Maximum");

        }

        public EntityFactory(ILandscapeManager2D landscapeManager)
        {
            

            // type hierarhy should be described here


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

            return (Entity)RuntimeTypeModel.Default.Deserialize(reader.BaseStream, entity, entity.GetType());
        }

        public void Serialize(Entity entity, Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
                writer.Write(entity.ClassId);

            Serializer.Serialize(stream, entity);
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
