using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Interfaces;
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
        }

        #region Public Methods
        public void CreateDefaultConfiguration()
        {
            ClearAllCollections();

            CreateDefaultBiomes();

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

        public Biome CreateNewBiome()
        {
            Biome newBiome = new Biome(Config)
            {
                Name = "Default",
                SurfaceCube = WorldConfiguration.CubeId.Grass,
                UnderSurfaceCube = WorldConfiguration.CubeId.Dirt,
                GroundCube = WorldConfiguration.CubeId.Stone,
                CubeVeins = new List<CubeVein>()
                {
                    new CubeVein(){ Name = "Sand Vein", CubeId = WorldConfiguration.CubeId.Sand, VeinSize = 12, VeinPerChunk = 8, SpawningHeight = new RangeB(40,128) },
                    new CubeVein(){ Name = "Rock Vein",CubeId = WorldConfiguration.CubeId.Rock, VeinSize = 8, VeinPerChunk = 8, SpawningHeight = new RangeB(1,50) },
                    new CubeVein(){ Name = "Dirt Vein",CubeId = WorldConfiguration.CubeId.Dirt, VeinSize = 12, VeinPerChunk = 16, SpawningHeight = new RangeB(1,128) },
                    new CubeVein(){ Name = "Gravel Vein",CubeId = WorldConfiguration.CubeId.Gravel, VeinSize = 16, VeinPerChunk = 5, SpawningHeight = new RangeB(40,128) },
                    new CubeVein(){ Name = "GoldOre Vein",CubeId = WorldConfiguration.CubeId.GoldOre, VeinSize = 8, VeinPerChunk = 5, SpawningHeight = new RangeB(1,40) },
                    new CubeVein(){ Name = "CoalOre Vein",CubeId = WorldConfiguration.CubeId.CoalOre, VeinSize = 16, VeinPerChunk = 16, SpawningHeight = new RangeB(1,80) },
                    new CubeVein(){ Name = "MoonStone Vein",CubeId = WorldConfiguration.CubeId.MoonStone, VeinSize = 4, VeinPerChunk = 3, SpawningHeight = new RangeB(1,20) },
                    new CubeVein(){ Name = "DynamicWater",CubeId = WorldConfiguration.CubeId.DynamicWater, VeinSize = 5, VeinPerChunk = 20, SpawningHeight = new RangeB(60,120) },
                    new CubeVein(){ Name = "DynamicLava",CubeId = WorldConfiguration.CubeId.DynamicLava, VeinSize = 5, VeinPerChunk = 40, SpawningHeight = new RangeB(2,60) }
                }
            };

            Biomes.Add(newBiome);
            return newBiome;
        }
        #endregion

        #region Private Methods
        //Definition of default biomes
        private void CreateDefaultBiomes()
        {
            CreateNewBiome();
        }

        private void ClearAllCollections()
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
            ClearAllCollections();
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
    }
}
