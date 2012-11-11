using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Concrete.Collectible;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Settings;
using Utopia.Shared.Tools.BinarySerializer;
using Utopia.Shared.World.Processors.Utopia;
using Utopia.Shared.World.Processors.Utopia.Biomes;
using Utopia.Shared.World.Processors.Utopia.LandformFct;

namespace Utopia.Shared.Configuration
{
    /// <summary>
    /// Class that will hold the various parameters needed for landscape generation by the processor Utopia
    /// </summary>
    public class UtopiaProcessorParams : IBinaryStorable, IProcessorParams
    {
        public static class DefaultConfig
        {
            public const int BasicPlain_Flat = 0;
            public const int BasicPlain_Plain = 1;
            public const int BasicPlain_Hill = 2;
            public const int BasicMidLand_Midland = 0;
            public const int BasicMontain_Montain = 0;
            public const int BasicOcean_Ocean = 0;

            public const int Ground_BasicPlain = 0;
            public const int Ground_BasicMidLand = 1;
            public const int Ground_BasicMontain = 2;

            public const int Ocean_BasicOcean = 0;
            
            public const int World_Ocean = 0;
            public const int World_Ground = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;          
            if (handler != null)                                            
            {
                handler(this, new PropertyChangedEventArgs(propertyName));  
            }
        }

        #region Private Variables
        public WorldConfiguration Config { get; set; }
        #endregion

        #region Public Properties
        /// <summary>
        /// Holds Biomes Profiles configuration
        /// </summary>
        public List<Biome> Biomes { get; set; }

        public List<LandscapeRange> BasicPlain { get; set; }
        public List<LandscapeRange> BasicMidland { get; set; }
        public List<LandscapeRange> BasicMontain { get; set; }
        public List<LandscapeRange> BasicOcean { get; set; }
        public List<LandscapeRange> Ground { get; set; }
        public List<LandscapeRange> Ocean { get; set; }
        public List<LandscapeRange> World { get; set; }
        public enuWorldType WorldType { get; set; }

        public int WorldGeneratedHeight { get; set; }
        public int WaterLevel  { get; set; }

        public double PlainCtrlFrequency { get; set; }
        public int PlainCtrlOctave { get; set; }
        public double GroundCtrlFrequency { get; set; }
        public int GroundCtrlOctave { get; set; }
        public double WorldCtrlFrequency  { get; set; }
        public int WorldCtrlOctave { get; set; }
        public double IslandCtrlSize { get; set; }

        public double TempCtrlFrequency { get; set; }
        public int TempCtrlOctave { get; set; }

        public double MoistureCtrlFrequency { get; set; }
        public int MoistureCtrlOctave { get; set; }

        #endregion

        public UtopiaProcessorParams()
        {
            Initialize();
        }

        #region Public Methods
        private void Initialize()
        {
            BasicPlain = new List<LandscapeRange>();
            BasicMidland = new List<LandscapeRange>();
            BasicMontain = new List<LandscapeRange>();
            BasicOcean = new List<LandscapeRange>();
            Ground = new List<LandscapeRange>();
            Ocean = new List<LandscapeRange>();
            World = new List<LandscapeRange>();

            Biomes = new List<Biome>();

            WorldType = enuWorldType.Normal;
            WorldGeneratedHeight = 128;
            WaterLevel = 64;

            PlainCtrlFrequency = 2.5;
            PlainCtrlOctave = 3;

            GroundCtrlFrequency = 1.5;
            GroundCtrlOctave = 2;

            WorldCtrlFrequency = 1.5;
            WorldCtrlOctave = 3;

            IslandCtrlSize = 0.7;

            TempCtrlFrequency = 1;
            TempCtrlOctave = 2;

            MoistureCtrlFrequency = 1;
            MoistureCtrlOctave = 2;

            ClearAllinternalCollections();
            InitializeLandscapeComponent();
            InjectDefaultBiomes();
        }


