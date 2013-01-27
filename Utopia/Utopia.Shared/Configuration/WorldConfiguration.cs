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

        public abstract WorldProcessors ConfigType { get; }

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
        [ProtoMember(9, OverwriteList = true)]
        public BlockProfile[] BlockProfiles { get; set; }

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

        /// <summary>
        /// Gets or sets list of all possible recipes
        /// </summary>
        [ProtoMember(13)]
        public List<Recipe> Recipes { get; set; }

        #endregion

        protected WorldConfiguration(EntityFactory factory = null, bool withHelperAssignation = false)
        {
            if (factory == null) 
                factory = new EntityFactory();
            if (withHelperAssignation) 
                EditorConfigHelper.Config = this;
            
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
                writer.Write((int)ConfigType);

                Serializer.Serialize(fs, this);
            }
        }
        
        public static WorldConfiguration LoadFromFile(string path, bool withHelperAssignation = false)
        {
            using (var fs = new GZipStream(File.OpenRead(path), CompressionMode.Decompress))
            {
                var reader = new BinaryReader(fs);

                if (reader.ReadString() != FormatMagic)
                {
                    throw new InvalidDataException("Realm file is in wrong format");
                }

                WorldProcessors processorType = (WorldProcessors)reader.ReadInt32();
                Type type;
                switch (processorType)
                {
                    case WorldProcessors.Utopia:
                        type = typeof(UtopiaWorldConfiguration);
                        break;
                    case WorldProcessors.Flat:
                        type = typeof(FlatWorldConfiguration);
                        break;
                    default:
                        throw new FormatException("unable to load such configuration type, update your game");
                }

                WorldConfiguration configuration = (WorldConfiguration)RuntimeTypeModel.Default.Deserialize(fs, null, type);
                if (withHelperAssignation)
                {
                    EditorConfigHelper.Config = configuration;
                }

                return configuration;
            }
        }
        #endregion

        public object CreateNewCube()
        {
            //Get New Cube ID.
            //We keep the id from 0 to 99 for "System" cubes
            //101 to 254 for Custom created cubes
            byte newProfileId;
            if (BlockProfiles.Where(x => x != null).Count(x => x.Id > 99) >= 1)
            {
                newProfileId = (byte)(BlockProfiles.Where(x => x.Id > 99).Select(y => y.Id).Max() + 1);
            }
            else newProfileId = 100;

            BlockProfile newCubeProfile = new BlockProfile()
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

            if (BlockProfiles.Length <= newProfileId)
            {
                var array = BlockProfiles;
                Array.Resize(ref array, newProfileId + 1);
                BlockProfiles = array;
            }
            BlockProfiles[newProfileId] = newCubeProfile;

            return newCubeProfile;
        }

        public bool DeleteBlockProfile(BlockProfile profile)
        {
            //Get cube profile id
            int profileID = 0;
            while(BlockProfiles[profileID] != profile) profileID++;
            if (profileID <= 99) return false; //Don't remove system cube
            //New Array
            BlockProfile[] newArray = new BlockProfile[255];

            //Copy until the ID from old to new
            int i;
            for (i = 0; i < profileID; i++) newArray[i] = BlockProfiles[i];
            //Copy the Old array to the new one skipping the removed Block
            for (; i < BlockProfiles.Length - 1; i++) newArray[i] = BlockProfiles[i + 1];
            BlockProfiles = newArray;
            return true;
        }

        public object CreateNewEntity(Type entityClassType)
        {
            //Create a new EntityClass object
            IEntity instance = (IEntity)Activator.CreateInstance(entityClassType);
            AddNewEntity(instance);
            return instance;
        }

        public IEnumerable<BlockProfile> GetAllCubesProfiles()
        {
            foreach (var profile in BlockProfiles.Where(x => x != null && x.Name != "System Reserved"))
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
            BlockProfiles = new BlockProfile[255];
            Services = new List<KeyValuePair<string, string>>();
            ContainerSets = new Dictionary<string, SlotContainer<BlueprintSlot>>();
            Recipes = new List<Recipe>();
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
            for (byte currentCubeId = (byte)(BlockProfiles.Where(x => x != null && x.Id < 100).Max(x => x.Id) + 1); currentCubeId < 100; currentCubeId++)
            {
                BlockProfiles[currentCubeId] = new BlockProfile { Name = "System Reserved", Id = currentCubeId };
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
        public enum WorldProcessors : int
        {
            Utopia = 1,
            Flat = 2
        }
        #endregion
    }
}
