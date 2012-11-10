using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Settings;
using Utopia.Shared.Structs;
using Utopia.Shared.World.Processors.Utopia.Biomes;
using System.Linq;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Concrete.Collectible;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Configuration
{
    /// <summary>
    /// Contains all gameplay parameters of the realm
    /// Holds possible entities types, their names, world generator settings, defines everything
    /// Allows to save and load the realm configuration
    /// </summary>
    public abstract class WorldConfiguration
    {
        #region Private Variables
        /// <summary>
        /// Realm format version
        /// </summary>
        private const int RealmFormat = 1;
        #endregion

        #region Public Properties
        public readonly EntityFactory Factory;

        /// <summary>
        /// General realm display name
        /// </summary>
        public string ConfigurationName { get; set; }

        /// <summary>
        /// Author name of the realm
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Get, set the world processor attached
        /// </summary>
        [Browsable(false)]
        public WorldProcessors WorldProcessor { get; set; }

        /// <summary>
        /// World Height
        /// </summary>
        public virtual int WorldHeight { get; set; }        

        /// <summary>
        /// Datetime of the moment of creation
        /// </summary>
        [Browsable(false)]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Datetime of the last update
        /// </summary>
        [Browsable(false)]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Allows to comapre the realms equality
        /// </summary>
        [Browsable(false)]
        public Md5Hash IntegrityHash { get; set; }

        /// <summary>
        /// Holds examples of entities of all types in the realm
        /// </summary>
        [Browsable(false)]
        public List<IEntity> Entities { get; set; }

        /// <summary>
        /// Keep a list of the entities lookable by ConcreteId
        /// </summary>
        [Browsable(false)]
        public Dictionary<ushort, Entity> BluePrints { get; set; }
        /// <summary>
        /// Holds Cube Profiles configuration
        /// </summary>
        [Browsable(false)]
        public CubeProfile[] CubeProfiles { get; set; }

        /// <summary>
        /// Holds a server services list with parameters
        /// The key is a type name
        /// The value is a initializarion string
        /// 
        /// You can see strings here instead of Services itself because of need to link server library here
        /// </summary>
        [Browsable(false)]
        public List<KeyValuePair<string,string>> Services { get; set; }

        /// <summary>
        /// Key is a set Id
        /// Value is a container
        /// </summary>
        [Browsable(false)]
        [Description("Defines the sets used to fill containers")]
        public Dictionary<string, SlotContainer<BlueprintSlot>> ContainerSets { get; set; }

        #endregion

        public WorldConfiguration(EntityFactory factory = null, bool withHelperAssignation = false)
        {
            if (factory == null) factory = new EntityFactory(null);
            if (withHelperAssignation) EditorConfigHelper.Config = this;
            
            factory.Config = this; //Inject itself into the factory

            Factory = factory;
            WorldHeight = 128;

            InitCollections();
        }

        #region Public Methods

        #region Loading / Saving Configuration
        public void SaveToFile(string path)
        {
            using (var fs = new GZipStream(File.OpenWrite(path), CompressionMode.Compress))
            {
                var writer = new BinaryWriter(fs);
                Save(writer);
            }
        }

        public virtual void Save(BinaryWriter writer)
        {
            writer.Write((byte)WorldProcessor);

            writer.Write(RealmFormat);

            writer.Write(ConfigurationName ?? string.Empty);
            writer.Write(Author ?? string.Empty);
            writer.Write(CreatedAt.ToBinary());
            writer.Write(UpdatedAt.ToBinary());
            
            writer.Write(WorldHeight);

            writer.Write(Entities.Count);
            foreach (var entitySample in Entities)
            {
                entitySample.Save(writer);
            }

            writer.Write(CubeProfiles.Where(x => x != null && x.Name != "System Reserved").Count());
            foreach (var cubeProfile in CubeProfiles.Where(x => x != null && x.Name != "System Reserved"))
            {
                cubeProfile.Save(writer);
            }

            writer.Write(Services.Count);
            foreach (var pair in Services)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }

            writer.Write(ContainerSets.Count);
            foreach (var pair in ContainerSets)
            {
                writer.Write(pair.Key);
                pair.Value.Save(writer);
            }
        }

        public virtual void Load(BinaryReader reader)
        {
            InitCollections();

            WorldProcessor = (WorldProcessors)reader.ReadByte();

            var currentFormat = reader.ReadInt32();
            if (currentFormat != RealmFormat)
                throw new InvalidDataException("Unsupported realm config format, expected " + RealmFormat + " current " + currentFormat);

            ConfigurationName = reader.ReadString();
            Author = reader.ReadString();
            CreatedAt = DateTime.FromBinary(reader.ReadInt64());
            UpdatedAt = DateTime.FromBinary(reader.ReadInt64());
            
            WorldHeight = reader.ReadInt32();

            Entities.Clear();
            BluePrints.Clear();
            int countEntity = reader.ReadInt32();
            for (var i = 0; i < countEntity; i++)
            {
                var entity = Factory.CreateFromBytes(reader);
                Entities.Add(entity);
                BluePrints.Add(entity.BluePrintId, entity);
            }

            CubeProfiles = new CubeProfile[255];
            var countCubes = reader.ReadInt32();
            for (var i = 0; i < countCubes; i++)
            {
                var cp = new CubeProfile();
                cp.Load(reader);
                CubeProfiles[cp.Id] = cp;
            }

            FilledUpReservedCubeInArray();

            Services.Clear();
            var servicesCount = reader.ReadInt32();
            for (int i = 0; i < servicesCount; i++)
            {
                var pair = new KeyValuePair<string, string>(reader.ReadString(), reader.ReadString());
                Services.Add(pair);
            }

            ContainerSets.Clear();
            var setsCount = reader.ReadInt32();
            for (int i = 0; i < setsCount; i++)
            {
                var name = reader.ReadString();
                var slotContainer = new SlotContainer<BlueprintSlot>();
                slotContainer.Load(reader, null);

                ContainerSets.Add(name, slotContainer);
            }
        }

        public static WorldConfiguration LoadFromFile(string path, EntityFactory factory = null, bool withHelperAssignation = false)
        {
            string processorType;
            //Read the Generic type of the saved file.
            using (var fs = new GZipStream(File.OpenRead(path), CompressionMode.Decompress))
            {
                var reader = new BinaryReader(fs);
                processorType = "Utopia.Shared.Configuration." + ((WorldProcessors)reader.ReadByte()).ToString() + "ProcessorParams, Utopia.Shared";
            }

            Type type = typeof(WorldConfiguration<>).MakeGenericType(Type.GetType(processorType));
            WorldConfiguration configuration = (WorldConfiguration)Activator.CreateInstance(type, factory, withHelperAssignation);

            using (var fs = new GZipStream(File.OpenRead(path), CompressionMode.Decompress))
            {
                var reader = new BinaryReader(fs);
                configuration.Load(reader);
            }

            return configuration;
        }
        #endregion

        public object CreateNewCube()
        {
            //Get New Cube ID.
            //We keep the id from 0 to 100 for "System" cubes
            //101 to 254 for Custom created cubes
            byte newProfileId;
            if (CubeProfiles.Where(x => x != null).Count(x => x.Id > 100) > 1)
            {
                newProfileId = CubeProfiles.Where(x => x.Id > 100).Select(y => y.Id).Max();
            }
            else newProfileId = 100;

            CubeProfile newCubeProfile = new CubeProfile()
            {
                Name = "NewCustomCube",
                Id = newProfileId,
                Tex_Top = 1,
                Tex_Bottom = 1,
                Tex_Back = 1,
                Tex_Front = 1,
                Tex_Left = 1,
                Tex_Right = 1,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = false
            };

            CubeProfiles[newProfileId] = newCubeProfile;

            return newCubeProfile;
        }

        public object CreateNewEntity(Type entityClassType)
        {
            //Create a new EntityClass object
            IEntity instance = (IEntity)Activator.CreateInstance(entityClassType);
            AddNewEntity(instance);
            return instance;
        }

        public IEnumerable<CubeProfile> GettAllCubesProfiles()
        {
            foreach (var profile in CubeProfiles.Where(x => x != null && x.Name != "System Reserved"))
            {
                yield return profile;
            }
        }

        public void InjectMandatoryObjects()
        {
            CreateDefaultValues();
        }
        #endregion

        #region Private Methods

        private void InitCollections()
        {
            Entities = new List<IEntity>();
            BluePrints = new Dictionary<ushort, Entity>();
            CubeProfiles = new CubeProfile[255];
            Services = new List<KeyValuePair<string, string>>();
            ContainerSets = new Dictionary<string, SlotContainer<BlueprintSlot>>();
        }

        private void CreateDefaultValues()
        {
            //These are mandatory configuration !!
            CreateDefaultCubeProfiles();
            CreateDefaultEntities();
        }

        protected void AddNewEntity(IEntity entityInstance)
        {
            //Generate a new Entity ID, it will represent this Blue print, and must be unique
            ushort newId;
            if (Entities.Count == 0) newId = 0;
            else newId = (ushort)(Entities.Select(x => x.BluePrintId).Max(y => y) + 1);

            entityInstance.BluePrintId = newId;
            entityInstance.isSystemEntity = false;

            Entities.Add(entityInstance);
        }

        //Definition of default cube profile
        protected virtual void CreateDefaultCubeProfiles()
        {
            FilledUpReservedCubeInArray();
        }

        private void FilledUpReservedCubeInArray()
        {
            //Field up to 100 included for Reserved Cube ID
            for (byte currentCubeId = (byte)(CubeProfiles.Where(x => x != null && x.Id < 100).Max(x => x.Id) + 1); currentCubeId < 100; currentCubeId++)
            {
                CubeProfiles[currentCubeId] = new CubeProfile() { Name = "System Reserved", Id = currentCubeId };
            }
        }

        protected virtual void CreateDefaultEntities()
        {
        }

        #endregion

        #region Inner Classes

        public static class CubeId
        {
            public const byte Air = 0;
        }

        /// <summary>
        /// Defines world processor possible types
        /// </summary>
        public enum WorldProcessors : byte
        {
            Flat,
            Utopia
            //Plan
        }
        #endregion
    }
}
