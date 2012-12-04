using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using ProtoBuf;
using ProtoBuf.Meta;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Settings;
using Utopia.Shared.Structs;
using Utopia.Shared.Tools;
using System.Linq;

namespace Utopia.Shared.Configuration
{
    /// <summary>
    /// Contains all gameplay parameters of the realm
    /// Holds possible entities types, their names, world generator settings, defines everything
    /// Allows to save and load the realm configuration
    /// </summary>
    [ProtoContract]
    public abstract class WorldConfiguration
    {
        /// <summary>
        /// Realm format version
        /// </summary>
        private const int RealmFormat = 1;

        private const string FormatMagic = "s33m3&Erty Hackward";


        #region Public Properties

        public abstract int ConfigType { get; }

        public readonly EntityFactory Factory;

        /// <summary>
        /// General realm display name
        /// </summary>
        [ProtoMember(1)]
        public string ConfigurationName { get; set; }

        /// <summary>
        /// Author name of the realm
        /// </summary>
        [ProtoMember(2)]
        public string Author { get; set; }

        /// <summary>
        /// Get, set the world processor attached
        /// </summary>
        [Browsable(false)]
        [ProtoMember(3)]
        public WorldProcessors WorldProcessor { get; set; }

        /// <summary>
        /// World Height
        /// </summary>
        [ProtoMember(4)]
        public virtual int WorldHeight { get; set; }        

        /// <summary>
        /// Datetime of the moment of creation
        /// </summary>
        [Browsable(false)]
        [ProtoMember(5)]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Datetime of the last update
        /// </summary>
        [Browsable(false)]
        [ProtoMember(6)]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Allows to comapre the realms equality
        /// </summary>
        [Browsable(false)]
        [ProtoMember(7)]
        public Md5Hash IntegrityHash { get; set; }

        /// <summary>
        /// Keep a list of the entities lookable by ConcreteId
        /// </summary>
        [Browsable(false)]
        [ProtoMember(8)]
        public Dictionary<ushort, Entity> BluePrints { get; set; }

        /// <summary>
        /// Holds Cube Profiles configuration
        /// </summary>
        [Browsable(false)]
        [ProtoMember(9)]
        public CubeProfile[] CubeProfiles { get; set; }

        /// <summary>
        /// Holds a server services list with parameters
        /// The key is a type name
        /// The value is a initializarion string
        /// 
        /// You can see strings here instead of Services itself because of need to link server library here
        /// </summary>
        [Browsable(false)]
        [ProtoMember(10)]
        public List<KeyValuePair<string,string>> Services { get; set; }

        /// <summary>
        /// Key is a set Id
        /// Value is a container
        /// </summary>
        [Browsable(false)]
        [Description("Defines the sets used to fill containers")]
        [ProtoMember(11)]
        public Dictionary<string, SlotContainer<BlueprintSlot>> ContainerSets { get; set; }

        /// <summary>
        /// Gets or sets player start inventory set name
        /// </summary>
        [Description("Defines the start inventory set of the player")]
        [TypeConverter(typeof(ContainerSetSelector))]
        [ProtoMember(12)]
        public string StartSet { get; set; }

        #endregion

        protected WorldConfiguration(EntityFactory factory = null, bool withHelperAssignation = false)
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
            if (File.Exists(path))
                File.Delete(path);

            using (var fs = new GZipStream(File.OpenWrite(path), CompressionMode.Compress))
            {
                var writer = new BinaryWriter(fs);
                writer.Write(FormatMagic);
                writer.Write(ConfigType);

                Serializer.Serialize(fs, this);
            }
        }
        
        public static WorldConfiguration LoadFromFile(string path, EntityFactory factory = null, bool withHelperAssignation = false)
        {
            using (var fs = new GZipStream(File.OpenRead(path), CompressionMode.Decompress))
            {
                var reader = new BinaryReader(fs);

                if (reader.ReadString() != FormatMagic)
                {
                    throw new InvalidDataException("Realm file is in wrong format");
                }

                var typeId = reader.ReadInt32();
                Type type;
                switch (typeId)
                {
                    case 1:
                        type = typeof(UtopiaWorldConfiguration);
                        break;
                    default:
                        throw new FormatException("unable to load such configuration type, update your game");
                }

                return (WorldConfiguration)RuntimeTypeModel.Default.Deserialize(fs, null, type);
            }
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

        public IEnumerable<CubeProfile> GetAllCubesProfiles()
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
            BluePrints = new Dictionary<ushort, Entity>();
            CubeProfiles = new CubeProfile[0];
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
            //Generate a new Blueprint ID, it will represent this Blue print, and must be unique
            ushort newId;
            if (BluePrints.Count == 0) 
                // 1-255 cube resource values (0 - air)
                newId = 256;
            else
                newId = (ushort)(BluePrints.Values.Select(x => x.BluePrintId).Max() + 1);

            entityInstance.BluePrintId = newId;
            entityInstance.isSystemEntity = false;

            BluePrints.Add(newId, (Entity)entityInstance);
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
                CubeProfiles[currentCubeId] = new CubeProfile { Name = "System Reserved", Id = currentCubeId };
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
