using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Settings;
using Utopia.Shared.World.Processors.Utopia;
using Utopia.Shared.World.Processors.Utopia.Biomes;
using Utopia.Shared.World.Processors.Utopia.LandformFct;

namespace Utopia.Shared.Configuration
{
    /// <summary>
    /// Class that will hold the various parameters needed for landscape generation by the processor Utopia
    /// </summary>
    [ProtoContract]
    public class UtopiaProcessorParams : IProcessorParams
    {
        private List<Biome> _biomes;
        private WorldConfiguration _config;

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
            var handler = PropertyChanged;          
            if (handler != null)                                            
            {
                handler(this, new PropertyChangedEventArgs(propertyName));  
            }
        }

        public WorldConfiguration Config
        {
            get { return _config; }
            set {
                _config = value;

                foreach (var biome in Biomes)
                {
                    biome.Configuration = _config;
                }
            }
        }

        #region Public Properties
        /// <summary>
        /// Holds Biomes Profiles configuration
        /// </summary>
        [ProtoMember(1, OverwriteList = true)]
        public List<Biome> Biomes
        {
            get { return _biomes; }
            set { 
                _biomes = value;
            }
        }

        [ProtoMember(2, OverwriteList = true)]
        public List<LandscapeRange> BasicPlain { get; set; }

        [ProtoMember(3, OverwriteList = true)]
        public List<LandscapeRange> BasicMidland { get; set; }

        [ProtoMember(4, OverwriteList = true)]
        public List<LandscapeRange> BasicMontain { get; set; }

        [ProtoMember(5, OverwriteList = true)]
        public List<LandscapeRange> BasicOcean { get; set; }

        [ProtoMember(6, OverwriteList = true)]
        public List<LandscapeRange> Ground { get; set; }

        [ProtoMember(7, OverwriteList = true)]
        public List<LandscapeRange> Ocean { get; set; }

        [ProtoMember(8, OverwriteList=true)]
        public List<LandscapeRange> World { get; set; }

        [ProtoMember(9)]
        public enuWorldType WorldType { get; set; }

        [ProtoMember(10)]
        public int WorldGeneratedHeight { get; set; }

        [ProtoMember(11)]
        public int WaterLevel  { get; set; }

        [ProtoMember(12)]
        public double TempCtrlFrequency { get; set; }

        [ProtoMember(13)]
        public int TempCtrlOctave { get; set; }

        [ProtoMember(14)]
        public double MoistureCtrlFrequency { get; set; }

        [ProtoMember(15)]
        public int MoistureCtrlOctave { get; set; }

        [ProtoMember(16)]
        public double PlainCtrlFrequency { get; set; }

        [ProtoMember(17)]
        public int PlainCtrlOctave { get; set; }

        [ProtoMember(18)]
        public double GroundCtrlFrequency { get; set; }

        [ProtoMember(19)]
        public int GroundCtrlOctave { get; set; }

        [ProtoMember(20)]
        public double WorldCtrlFrequency { get; set; }

        [ProtoMember(21)]
        public int WorldCtrlOctave { get; set; }

        [ProtoMember(22)]
        public double IslandCtrlSize { get; set; }

        [ProtoMember(23)]
        public double ZoneCtrlFrequency { get; set; }
        #endregion

        public UtopiaProcessorParams()
        {
            BasicPlain = new List<LandscapeRange>();
            BasicMidland = new List<LandscapeRange>();
            BasicMontain = new List<LandscapeRange>();
            BasicOcean = new List<LandscapeRange>();
            Ground = new List<LandscapeRange>();
            Ocean = new List<LandscapeRange>();
            World = new List<LandscapeRange>();
            Biomes = new List<Biome>();
        }

