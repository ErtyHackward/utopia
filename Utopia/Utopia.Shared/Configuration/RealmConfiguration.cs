using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Settings;
using Utopia.Shared.Structs;
using Utopia.Shared.World.Processors.Utopia.Biomes;
using System.Linq;
using S33M3Resources.Structs;
using Utopia.Shared.World.Processors.Utopia;
using Utopia.Shared.Entities.Concrete.Collectible;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Configuration
{
    /// <summary>
    /// Contains all gameplay parameters of the realm
    /// Holds possible entities types, their names, world generator settings, defines everything
    /// Allows to save and load the realm configuration
    /// </summary>
    public class RealmConfiguration : IBinaryStorable
    {
        #region Private Variables
        private readonly EntityFactory _factory;
        private int _worldHeight;
        /// <summary>
        /// Realm format version
        /// </summary>
        private const int RealmFormat = 1;
        #endregion

        #region Public Properties
        /// <summary>
        /// General realm display name
        /// </summary>
        public string ConfigurationName { get; set; }

        /// <summary>
        /// Author name of the realm
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// World Height
        /// </summary>
        public int WorldHeight
        {
            get { return _worldHeight; }
            set
            {
                if (value >= 128 && value <= 256)
                {
                    if(WorldProcessor != WorldProcessors.Utopia || (UtopiaProcessorParam != null && UtopiaProcessorParam.WorldGeneratedHeight <= value))
                    _worldHeight = value;
                }
            }
        }

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
        /// Defines realm world processor
        /// </summary>
        public WorldProcessors WorldProcessor { get; set; }

        /// <summary>
        /// Holds examples of entities of all types in the realm
        /// </summary>
        [Browsable(false)]
        public static List<IEntity> Entities { get; set; }

        [Browsable(false)]
        public List<IEntity> RealmEntities
        {
            get { return RealmConfiguration.Entities; }
            set { RealmConfiguration.Entities = value; }
        }

        /// <summary>
        /// Keep a list of the entities lookable by ConcreteId
        /// </summary>
        [Browsable(false)]
        public static Dictionary<ushort, Entity> BluePrints { get; set; }
        /// <summary>
        /// Holds Cube Profiles configuration
        /// </summary>
        [Browsable(false)]
        public static CubeProfile[] CubeProfiles { get; set; }

        [Browsable(false)]
        public CubeProfile[] RealmCubeProfiles
        {
            get { return RealmConfiguration.CubeProfiles; }
            set { RealmConfiguration.CubeProfiles = value; }
        }

        /// <summary>
        /// Holds Biomes Profiles configuration
        /// </summary>
        [Browsable(false)]
        public static List<Biome> Biomes { get; set; }

        [Browsable(false)]
        public List<Biome> RealmBiomes
        {
            get { return RealmConfiguration.Biomes; }
            set { RealmConfiguration.Biomes = value; }
        }

        /// <summary>
        /// Hold Params for Utopia processor
        /// </summary>
        [Browsable(false)]
        public UtopiaProcessorParams UtopiaProcessorParam { get; set; }

        #endregion

        public RealmConfiguration(EntityFactory factory = null, bool withDefaultValueCreation = false)
        {
            if (factory == null)
                factory = new EntityFactory(null);

            _factory = factory;
            Entities = new List<IEntity>();
            BluePrints = new Dictionary<ushort, Entity>();
            CubeProfiles = new CubeProfile[255];
            Biomes = new List<Biome>();
            UtopiaProcessorParam = new UtopiaProcessorParams();
            WorldProcessor = WorldProcessors.Utopia;

            if (withDefaultValueCreation)
            {
                CreateDefaultValues();
            }
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

        public void Save(BinaryWriter writer)
        {
            writer.Write(RealmFormat);

            writer.Write(ConfigurationName ?? string.Empty);
            writer.Write(Author ?? string.Empty);
            writer.Write(CreatedAt.ToBinary());
            writer.Write(UpdatedAt.ToBinary());
            writer.Write((byte)WorldProcessor);
            writer.Write(WorldHeight);

            writer.Write(Entities.Count);
            foreach (IEntity entitySample in Entities)
            {
                entitySample.Save(writer);
            }

            writer.Write(CubeProfiles.Where(x => x != null && x.Name != "System Reserved").Count());
            foreach (CubeProfile cubeProfile in CubeProfiles.Where(x => x != null && x.Name != "System Reserved"))
            {
                cubeProfile.Save(writer);
            }

            writer.Write(Biomes.Count);
            foreach (Biome biome in Biomes)
            {
                biome.Save(writer);
            }

            UtopiaProcessorParam.Save(writer);
        }

        public void Load(BinaryReader reader)
        {
            var currentFormat = reader.ReadInt32();
            if (currentFormat != RealmFormat)
                throw new InvalidDataException("Unsupported realm config format, expected " + RealmFormat + " current " + currentFormat);

            ConfigurationName = reader.ReadString();
            Author = reader.ReadString();
            CreatedAt = DateTime.FromBinary(reader.ReadInt64());
            UpdatedAt = DateTime.FromBinary(reader.ReadInt64());
            WorldProcessor = (WorldProcessors)reader.ReadByte();
            WorldHeight = reader.ReadInt32();

            Entities.Clear();
            BluePrints.Clear();
            int countEntity = reader.ReadInt32();
            for (var i = 0; i < countEntity; i++)
            {
                Entity entity = _factory.CreateFromBytes(reader);
                Entities.Add(entity);
                BluePrints.Add(entity.BluePrintId, entity);
            }

            CubeProfiles = new CubeProfile[255];
            var countCubes = reader.ReadInt32();
            for (var i = 0; i < countCubes; i++)
            {
                CubeProfile cp = new CubeProfile();
                cp.Load(reader);
                CubeProfiles[i] = cp;
            }
            FilledUpReservedCubeInArray();

            var countBiomes = reader.ReadInt32();
            for (var i = 0; i < countBiomes; i++)
            {
                Biome biome = new Biome();
                biome.Load(reader);
                Biomes.Add(biome);
            }

            UtopiaProcessorParam.Load(reader);

            RealmCubeProfiles = CubeProfiles;
        }

        public static RealmConfiguration LoadFromFile(string path, EntityFactory factory = null)
        {
            var configuration = new RealmConfiguration(factory ?? new EntityFactory(null));
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

            //Generate a new Entity ID, it will represent this Blue print, and must be unique
            ushort newId;
            if (RealmEntities.Count == 0) newId = 1;
            else newId = (ushort)(RealmEntities.Select(x => x.BluePrintId).Max(y => y) + 1);

            instance.BluePrintId = newId;
            instance.isSystemEntity = false;

            RealmEntities.Add(instance);

            return instance;
        }

        public Biome CreateNewBiome()
        {
            Biome newBiome = new Biome()
            {
                Name = "Default",
                SurfaceCube = RealmConfiguration.CubeId.Grass,
                UnderSurfaceCube = RealmConfiguration.CubeId.Dirt,
                GroundCube = RealmConfiguration.CubeId.Stone,
                CubeVeins = new List<CubeVein>()
                {
                    new CubeVein(){ Name = "Sand Vein", CubeId = RealmConfiguration.CubeId.Sand, VeinSize = 12, VeinPerChunk = 8, SpawningHeight = new RangeB(40,128) },
                    new CubeVein(){ Name = "Rock Vein",CubeId = RealmConfiguration.CubeId.Rock, VeinSize = 8, VeinPerChunk = 8, SpawningHeight = new RangeB(1,50) },
                    new CubeVein(){ Name = "Dirt Vein",CubeId = RealmConfiguration.CubeId.Dirt, VeinSize = 12, VeinPerChunk = 16, SpawningHeight = new RangeB(1,128) },
                    new CubeVein(){ Name = "Gravel Vein",CubeId = RealmConfiguration.CubeId.Gravel, VeinSize = 16, VeinPerChunk = 5, SpawningHeight = new RangeB(40,128) },
                    new CubeVein(){ Name = "GoldOre Vein",CubeId = RealmConfiguration.CubeId.GoldOre, VeinSize = 8, VeinPerChunk = 5, SpawningHeight = new RangeB(1,40) },
                    new CubeVein(){ Name = "CoalOre Vein",CubeId = RealmConfiguration.CubeId.CoalOre, VeinSize = 16, VeinPerChunk = 16, SpawningHeight = new RangeB(1,80) },
                    new CubeVein(){ Name = "MoonStone Vein",CubeId = RealmConfiguration.CubeId.MoonStone, VeinSize = 4, VeinPerChunk = 3, SpawningHeight = new RangeB(1,20) },
                    new CubeVein(){ Name = "DynamicWater",CubeId = RealmConfiguration.CubeId.DynamicWater, VeinSize = 5, VeinPerChunk = 20, SpawningHeight = new RangeB(60,120) },
                    new CubeVein(){ Name = "DynamicLava",CubeId = RealmConfiguration.CubeId.DynamicLava, VeinSize = 5, VeinPerChunk = 40, SpawningHeight = new RangeB(2,60) }
                }
            };

            Biomes.Add(newBiome);
            return newBiome;
        }

        #endregion

        #region Private Methods

        private void CreateDefaultValues()
        {
            //These are mandatory configuration !!
            _worldHeight = 128;
            CreateDefaultCubeProfiles();
            CreateDefaultEntities();
            CreateDefaultBiomes();
            CreateDefaultUtopiaProcessorParam();
        }

        //Definition of default cube profile
        private void CreateDefaultCubeProfiles()
        {
            int id = 0;
            //Air Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Air",
                Description = "A cube",
                Id = 0,
                Tex_Top = 255,
                Tex_Bottom = 255,
                Tex_Back = 255,
                Tex_Front = 255,
                Tex_Left = 255,
                Tex_Right = 255,
                IsSeeThrough = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true
            });

            id++;

            //Stone Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Stone",
                Description = "A cube",
                Id = 1,
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
                IsSystemCube = true
            });

            id++;

            //Dirt Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Dirt",
                Description = "A cube",
                Id = 2,
                Tex_Top = 2,
                Tex_Bottom = 2,
                Tex_Back = 2,
                Tex_Front = 2,
                Tex_Left = 2,
                Tex_Right = 2,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true
            });

            id++;

            //Grass Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Grass",
                Description = "A cube",
                Id = 3,
                Tex_Top = 0,
                Tex_Bottom = 2,
                Tex_Back = 3,
                Tex_Front = 3,
                Tex_Left = 3,
                Tex_Right = 3,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true,
                BiomeColorArrayTexture = 0
            });

            id++;

            //StillWater Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "StillWater",
                Description = "A cube",
                Id = 4,
                Tex_Top = 5,
                Tex_Bottom = 5,
                Tex_Back = 5,
                Tex_Front = 5,
                Tex_Left = 5,
                Tex_Right = 5,
                LightAbsorbed = 20,
                IsSeeThrough = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Liquid,
                Friction = 0.3f,
                IsSystemCube = true,
                BiomeColorArrayTexture = 2
            });

            id++;

            //DynamicWater Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "DynamicWater",
                Description = "A cube",
                Id = 5,
                Tex_Top = 5,
                Tex_Bottom = 5,
                Tex_Back = 5,
                Tex_Front = 5,
                Tex_Left = 5,
                Tex_Right = 5,
                LightAbsorbed = 20,
                IsSeeThrough = true,
                IsBlockingWater = true,
                IsTaggable = true,
                CubeFamilly = Enums.enuCubeFamilly.Liquid,
                Friction = 0.3f,
                IsSystemCube = true,
                BiomeColorArrayTexture = 2
            });

            id++;

            //LightWhite Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "LightWhite",
                Description = "A cube",
                Id = 6,
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
                IsSystemCube = true,
                IsEmissiveColorLightSource = true,
                EmissiveColorA = 255,
                EmissiveColorR = 255,
                EmissiveColorG = 255,
                EmissiveColorB = 255
            });

            id++;

            //Rock Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Rock",
                Description = "A cube",
                Id = 7,
                Tex_Top = 6,
                Tex_Bottom = 6,
                Tex_Back = 6,
                Tex_Front = 6,
                Tex_Left = 6,
                Tex_Right = 6,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true,
                SlidingValue = 0.05f
            });

            id++;

            //Sand Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Sand",
                Description = "A cube",
                Id = 8,
                Tex_Top = 7,
                Tex_Bottom = 7,
                Tex_Back = 7,
                Tex_Front = 7,
                Tex_Left = 7,
                Tex_Right = 7,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.3f,
                IsSystemCube = true,
            });

            id++;

            //Gravel Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Gravel",
                Description = "A cube",
                Id = 9,
                Tex_Top = 8,
                Tex_Bottom = 8,
                Tex_Back = 8,
                Tex_Front = 8,
                Tex_Left = 8,
                Tex_Right = 8,
                LightAbsorbed = 255,                                
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true,
            });

            id++;

            //Trunk Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Trunk",
                Description = "A cube",
                Id = 10,
                Tex_Top = 10,
                Tex_Bottom = 10,
                Tex_Back = 9,
                Tex_Front = 9,
                Tex_Left = 9,
                Tex_Right = 9,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true,
            });

            id++;

            //GoldOre Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "GoldOre",
                Description = "A cube",
                Id = 11,
                Tex_Top = 11,
                Tex_Bottom = 11,
                Tex_Back = 11,
                Tex_Front = 11,
                Tex_Left = 11,
                Tex_Right = 11,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true,
            });

            id++;

            //CoalOre Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "CoalOre",
                Description = "A cube",
                Id = 12,
                Tex_Top = 12,
                Tex_Bottom = 12,
                Tex_Back = 12,
                Tex_Front = 12,
                Tex_Left = 12,
                Tex_Right = 12,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true,
            });

            id++;

            //MoonStone Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "MoonStone",
                Description = "A cube",
                Id = 13,
                Tex_Top = 13,
                Tex_Bottom = 13,
                Tex_Back = 13,
                Tex_Front = 13,
                Tex_Left = 13,
                Tex_Right = 13,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true,
                IsEmissiveColorLightSource = true,
                EmissiveColorA = 255,
                EmissiveColorR = 86,
                EmissiveColorG = 143,
                EmissiveColorB = 255
            });

            id++;

            //Foliage Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Foliage",
                Description = "A cube",
                Id = 14,
                Tex_Top = 15,
                Tex_Bottom = 15,
                Tex_Back = 15,
                Tex_Front = 15,
                Tex_Left = 15,
                Tex_Right = 15,
                LightAbsorbed = 255,
                IsSeeThrough = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true,
                BiomeColorArrayTexture = 1
            });

            id++;

            //Snow Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Snow",
                Description = "A cube",
                Id = 15,
                Tex_Top = 17,
                Tex_Bottom = 17,
                Tex_Back = 17,
                Tex_Front = 17,
                Tex_Left = 17,
                Tex_Right = 17,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                YBlockOffset = 0.9,
                Friction = 0.35f,
                IsSystemCube = true
            });

            id++;

            //Ice Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Ice",
                Description = "A cube",
                Id = 16,
                Tex_Top = 18,
                Tex_Bottom = 18,
                Tex_Back = 18,
                Tex_Front = 18,
                Tex_Left = 18,
                Tex_Right = 18,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                IsSystemCube = true
            });

            id++;

            //StillLava Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "StillLava",
                Description = "A cube",
                Id = 17,
                Tex_Top = 19,
                Tex_Bottom = 19,
                Tex_Back = 19,
                Tex_Front = 19,
                Tex_Left = 19,
                Tex_Right = 19,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                IsSystemCube = true,
                IsEmissiveColorLightSource = true,
                EmissiveColorA = 255,
                EmissiveColorR = 255,
                EmissiveColorG = 161,
                EmissiveColorB = 38
            });

            id++;

            //DynamicLava Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "DynamicLava",
                Description = "A cube",
                Id = 18,
                Tex_Top = 19,
                Tex_Bottom = 19,
                Tex_Back = 19,
                Tex_Front = 19,
                Tex_Left = 19,
                Tex_Right = 19,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                IsSystemCube = true,
                IsEmissiveColorLightSource = true,
                EmissiveColorA = 255,
                EmissiveColorR = 255,
                EmissiveColorG = 161,
                EmissiveColorB = 38,
                IsTaggable = true
            });

            id++;

            //Cactus Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Cactus",
                Description = "A cube",
                Id = 19,
                Tex_Top = 22,
                Tex_Bottom = 22,
                Tex_Back = 20,
                Tex_Front = 20,
                Tex_Left = 20,
                Tex_Right = 20,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                IsSystemCube = true,
                SideOffsetMultiplier = 1
            });

            id++;

            //CactusTop Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "CactusTop",
                Description = "A cube",
                Id = 20,
                Tex_Top = 21,
                Tex_Bottom = 22,
                Tex_Back = 20,
                Tex_Front = 20,
                Tex_Left = 20,
                Tex_Right = 20,
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                IsSystemCube = true,
                SideOffsetMultiplier = 1
            });

            id++;

            FilledUpReservedCubeInArray();

        }

        private void FilledUpReservedCubeInArray()
        {
            //Field up to 100 included for Reserved Cube ID
            for (byte currentCubeId = (byte)(CubeProfiles.Where(x => x != null).Max(x => x.Id) + 1); currentCubeId < 100; currentCubeId++)
            {
                CubeProfiles[currentCubeId] = new CubeProfile() { Name = "System Reserved", Id = currentCubeId };
            }
        }

        private void CreateDefaultEntities()
        {
           //Cactus Entity blue print
           Plant cactusFlower = (Plant)CreateNewEntity(typeof(Plant));
           cactusFlower.Name = "Cactus Flower";
           cactusFlower.MountPoint = BlockFace.Top;
           cactusFlower.ModelName = "Flower4";
           cactusFlower.isSystemEntity = true;      // Cannot de removed, mandatory Entity
           cactusFlower.MaxStackSize = 99;
        }

        //Definition of default biomes
        private void CreateDefaultBiomes()
        {
            //Desert Biome Definition
            Biomes.Add(new Biome()
            {
                Name = "Default",
                SurfaceCube = RealmConfiguration.CubeId.Grass,
                UnderSurfaceCube = RealmConfiguration.CubeId.Dirt,
                GroundCube = RealmConfiguration.CubeId.Stone,
                CubeVeins = new List<CubeVein>()
                {
                    new CubeVein(){ Name = "Sand Vein", CubeId = RealmConfiguration.CubeId.Sand, VeinSize = 12, VeinPerChunk = 8, SpawningHeight = new RangeB(40,128) },
                    new CubeVein(){ Name = "Rock Vein",CubeId = RealmConfiguration.CubeId.Rock, VeinSize = 8, VeinPerChunk = 8, SpawningHeight = new RangeB(1,50) },
                    new CubeVein(){ Name = "Dirt Vein",CubeId = RealmConfiguration.CubeId.Dirt, VeinSize = 12, VeinPerChunk = 16, SpawningHeight = new RangeB(1,128) },
                    new CubeVein(){ Name = "Gravel Vein",CubeId = RealmConfiguration.CubeId.Gravel, VeinSize = 16, VeinPerChunk = 5, SpawningHeight = new RangeB(40,128) },
                    new CubeVein(){ Name = "GoldOre Vein",CubeId = RealmConfiguration.CubeId.GoldOre, VeinSize = 8, VeinPerChunk = 5, SpawningHeight = new RangeB(1,40) },
                    new CubeVein(){ Name = "CoalOre Vein",CubeId = RealmConfiguration.CubeId.CoalOre, VeinSize = 16, VeinPerChunk = 16, SpawningHeight = new RangeB(1,80) },
                    new CubeVein(){ Name = "MoonStone Vein",CubeId = RealmConfiguration.CubeId.MoonStone, VeinSize = 4, VeinPerChunk = 3, SpawningHeight = new RangeB(1,20) },
                    new CubeVein(){ Name = "DynamicWater",CubeId = RealmConfiguration.CubeId.DynamicWater, VeinSize = 5, VeinPerChunk = 20, SpawningHeight = new RangeB(60,120) },
                    new CubeVein(){ Name = "DynamicLava",CubeId = RealmConfiguration.CubeId.DynamicLava, VeinSize = 5, VeinPerChunk = 40, SpawningHeight = new RangeB(2,60) }
                }
            });
        }

        //Definition of all default Utopia processor params
        private void CreateDefaultUtopiaProcessorParam()
        {
            UtopiaProcessorParam.CreateDefaultConfiguration();
        }

        #endregion

        #region Inner Classes
        //Helper inner class, to quickly get the corresponding static cube ID (These cannot be modified by users), they are "system" blocks
        public static class CubeId
        {
            public const byte Air = 0;
            public const byte Stone = 1;
            public const byte Dirt = 2;
            public const byte Grass = 3;
            public const byte StillWater = 4;
            public const byte DynamicWater = 5;
            public const byte LightWhite = 6;
            public const byte Rock = 7;
            public const byte Sand = 8;
            public const byte Gravel = 9;
            public const byte Trunk = 10;
            public const byte GoldOre = 11;
            public const byte CoalOre = 12;
            public const byte MoonStone = 13;
            public const byte Foliage = 14;
            public const byte Snow = 15;
            public const byte Ice = 16;
            public const byte StillLava = 17;
            public const byte DynamicLava = 18;
            public const byte Cactus = 19;
            public const byte CactusTop = 20;
            public const byte Error = 255;

            public static IEnumerable<byte> All()
            {
                foreach (var profile in CubeProfiles.Where(x => x != null && x.Name != "System Reserved"))
                {
                    yield return profile.Id;
                }
            }
        }

        public static class BluePrintId
        {
            public const byte CactusFlower = 1;
        }

        #endregion
    }

    /// <summary>
    /// Defines world processor possible types
    /// </summary>
    public enum WorldProcessors : byte
    {
        Flat,
        Utopia,
        Plan
    }
}