        public Biome CreateNewBiome()
        {
            Biome newBiome = new Biome(Config)
            {
                Name = "Default",
                SurfaceCube = UtopiaProcessorParams.CubeId.Grass,
                UnderSurfaceCube = UtopiaProcessorParams.CubeId.Dirt,
                GroundCube = UtopiaProcessorParams.CubeId.Stone,
                CubeVeins = new List<CubeVein>()
                {
                    new CubeVein(){ Name = "Sand Vein", CubeId = UtopiaProcessorParams.CubeId.Sand, VeinSize = 12, VeinPerChunk = 8, SpawningHeight = new RangeB(40,128) },
                    new CubeVein(){ Name = "Rock Vein",CubeId = UtopiaProcessorParams.CubeId.Rock, VeinSize = 8, VeinPerChunk = 8, SpawningHeight = new RangeB(1,50) },
                    new CubeVein(){ Name = "Dirt Vein",CubeId = UtopiaProcessorParams.CubeId.Dirt, VeinSize = 12, VeinPerChunk = 16, SpawningHeight = new RangeB(1,128) },
                    new CubeVein(){ Name = "Gravel Vein",CubeId = UtopiaProcessorParams.CubeId.Gravel, VeinSize = 16, VeinPerChunk = 5, SpawningHeight = new RangeB(40,128) },
                    new CubeVein(){ Name = "GoldOre Vein",CubeId = UtopiaProcessorParams.CubeId.GoldOre, VeinSize = 8, VeinPerChunk = 5, SpawningHeight = new RangeB(1,40) },
                    new CubeVein(){ Name = "CoalOre Vein",CubeId = UtopiaProcessorParams.CubeId.CoalOre, VeinSize = 16, VeinPerChunk = 16, SpawningHeight = new RangeB(1,80) },
                    new CubeVein(){ Name = "MoonStone Vein",CubeId = UtopiaProcessorParams.CubeId.MoonStone, VeinSize = 4, VeinPerChunk = 3, SpawningHeight = new RangeB(1,20) },
                    new CubeVein(){ Name = "DynamicWater",CubeId = UtopiaProcessorParams.CubeId.DynamicWater, VeinSize = 5, VeinPerChunk = 20, SpawningHeight = new RangeB(60,120) },
                    new CubeVein(){ Name = "DynamicLava",CubeId = UtopiaProcessorParams.CubeId.DynamicLava, VeinSize = 5, VeinPerChunk = 40, SpawningHeight = new RangeB(2,60) }
                }
            };

            Biomes.Add(newBiome);
            return newBiome;
        }