        #region Public Methods
        public void CreateDefaultValues()
        {
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

            ZoneCtrlFrequency = 1;

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
                    new CubeVein(){ Name = "Sand Vein", Cube = UtopiaProcessorParams.CubeId.Sand, VeinSize = 12, VeinPerChunk = 8, SpawningHeight = new RangeB(40,128) },
                    new CubeVein(){ Name = "Rock Vein",Cube = UtopiaProcessorParams.CubeId.Rock, VeinSize = 8, VeinPerChunk = 8, SpawningHeight = new RangeB(1,50) },
                    new CubeVein(){ Name = "Dirt Vein",Cube = UtopiaProcessorParams.CubeId.Dirt, VeinSize = 12, VeinPerChunk = 16, SpawningHeight = new RangeB(1,128) },
                    new CubeVein(){ Name = "Gravel Vein",Cube = UtopiaProcessorParams.CubeId.Gravel, VeinSize = 16, VeinPerChunk = 5, SpawningHeight = new RangeB(40,128) },
                    new CubeVein(){ Name = "GoldOre Vein",Cube = UtopiaProcessorParams.CubeId.GoldOre, VeinSize = 8, VeinPerChunk = 5, SpawningHeight = new RangeB(1,40) },
                    new CubeVein(){ Name = "CoalOre Vein",Cube = UtopiaProcessorParams.CubeId.CoalOre, VeinSize = 16, VeinPerChunk = 16, SpawningHeight = new RangeB(1,80) },
                    new CubeVein(){ Name = "MoonStone Vein",Cube = UtopiaProcessorParams.CubeId.MoonStone, VeinSize = 4, VeinPerChunk = 3, SpawningHeight = new RangeB(1,20) },
                    new CubeVein(){ Name = "DynamicWater",Cube = UtopiaProcessorParams.CubeId.WaterFlow, VeinSize = 5, VeinPerChunk = 20, SpawningHeight = new RangeB(60,120) },
                    new CubeVein(){ Name = "DynamicLava",Cube = UtopiaProcessorParams.CubeId.LavaFlow, VeinSize = 5, VeinPerChunk = 40, SpawningHeight = new RangeB(2,60) }
                }
            };

            Biomes.Add(newBiome);
            return newBiome;
        }

        public IEnumerable<BlockProfile> InjectDefaultCubeProfiles()
        {
            //Stone Block
            yield return (new BlockProfile()
            {
                Name = "Stone",
                Description = "A cube",
                Id = 1,
                Tex_Top = new TextureData("Stone"),
                Tex_Bottom = new TextureData("Stone"),
                Tex_Back = new TextureData("Stone"),
                Tex_Front = new TextureData("Stone"),
                Tex_Left = new TextureData("Stone"),
                Tex_Right = new TextureData("Stone"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = false,
                Hardness = 100
            });

            //Dirt Block
            yield return (new BlockProfile()
            {
                Name = "Dirt",
                Description = "A cube",
                Id = 2,
                Tex_Top = new TextureData("Dirt"),
                Tex_Bottom = new TextureData("Dirt"),
                Tex_Back = new TextureData("Dirt"),
                Tex_Front = new TextureData("Dirt"),
                Tex_Left = new TextureData("Dirt"),
                Tex_Right = new TextureData("Dirt"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = false,
                Hardness = 50
            });

            //Grass Block
            yield return (new BlockProfile()
            {
                Name = "Grass",
                Description = "A cube",
                Id = 3,
                Tex_Top = new TextureData("Grass"),
                Tex_Bottom = new TextureData("Dirt"),
                Tex_Back = new TextureData("GrassSide"),
                Tex_Front = new TextureData("GrassSide"),
                Tex_Left = new TextureData("GrassSide"),
                Tex_Right = new TextureData("GrassSide"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = false,
                BiomeColorArrayTexture = 0,
                Hardness = 50
            });

            //StillWater Block
            yield return (new BlockProfile()
            {
                Name = "Water_still",
                Description = "A cube",
                Id = 4,
                Tex_Top = new TextureData("Water"),
                Tex_Bottom = new TextureData("Water"),
                Tex_Back = new TextureData("Water"),
                Tex_Front = new TextureData("Water"),
                Tex_Left = new TextureData("Water"),
                Tex_Right = new TextureData("Water"),
                LightAbsorbed = 20,
                IsSeeThrough = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Liquid,
                Friction = 0.3f,
                IsSystemCube = false,
                BiomeColorArrayTexture = 1,
                YBlockOffset = 0.1,
                Hardness = 0
            });

            //DynamicWater Block
            yield return (new BlockProfile()
            {
                Name = "Water_flow",
                Description = "A cube",
                Id = 5,
                Tex_Top = new TextureData("Water"),
                Tex_Bottom = new TextureData("Water"),
                Tex_Back = new TextureData("Water"),
                Tex_Front = new TextureData("Water"),
                Tex_Left = new TextureData("Water"),
                Tex_Right = new TextureData("Water"),
                LightAbsorbed = 20,
                IsSeeThrough = true,
                IsBlockingWater = true,
                IsTaggable = true,
                CubeFamilly = Enums.enuCubeFamilly.Liquid,
                Friction = 0.3f,
                IsSystemCube = false,
                BiomeColorArrayTexture = 1,
                Hardness = 0
            });

            //LightWhite Block
            yield return (new BlockProfile()
            {
                Name = "LightWhite",
                Description = "A cube",
                Id = 6,
                Tex_Top = new TextureData("Stone"),
                Tex_Bottom = new TextureData("Stone"),
                Tex_Back = new TextureData("Stone"),
                Tex_Front = new TextureData("Stone"),
                Tex_Left = new TextureData("Stone"),
                Tex_Right = new TextureData("Stone"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = false,
                IsEmissiveColorLightSource = true,
                EmissiveColorA = 255,
                EmissiveColorR = 255,
                EmissiveColorG = 255,
                EmissiveColorB = 255,
                Hardness = 100
            });

            //Rock Block
            yield return (new BlockProfile()
            {
                Name = "Rock",
                Description = "A cube",
                Id = 7,
                Tex_Top = new TextureData("Rock"),
                Tex_Bottom = new TextureData("Rock"),
                Tex_Back = new TextureData("Rock"),
                Tex_Front = new TextureData("Rock"),
                Tex_Left = new TextureData("Rock"),
                Tex_Right = new TextureData("Rock"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = false,
                SlidingValue = 0.05f,
                Hardness = 200,
                Indestructible = true
            });

            //Sand Block
            yield return (new BlockProfile()
            {
                Name = "Sand",
                Description = "A cube",
                Id = 8,
                Tex_Top = new TextureData("Sand"),
                Tex_Bottom = new TextureData("Sand"),
                Tex_Back = new TextureData("Sand"),
                Tex_Front = new TextureData("Sand"),
                Tex_Left = new TextureData("Sand"),
                Tex_Right = new TextureData("Sand"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.3f,
                IsSystemCube = false,
                Hardness = 40
            });

            //Gravel Block
            yield return (new BlockProfile()
            {
                Name = "Gravel",
                Description = "A cube",
                Id = 9,
                Tex_Top = new TextureData("Gravel"),
                Tex_Bottom = new TextureData("Gravel"),
                Tex_Back = new TextureData("Gravel"),
                Tex_Front = new TextureData("Gravel"),
                Tex_Left = new TextureData("Gravel"),
                Tex_Right = new TextureData("Gravel"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = false,
                Hardness = 100
            });

            //Trunk Block
            yield return (new BlockProfile()
            {
                Name = "Trunk",
                Description = "A cube",
                Id = 10,
                Tex_Top = new TextureData("TreeSlide"),
                Tex_Bottom = new TextureData("TreeSlide"),
                Tex_Back = new TextureData("TreeTrunk"),
                Tex_Front = new TextureData("TreeTrunk"),
                Tex_Left = new TextureData("TreeTrunk"),
                Tex_Right = new TextureData("TreeTrunk"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = false,
                Hardness = 80
            });

            //GoldOre Block
            yield return (new BlockProfile()
            {
                Name = "GoldOre",
                Description = "A cube",
                Id = 11,
                Tex_Top = new TextureData("GoldStone"),
                Tex_Bottom = new TextureData("GoldStone"),
                Tex_Back = new TextureData("GoldStone"),
                Tex_Front = new TextureData("GoldStone"),
                Tex_Left = new TextureData("GoldStone"),
                Tex_Right = new TextureData("GoldStone"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = false,
                Hardness = 1000
            });

            //CoalOre Block
            yield return (new BlockProfile()
            {
                Name = "CoalOre",
                Description = "A cube",
                Id = 12,
                Tex_Top = new TextureData("CoalStone"),
                Tex_Bottom = new TextureData("CoalStone"),
                Tex_Back = new TextureData("CoalStone"),
                Tex_Front = new TextureData("CoalStone"),
                Tex_Left = new TextureData("CoalStone"),
                Tex_Right = new TextureData("CoalStone"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = false,
                Hardness = 150
            });

            //MoonStone Block
            yield return (new BlockProfile()
            {
                Name = "MoonStone",
                Description = "A cube",
                Id = 13,
                Tex_Top = new TextureData("MoonStone"),
                Tex_Bottom = new TextureData("MoonStone"),
                Tex_Back = new TextureData("MoonStone"),
                Tex_Front = new TextureData("MoonStone"),
                Tex_Left = new TextureData("MoonStone"),
                Tex_Right = new TextureData("MoonStone"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = false,
                IsEmissiveColorLightSource = true,
                EmissiveColorA = 255,
                EmissiveColorR = 86,
                EmissiveColorG = 143,
                EmissiveColorB = 255,
                Hardness = 2000
            });

            //Foliage Block
            yield return (new BlockProfile()
            {
                Name = "Foliage",
                Description = "A cube",
                Id = 14,
                Tex_Top = new TextureData("FoliageLight"),
                Tex_Bottom = new TextureData("FoliageLight"),
                Tex_Back = new TextureData("FoliageLight"),
                Tex_Front = new TextureData("FoliageLight"),
                Tex_Left = new TextureData("FoliageLight"),
                Tex_Right = new TextureData("FoliageLight"),
                LightAbsorbed = 255,
                IsSeeThrough = true,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = false,
                BiomeColorArrayTexture = 2,
                Hardness = 20
            });

            //Snow Block
            yield return (new BlockProfile()
            {
                Name = "Snow",
                Description = "A cube",
                Id = 15,
                Tex_Top = new TextureData("Snow"),
                Tex_Bottom = new TextureData("Snow"),
                Tex_Back = new TextureData("Snow"),
                Tex_Front = new TextureData("Snow"),
                Tex_Left = new TextureData("Snow"),
                Tex_Right = new TextureData("Snow"),
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                YBlockOffset = 0.9,
                Friction = 0.35f,
                IsSystemCube = false,
                Hardness = 10
            });

            //Ice Block
            yield return (new BlockProfile()
            {
                Name = "Ice",
                Description = "A cube",
                Id = 16,
                Tex_Top = new TextureData("Ice"),
                Tex_Bottom = new TextureData("Ice"),
                Tex_Back = new TextureData("Ice"),
                Tex_Front = new TextureData("Ice"),
                Tex_Left = new TextureData("Ice"),
                Tex_Right = new TextureData("Ice"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                IsSystemCube = false,
                Hardness = 100
            });

            //StillLava Block
            yield return (new BlockProfile()
            {
                Name = "Lava_still",
                Description = "A cube",
                Id = 17,
                Tex_Top = new TextureData("Lava"),
                Tex_Bottom = new TextureData("Lava"),
                Tex_Back = new TextureData("Lava"),
                Tex_Front = new TextureData("Lava"),
                Tex_Left = new TextureData("Lava"),
                Tex_Right = new TextureData("Lava"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                IsSystemCube = false,
                IsEmissiveColorLightSource = true,
                EmissiveColorA = 255,
                EmissiveColorR = 255,
                EmissiveColorG = 161,
                EmissiveColorB = 38,
                Hardness = 200
            });

            //DynamicLava Block
            yield return (new BlockProfile()
            {
                Name = "Lava_flow",
                Description = "A cube",
                Id = 18,
                Tex_Top = new TextureData("Lava"),
                Tex_Bottom = new TextureData("Lava"),
                Tex_Back = new TextureData("Lava"),
                Tex_Front = new TextureData("Lava"),
                Tex_Left = new TextureData("Lava"),
                Tex_Right = new TextureData("Lava"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.15f,
                SlidingValue = 0.05f,
                IsSystemCube = false,
                IsEmissiveColorLightSource = true,
                EmissiveColorA = 255,
                EmissiveColorR = 255,
                EmissiveColorG = 161,
                EmissiveColorB = 38,
                IsTaggable = true,
                Hardness = 200
            });
        }

        public IEnumerable<IEntity> InjectDefaultEntities()
        {
            //Plant cactusFlower = Config.Factory.CreateEntity<Plant>();
            //cactusFlower.Name = "Cactus Flower";
            //cactusFlower.MountPoint = BlockFace.Top;
            //cactusFlower.RndRotationAroundY = true;
            //cactusFlower.ModelName = "Flower4";
            //cactusFlower.IsSystemEntity = false;      
            //cactusFlower.MaxStackSize = 99;
            //yield return cactusFlower;
            return new List<IEntity>();
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

        //Helper inner class, to quickly get the corresponding static cube ID (These cannot be modified by users), they are "system" blocks
        public static class CubeId
        {
            public const byte Air = 0;
            public const byte Stone = 1;
            public const byte Dirt = 2;
            public const byte Grass = 3;
            public const byte WaterStill = 4;
            public const byte WaterFlow = 5;
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
            public const byte LavaStill = 17;
            public const byte LavaFlow = 18;
        }

        public static class BluePrintId
        {
            public const ushort CactusFlower = 256;
        }
    }
}
