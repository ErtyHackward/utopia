using System;
using System.IO;
using Ninject;
using ProtoBuf;
using ProtoBuf.Meta;
using S33M3CoreComponents.Sound;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server.Managers;
using Utopia.Shared.Settings;
using Utopia.Shared.Structs;
using Utopia.Shared.World.Processors.Utopia.Biomes;
using Utopia.Shared.Entities.Sound;
using Utopia.Shared.LandscapeEntities;
using Utopia.Shared.LandscapeEntities.Trees;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Performs creation of entities objects
    /// </summary>
    public class EntityFactory
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public WorldConfiguration Config { get; set; }

        /// <summary>
        /// Gets landscape manager used to create new tools
        /// </summary>
        [Inject]
        public ILandscapeManager LandscapeManager { get; set; }

        /// <summary>
        /// Gets dynamic entity manager
        /// </summary>
        public IDynamicEntityManager DynamicEntityManager { get; set; }

        /// <summary>
        /// Gets faction manager
        /// </summary>
        public IGlobalStateManager GlobalStateManager { get; set; }

        /// <summary>
        /// Gets main schedule manager (presented only on the server side)
        /// </summary>
        public IScheduleManager ScheduleManager { get; set; }

        /// <summary>
        /// Gets or sets optional sound manager used by ISoundEmitterEntities
        /// </summary>
        [Inject]
        public ISoundEngine SoundEngine { get; set; }

        /// <summary>
        /// Gets or sets value that indicates if this server side factory 
        /// </summary>
        public bool ServerSide { get; set; }

        public static void InitializeProtobufInheritanceHierarchy()
        {
            var protoTypeModel = RuntimeTypeModel.Default;

            var entityInterface =           protoTypeModel.Add(typeof(IEntity), true); // we need to register all entities hierarchy
            var slotType =                  protoTypeModel.Add(typeof(Slot), true);
            var containedSlotType =         protoTypeModel.Add(typeof(ContainedSlot), true);
            var worldConfig =               protoTypeModel.Add(typeof(WorldConfiguration), true);
            var soundSource =               protoTypeModel.Add(typeof(SoundSource), true);
            var chunkDataProvider =         protoTypeModel.Add(typeof(ChunkDataProvider), true);
            var landscapeEntityBluePrint =  protoTypeModel.Add(typeof(LandscapeEntityBluePrint), true);

            landscapeEntityBluePrint.AddSubType(100, typeof(TreeBluePrint));

            chunkDataProvider.AddSubType(100, typeof(InsideDataProvider));
            chunkDataProvider.AddSubType(101, typeof(SingleArrayDataProvider));

            soundSource.AddSubType(100, typeof(BiomeSoundSource));
            soundSource.AddSubType(101, typeof(StaticEntitySoundSource));

            // world configs

            worldConfig.AddSubType(100, typeof(UtopiaWorldConfiguration));
            worldConfig.AddSubType(101, typeof(FlatWorldConfiguration));

            // slots hierarchy

            var slotContainer = protoTypeModel.Add(typeof(SlotContainer<ContainedSlot>), true);

            slotContainer.AddSubType(100, typeof(CharacterEquipment));

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

            var vector2 = protoTypeModel.Add(typeof(Vector2), true);
            vector2.AddField(1, "X");
            vector2.AddField(2, "Y");

            var matrix = protoTypeModel.Add(typeof(Matrix), true);
            matrix.AddField(1, "Row1");
            matrix.AddField(2, "Row2");
            matrix.AddField(3, "Row3");
            matrix.AddField(4, "Row4");

            var vector3 = protoTypeModel.Add(typeof(Vector3), true);
            vector3.AddField(1, "X");
            vector3.AddField(2, "Y");
            vector3.AddField(3, "Z");

            var vector3D = protoTypeModel.Add(typeof(Vector3D), true);
            vector3D.AddField(1, "X");
            vector3D.AddField(2, "Y");
            vector3D.AddField(3, "Z");

            var vector3I = protoTypeModel.Add(typeof(Vector3I), true);
            vector3I.AddField(1, "X");
            vector3I.AddField(2, "Y");
            vector3I.AddField(3, "Z");

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
            
            var iBinaryMessage = protoTypeModel.Add(typeof(IBinaryMessage), true);
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

        public T CreateFromBluePrint<T>(ushort bluePrintId) where T : Entity
        {
            return (T)CreateFromBluePrint(bluePrintId);
        }

        public Entity CreateFromBluePrint(ushort bluePrintId)
        {
            if (bluePrintId == 0)
            {
                //The bluePrintID 0 means not linked to a blueprint !
                throw new ArgumentOutOfRangeException("bluePrintId");
            }

            Entity entity;

            //Block creation
            if (bluePrintId < 256)
            {
                var res = CreateEntity<CubeResource>();
                res.BluePrintId = bluePrintId;
                var profile = Config.BlockProfiles[bluePrintId];
                res.CubeId = (byte)bluePrintId;
                res.Name = profile.Name;
                res.MaxStackSize = Config.CubeStackSize;
                res.PutSound = Config.ResourcePut;
                entity = res;
            }
            else
            {
                if (Config.BluePrints.TryGetValue(bluePrintId, out entity) == false)
                {
                    throw new ArgumentOutOfRangeException("bluePrintId");
                }

                //Create a clone of this entity.
                entity = (Entity)entity.Clone();
            }

            InjectFields(entity);

            entity.FactoryInitialize();

            // allow post produce prepare
            OnEntityCreated(new EntityFactoryEventArgs { Entity = entity });

            return entity;
        }

        /// <summary>
        /// Sets required field for special types of entities
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void InjectFields(IEntity entity)
        {
            var interactingEntity = entity as IWorldInteractingEntity;
            if (interactingEntity != null)
            {
                var item = interactingEntity;
                item.EntityFactory = this;
            }

            var emitterEntity = entity as ISoundEmitterEntity;
            if (emitterEntity != null)
            {
                var item = emitterEntity;
                item.SoundEngine = SoundEngine;
            }

            var custInit = entity as ICustomInitialization;
            if (custInit != null)
            {
                custInit.Initialize(this);
            }
        }
        
        public void Serialize(Entity entity, Stream stream)
        {
            Serializer.Serialize(stream, entity);
        }

        /// <summary>
        /// Creates and loads blueprint entity from binary form
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public Entity CreateFromBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var entity = (Entity)RuntimeTypeModel.Default.Deserialize(ms, null, typeof(IEntity));
                PrepareEntity(entity);
                return entity;
            }
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
                    try
                    {
                        var item = (Item)CreateFromBluePrint(blueprintSlot.BlueprintId);
                        container.PutItem(item, blueprintSlot.GridPosition, blueprintSlot.ItemsCount);
                    }
                    catch (Exception x)
                    {
                        logger.Error("Unable to create the item from blueprint {0}: {1}", blueprintSlot.BlueprintId, x.Message);
                    }
                }
            }
        }

        //Analyse Network Message, and if needed Inject Field to the passed in Object
        public void ProcessMessage(IBinaryMessage imsg)
        {
            switch ((MessageTypes)imsg.MessageId)
            {
                case MessageTypes.EntityData:
                {
                    var msg = (EntityDataMessage)imsg;
                    if (msg.Entity != null)
                    {
                        PrepareEntity(msg.Entity);
                    }

                }
                    break;
                case MessageTypes.EntityIn:
                {
                    var msg = (EntityInMessage)imsg;

                    if (msg.Entity != null)
                    {
                        PrepareEntity(msg.Entity);
                    }
                }
                    break;
                case MessageTypes.EntityEquipment:
                {
                    var msg = (EntityEquipmentMessage)imsg;

                    if (msg.Entity != null)
                    {
                        PrepareEntity(msg.Entity);
                    }
                }
                    break;
            }
        }

        //Prepare Entity by binding needed object to it:
        //LandscapeManager or
        //SoundEngine
        public void PrepareEntity(IEntity entity)
        {
            InjectFields(entity); //Inject fields at the entity level

            if (entity is CharacterEntity)
            {
                var charEntity = (CharacterEntity)entity;

                foreach (var slot in charEntity.Equipment)
                {
                    InjectFields(slot.Item); //Inject Equipment fields
                }

                foreach (var slot in charEntity.Inventory)
                {
                    InjectFields(slot.Item); //Inject Inventory fields
                }

                InjectFields(charEntity.HandTool);
            }

            if (entity is GodEntity)
            {
                var godEntity = (GodEntity)entity;
                InjectFields(godEntity.GodHand);
            }
        }
        
        public void PrepareEntities(EntityCollection entityCollection)
        {
            foreach (var staticEntity in entityCollection.EnumerateFast())
            {
                PrepareEntity(staticEntity);
            }
        }
    }
}