        public IEnumerable<CubeProfile> InjectDefaultCubeProfiles()
        {
            //Stone Block
            yield return (new CubeProfile()
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

            //Dirt Block
            yield return (new CubeProfile()
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

            //Grass Block
            yield return (new CubeProfile()
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

            //StillWater Block
            yield return (new CubeProfile()
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

            //DynamicWater Block
            yield return (new CubeProfile()
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

            //LightWhite Block
            yield return (new CubeProfile()
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

            //Rock Block
            yield return (new CubeProfile()
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

            //Sand Block
            yield return (new CubeProfile()
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

            //Gravel Block
            yield return (new CubeProfile()
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

            //Trunk Block
            yield return (new CubeProfile()
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

            //GoldOre Block
            yield return (new CubeProfile()
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

            //CoalOre Block
            yield return (new CubeProfile()
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

            //MoonStone Block
            yield return (new CubeProfile()
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

            //Foliage Block
            yield return (new CubeProfile()
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

            //Snow Block
            yield return (new CubeProfile()
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

            //Ice Block
            yield return (new CubeProfile()
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

            //StillLava Block
            yield return (new CubeProfile()
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

            //DynamicLava Block
            yield return (new CubeProfile()
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

            //Cactus Block
            yield return (new CubeProfile()
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

            //CactusTop Block
            yield return (new CubeProfile()
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


        }

        public IEnumerable<IEntity> InjectDefaultEntities()
        {
            Plant cactusFlower = Config.Factory.CreateEntity<Plant>();
            cactusFlower.Name = "Cactus Flower";
            cactusFlower.MountPoint = BlockFace.Top;
            cactusFlower.ModelName = "Flower4";
            cactusFlower.isSystemEntity = true;      // Cannot de removed, mandatory Entity
            cactusFlower.MaxStackSize = 99;
            yield return cactusFlower;

        }
        #endregion

        #region Private Methods
        //Definition of default biomes
        private void ClearAllinternalCollections()
        {
            Biomes.Clear();
            BasicPlain.Clear();
            BasicMidland.Clear();
            BasicMontain.Clear();
            BasicOcean.Clear();
            Ground.Clear();
            Ocean.Clear();
            World.Clear();
        }

        private void InjectDefaultBiomes()
        {
            CreateNewBiome();
        }


        private void InitializeLandscapeComponent()
        {
            //Create BasicPlain
            BasicPlain.Add(new LandscapeRange()
            {
                Name = "Flat",
                Color = Color.AliceBlue,
                Size = 0.2,
                MixedNextArea = 0.05
            });
            BasicPlain.Add(new LandscapeRange()
            {
                Name = "Plain",
                Color = Color.YellowGreen,
                Size = 0.5,
                MixedNextArea = 0.05
            });
            BasicPlain.Add(new LandscapeRange()
            {
                Name = "Hill",
                Color = Color.Tomato,
                Size = 0.3
            });

            //Create BasicMidLand
            BasicMidland.Add(new LandscapeRange()
            {
                Name = "Midland",
                Color = Color.Wheat,
                Size = 1
            });
            //Create BasicMontain
            BasicMontain.Add(new LandscapeRange()
            {
                Name = "Montain",
                Color = Color.Brown,
                Size = 1
            });
            //Create BasicOcean
            BasicOcean.Add(new LandscapeRange()
            {
                Name = "Ocean",
                Color = Color.Navy,
                Size = 1
            });

            //Create Ground 
            Ground.Add(new LandscapeRange()
            {
                Name = "BasicPlain",
                Color = Color.Green,
                Size = 0.4,
                MixedNextArea = 0.05
            });
            Ground.Add(new LandscapeRange()
            {
                Name = "BasicMidLand",
                Color = Color.YellowGreen,
                Size = 0.3,
                MixedNextArea = 0.05
            });
            Ground.Add(new LandscapeRange()
            {
                Name = "BasicMontain",
                Color = Color.Brown,
                Size = 0.3
            });

            //Create Ocean
            Ocean.Add(new LandscapeRange()
            {
                Name = "BasicOcean",
                Color = Color.Navy,
                Size = 1
            });

            //Create World
            World.Add(new LandscapeRange()
            {
                Name = "Ocean",
                Color = Color.Navy,
                Size = 0.1,
                MixedNextArea = 0.02
            });

            World.Add(new LandscapeRange()
            {
                Name = "Ground",
                Color = Color.Gold,
                Size = 0.9
            });
        }

        #endregion

        public void Save(BinaryWriter writer)
        {
            writer.Write(Biomes.Count);
            foreach (var biome in Biomes)
            {
                biome.Save(writer);
            }

            writer.Write(BasicPlain.Count);
            for (int i = 0; i < BasicPlain.Count; i++)
            {
                BasicPlain[i].Save(writer);
            }

            writer.Write(BasicMidland.Count);
            for (int i = 0; i < BasicMidland.Count; i++)
            {
                BasicMidland[i].Save(writer);
            }

            writer.Write(BasicMontain.Count);
            for (int i = 0; i < BasicMontain.Count; i++)
            {
                BasicMontain[i].Save(writer);
            }

            writer.Write(BasicOcean.Count);
            for (int i = 0; i < BasicOcean.Count; i++)
            {
                BasicOcean[i].Save(writer);
            }

            writer.Write(Ground.Count);
            for (int i = 0; i < Ground.Count; i++)
            {
                Ground[i].Save(writer);
            }

            writer.Write(Ocean.Count);
            for (int i = 0; i < Ocean.Count; i++)
            {
                Ocean[i].Save(writer);
            }

            writer.Write(World.Count);
            for (int i = 0; i < World.Count; i++)
            {
                World[i].Save(writer);
            }

            writer.Write((int)WorldType);

            writer.Write(WorldGeneratedHeight);
            writer.Write(WaterLevel);

            writer.Write(TempCtrlFrequency);
            writer.Write(TempCtrlOctave);

            writer.Write(MoistureCtrlFrequency);
            writer.Write(MoistureCtrlOctave);

            writer.Write(PlainCtrlFrequency);
            writer.Write(PlainCtrlOctave);

            writer.Write(GroundCtrlFrequency);
            writer.Write(GroundCtrlOctave);

            writer.Write(WorldCtrlFrequency);
            writer.Write(WorldCtrlOctave);

            writer.Write(IslandCtrlSize);
        }

        public void Load(BinaryReader reader)
        {
            ClearAllinternalCollections();
            LandscapeRange landscapeRange;
            int count;

            count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var biome = new Biome(Config);
                biome.Load(reader);
                Biomes.Add(biome);
            }

            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                landscapeRange = new LandscapeRange();
                landscapeRange.Load(reader);
                BasicPlain.Add(landscapeRange);
            }

            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                landscapeRange = new LandscapeRange();
                landscapeRange.Load(reader);
                BasicMidland.Add(landscapeRange);
            }

            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                landscapeRange = new LandscapeRange();
                landscapeRange.Load(reader);
                BasicMontain.Add(landscapeRange);
            }


            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                landscapeRange = new LandscapeRange();
                landscapeRange.Load(reader);
                BasicOcean.Add(landscapeRange);
            }


            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                landscapeRange = new LandscapeRange();
                landscapeRange.Load(reader);
                Ground.Add(landscapeRange);
            }


            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                landscapeRange = new LandscapeRange();
                landscapeRange.Load(reader);
                Ocean.Add(landscapeRange);
            }

            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                landscapeRange = new LandscapeRange();
                landscapeRange.Load(reader);
                World.Add(landscapeRange);
            }
            WorldType = (enuWorldType)reader.ReadInt32();

            WorldGeneratedHeight = reader.ReadInt32();
            WaterLevel = reader.ReadInt32();

            TempCtrlFrequency = reader.ReadDouble();
            TempCtrlOctave = reader.ReadInt32();

            MoistureCtrlFrequency = reader.ReadDouble();
            MoistureCtrlOctave = reader.ReadInt32();

            PlainCtrlFrequency = reader.ReadDouble();
            PlainCtrlOctave = reader.ReadInt32();

            GroundCtrlFrequency = reader.ReadDouble();
            GroundCtrlOctave = reader.ReadInt32();

            WorldCtrlFrequency = reader.ReadDouble();
            WorldCtrlOctave = reader.ReadInt32();

            IslandCtrlSize = reader.ReadDouble();
        }

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
        }

        public static class BluePrintId
        {
            public const ushort CactusFlower = 256;
        }
    }
}
