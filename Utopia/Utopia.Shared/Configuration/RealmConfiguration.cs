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
using S33M3_Resources.Structs;

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
        /// <summary>
        /// Realm format version
        /// </summary>
        private const int RealmFormat = 1;
        #endregion

        #region Public Properties
        /// <summary>
        /// General realm display name
        /// </summary>
        public string RealmName { get; set; }

        /// <summary>
        /// Author name of the realm
        /// </summary>
        public string Author { get; set; }

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
        public List<IEntity> EntityBluePrint { get; set; }

        /// <summary>
        /// Holds Cube Profiles configuration
        /// </summary>
        [Browsable(false)]
        public List<CubeProfile> CubeProfiles { get; set; }

        /// <summary>
        /// Holds Biomes Profiles configuration
        /// </summary>
        [Browsable(false)]
        public List<BiomeConfig> Biomes { get; set; }

        #endregion

        public RealmConfiguration(EntityFactory factory = null, bool withDefaultValueCreation = false)
        {
            if (factory == null)
                factory = new EntityFactory(null);

            _factory = factory;
            EntityBluePrint = new List<IEntity>();
            CubeProfiles = new List<CubeProfile>();
            Biomes = new List<BiomeConfig>();

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

            writer.Write(RealmName ?? string.Empty);
            writer.Write(Author ?? string.Empty);
            writer.Write(CreatedAt.ToBinary());
            writer.Write(UpdatedAt.ToBinary());
            writer.Write((byte)WorldProcessor);

            writer.Write(EntityBluePrint.Count);
            foreach (IEntity entitySample in EntityBluePrint)
            {
                entitySample.Save(writer);
            }

            writer.Write(CubeProfiles.Count);
            foreach (CubeProfile cubeProfile in CubeProfiles.Where(x => x.Name != "System Reserved"))
            {
                cubeProfile.Save(writer);
            }

            writer.Write(Biomes.Count);
            foreach (Biome biome in Biomes)
            {
                biome.Save(writer);
            }
        }

        public void Load(BinaryReader reader)
        {
            var currentFormat = reader.ReadInt32();
            if (currentFormat != RealmFormat)
                throw new InvalidDataException("Unsupported realm config format, expected " + RealmFormat + " current " + currentFormat);

            RealmName = reader.ReadString();
            Author = reader.ReadString();
            CreatedAt = DateTime.FromBinary(reader.ReadInt64());
            UpdatedAt = DateTime.FromBinary(reader.ReadInt64());
            WorldProcessor = (WorldProcessors)reader.ReadByte();

            EntityBluePrint.Clear();
            int countEntity = reader.ReadInt32();
            for (var i = 0; i < countEntity; i++)
            {
                EntityBluePrint.Add(_factory.CreateFromBytes(reader));
            }

            CubeProfiles.Clear();
            var countCubes = reader.ReadInt32();
            for (var i = 0; i < countCubes; i++)
            {
                CubeProfile cp = new CubeProfile();
                cp.Load(reader);
                CubeProfiles.Add(cp);
            }
            FilledUpReservedCubeInArray();

            var countBiomes = reader.ReadInt32();
            for (var i = 0; i < countBiomes; i++)
            {
                BiomeConfig bio = new BiomeConfig(CubeProfiles);
                bio.Load(reader);
                Biomes.Add(bio);
            }
        }

        public static RealmConfiguration LoadFromFile(string path, EntityFactory factory = null)
        {
            var voxelModel = new RealmConfiguration(factory ?? new EntityFactory(null));
            using (var fs = new GZipStream(File.OpenRead(path), CompressionMode.Decompress))
            {
                var reader = new BinaryReader(fs);
                voxelModel.Load(reader);
            }
            return voxelModel;
        }
        #endregion

        public void CreateNewCube()
        {
            //Get New Cube ID.
            //We keep the id from 0 to 100 for "System" cubes
            //101 to 254 for Custom created cubes
            byte newProfileId;
            if (CubeProfiles.Count(x => x.Id > 100) > 1)
            {
                newProfileId = CubeProfiles.Where(x => x.Id > 100).Select(y => y.Id).Max();
            }
            else newProfileId = 101;

            CubeProfiles.Add(new CubeProfile()
            {
                Name = "NewCustomCube",
                Id = newProfileId,
                Tex_Top = 1,
                Tex_Bottom = 1,
                Tex_Back = 1,
                Tex_Front = 1,
                Tex_Left = 1,
                Tex_Right = 1,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = true
            });

        }

        public void CreateNewBiome()
        {
        }

        #endregion

        #region Private Methods

        private void CreateDefaultValues()
        {
            CreateDefaultCubeProfiles();
            CreateDefaultBiomes();
        }

        //Definition of default cube profile
        private void CreateDefaultCubeProfiles()
        {
            //Air Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Air",
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
                CanBeModified = false
            });

            //Stone Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Stone",
                Id = 1,
                Tex_Top = 1,
                Tex_Bottom = 1,
                Tex_Back = 1,
                Tex_Front = 1,
                Tex_Left = 1,
                Tex_Right = 1,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = false
            });

            //Dirt Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Dirt",
                Id = 2,
                Tex_Top = 2,
                Tex_Bottom = 2,
                Tex_Back = 2,
                Tex_Front = 2,
                Tex_Left = 2,
                Tex_Right = 2,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = false
            });

            //Grass Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Grass",
                Id = 3,
                Tex_Top = 0,
                Tex_Bottom = 2,
                Tex_Back = 3,
                Tex_Front = 3,
                Tex_Left = 3,
                Tex_Right = 3,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = false,
                BiomeColorArrayTexture = 0
            });

            //WoodPlank Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "WoodPlank",
                Id = 4,
                Tex_Top = 4,
                Tex_Bottom = 4,
                Tex_Back = 4,
                Tex_Front = 4,
                Tex_Left = 4,
                Tex_Right = 4,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = true
            });

            //StillWater Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "StillWater",
                Id = 5,
                Tex_Top = 5,
                Tex_Bottom = 5,
                Tex_Back = 5,
                Tex_Front = 5,
                Tex_Left = 5,
                Tex_Right = 5,
                IsSeeThrough = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Liquid,
                Friction = 0.3f,
                CanBeModified = false,
                BiomeColorArrayTexture = 2
            });

            //DynamicWater Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "DynamicWater",
                Id = 6,
                Tex_Top = 5,
                Tex_Bottom = 5,
                Tex_Back = 5,
                Tex_Front = 5,
                Tex_Left = 5,
                Tex_Right = 5,
                IsSeeThrough = true,
                IsBlockingWater = true,
                IsTaggable = true,
                CubeFamilly = Enums.enuCubeFamilly.Liquid,
                Friction = 0.3f,
                CanBeModified = false,
                BiomeColorArrayTexture = 2
            });

            //LightWhite Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "LightWhite",
                Id = 7,
                Tex_Top = 1,
                Tex_Bottom = 1,
                Tex_Back = 1,
                Tex_Front = 1,
                Tex_Left = 1,
                Tex_Right = 1,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = true,
                IsEmissiveColorLightSource = true,
                EmissiveColorA = 255,
                EmissiveColorR = 255,
                EmissiveColorG = 255,
                EmissiveColorB = 255
            });

            //Rock Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Rock",
                Id = 8,
                Tex_Top = 6,
                Tex_Bottom = 6,
                Tex_Back = 6,
                Tex_Front = 6,
                Tex_Left = 6,
                Tex_Right = 6,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = false,
                SlidingValue = 0.05f
            });

            //Sand Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Sand",
                Id = 9,
                Tex_Top = 7,
                Tex_Bottom = 7,
                Tex_Back = 7,
                Tex_Front = 7,
                Tex_Left = 7,
                Tex_Right = 7,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.3f,
                CanBeModified = false,
            });

            //Gravel Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Gravel",
                Id = 10,
                Tex_Top = 8,
                Tex_Bottom = 8,
                Tex_Back = 8,
                Tex_Front = 8,
                Tex_Left = 8,
                Tex_Right = 8,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = false,
            });

            //Trunk Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Trunk",
                Id = 11,
                Tex_Top = 10,
                Tex_Bottom = 10,
                Tex_Back = 9,
                Tex_Front = 9,
                Tex_Left = 9,
                Tex_Right = 9,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = false,
            });

            //GoldOre Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "GoldOre",
                Id = 12,
                Tex_Top = 11,
                Tex_Bottom = 11,
                Tex_Back = 11,
                Tex_Front = 11,
                Tex_Left = 11,
                Tex_Right = 11,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = false,
            });

            //CoalOre Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "CoalOre",
                Id = 13,
                Tex_Top = 12,
                Tex_Bottom = 12,
                Tex_Back = 12,
                Tex_Front = 12,
                Tex_Left = 12,
                Tex_Right = 12,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = false,
            });

            //MoonStone Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "MoonStone",
                Id = 14,
                Tex_Top = 13,
                Tex_Bottom = 13,
                Tex_Back = 13,
                Tex_Front = 13,
                Tex_Left = 13,
                Tex_Right = 13,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = false,
                IsEmissiveColorLightSource = true,
                EmissiveColorA = 255,
                EmissiveColorR = 86,
                EmissiveColorG = 143,
                EmissiveColorB = 255
            });

            //Brick Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Brick",
                Id = 15,
                Tex_Top = 14,
                Tex_Bottom = 14,
                Tex_Back = 14,
                Tex_Front = 14,
                Tex_Left = 14,
                Tex_Right = 14,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = true,
            });

            //Foliage Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Foliage",
                Id = 16,
                Tex_Top = 15,
                Tex_Bottom = 15,
                Tex_Back = 15,
                Tex_Front = 15,
                Tex_Left = 15,
                Tex_Right = 15,
                IsBlockingLight = true,
                IsSeeThrough = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = false,
                BiomeColorArrayTexture = 1
            });

            //Glass Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Glass",
                Id = 17,
                Tex_Top = 16,
                Tex_Bottom = 16,
                Tex_Back = 16,
                Tex_Front = 16,
                Tex_Left = 16,
                Tex_Right = 16,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                CanBeModified = true
            });

            //Snow Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Snow",
                Id = 18,
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
                CanBeModified = false
            });

            //Ice Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Ice",
                Id = 19,
                Tex_Top = 18,
                Tex_Bottom = 18,
                Tex_Back = 18,
                Tex_Front = 18,
                Tex_Left = 18,
                Tex_Right = 18,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                CanBeModified = false
            });

            //StillLava Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "StillLava",
                Id = 20,
                Tex_Top = 19,
                Tex_Bottom = 19,
                Tex_Back = 19,
                Tex_Front = 19,
                Tex_Left = 19,
                Tex_Right = 19,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                CanBeModified = false,
                IsEmissiveColorLightSource = true,
                EmissiveColorA = 255,
                EmissiveColorR = 255,
                EmissiveColorG = 161,
                EmissiveColorB = 38
            });

            //DynamicLava Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "DynamicLava",
                Id = 21,
                Tex_Top = 19,
                Tex_Bottom = 19,
                Tex_Back = 19,
                Tex_Front = 19,
                Tex_Left = 19,
                Tex_Right = 19,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                CanBeModified = false,
                IsEmissiveColorLightSource = true,
                EmissiveColorA = 255,
                EmissiveColorR = 255,
                EmissiveColorG = 161,
                EmissiveColorB = 38,
                IsTaggable = true
            });


            //Cactus Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "Cactus",
                Id = 22,
                Tex_Top = 22,
                Tex_Bottom = 22,
                Tex_Back = 20,
                Tex_Front = 20,
                Tex_Left = 20,
                Tex_Right = 20,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                CanBeModified = false,
                SideOffsetMultiplier = 1
            });

            //CactusTop Block
            CubeProfiles.Add(new CubeProfile()
            {
                Name = "CactusTop",
                Id = 23,
                Tex_Top = 21,
                Tex_Bottom = 22,
                Tex_Back = 20,
                Tex_Front = 20,
                Tex_Left = 20,
                Tex_Right = 20,
                IsBlockingLight = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                CanBeModified = false,
                SideOffsetMultiplier = 1
            });

            FilledUpReservedCubeInArray();

        }

        private void FilledUpReservedCubeInArray()
        {
            //Field up to 100 included for Reserved Cube ID
            for (byte currentCubeId = (byte)(CubeProfiles.Max(x => x.Id) + 1); currentCubeId < 100; currentCubeId++)
            {
                CubeProfiles.Add(new CubeProfile() { Name = "System Reserved", Id = currentCubeId });
            }
        }

        //Definition of default biomes
        private void CreateDefaultBiomes()
        {
            //Desert Biome Definition
            Biomes.Add(new BiomeConfig(CubeProfiles)
            {
                Name = "Desert",
                SurfaceCube = CubeId.Sand,
                UnderSurfaceCube = CubeId.Sand,
                UnderSurfaceLayers = new RangeI(1, 3),  // = The layer under the surface is 1 to 3 block height, then after you have the ground Cubes
                GroundCube = CubeId.Stone
            });
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
            public const byte StillWater = 5;
            public const byte DynamicWater = 6;
            public const byte Rock = 8;
            public const byte Sand = 9;
            public const byte Gravel = 10;
            public const byte Trunk = 11;
            public const byte GoldOre = 12;
            public const byte CoalOre = 13;
            public const byte MoonStone = 14;
            public const byte Foliage = 16;
            public const byte Snow = 18;
            public const byte Ice = 19;
            public const byte StillLava = 20;
            public const byte DynamicLava = 21;
            public const byte Cactus = 22;
            public const byte CactusTop = 23;
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
