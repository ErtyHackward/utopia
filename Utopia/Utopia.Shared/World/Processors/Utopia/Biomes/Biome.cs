using System;
using System.Collections.Generic;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.World.Processors.Utopia.LandformFct;
using S33M3CoreComponents.Maths;
using Utopia.Shared.Chunks;
using Utopia.Shared.Settings;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using System.ComponentModel;
using Utopia.Shared.Configuration;
using System.Linq;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.Tools;
using System.Drawing.Design;
using Utopia.Shared.Entities.Concrete;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    [ProtoContract]
    public partial class Biome
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables

        //Chunk Population elements
        private List<CubeVein> _cubeVeins = new List<CubeVein>();
        private List<BiomeEntity> _biomeEntities = new List<BiomeEntity>();
        private List<ChunkSpawnableEntity> _spawnableEntities = new List<ChunkSpawnableEntity>();
        private List<Cavern> _caverns = new List<Cavern>();
        private BiomeTrees _biomeTrees = new BiomeTrees();
        private List<BiomeSoundSource> _ambientSound = new List<BiomeSoundSource>();

        //Chunk Composition elements
        private RangeI _underSurfaceLayers = new RangeI(1, 3);

        //Chunk filters needed to have this biome to spawn at a specific World Grid column (for a given X - Z cube world coord)
        //A landFormType
        //A temperature
        //A moisture
        private RangeD _temperatureFilter = new RangeD(0.0, 1.0);
        private RangeD _moistureFilter = new RangeD(0.0, 1.0);
        private List<enuLandFormType> _landFormFilters = new List<enuLandFormType>();
        private WorldConfiguration _config;

        #endregion

        #region Public Properties
        [Category("General")]
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        [Editor(typeof(BlueprintTypeEditor<CubeResource>), typeof(UITypeEditor))]
        [TypeConverter(typeof(BlueprintTextHintConverter))]
        public byte SurfaceCube { get; set; }

        [ProtoMember(3)]
        [Editor(typeof(BlueprintTypeEditor<CubeResource>), typeof(UITypeEditor))]
        [TypeConverter(typeof(BlueprintTextHintConverter))]
        public byte UnderSurfaceCube { get; set; }

        [ProtoMember(4)]
        [Editor(typeof(BlueprintTypeEditor<CubeResource>), typeof(UITypeEditor))]
        [TypeConverter(typeof(BlueprintTextHintConverter))]
        public byte GroundCube { get; set; }        

        [Description("Under surface layer size"), Category("Composition")]
        [ProtoMember(5)]
        public RangeI UnderSurfaceLayers
        {
            get { return _underSurfaceLayers; }
            set { _underSurfaceLayers = value; }
        }

        [Description("Mineral veins spawning configuration"), Category("Population")]
        [ProtoMember(6)]
        public List<CubeVein> CubeVeins
        {
            get { return _cubeVeins; }
            set { _cubeVeins = value; }
        }

        [Description("Biome linked ambient music"), Category("Sound")]
        [ProtoMember(8)]
        public List<BiomeSoundSource> AmbientSound
        {
            get { return _ambientSound; }
            set { _ambientSound = value; }
        }

        [Description("Cavern spawning configuration"), Category("Population")]
        [ProtoMember(9)]
        public List<Cavern> Caverns
        {
            get { return _caverns; }
            set { _caverns = value; }
        }

        [Description("Tree spawning configuration"), Category("Population")]
        [ProtoMember(10)]
        public BiomeTrees BiomeTrees
        {
            get { return _biomeTrees; }
            set { _biomeTrees = value; }
        }

        [Description("LandForm falling inside this biome"), Category("Filter")]
        [ProtoMember(11)]
        public List<enuLandFormType> LandFormFilters
        {
            get { return _landFormFilters; }
            set { _landFormFilters = value; }
        }

        [Description("Temperature range [0.00 to 1.00] inside this biome"), Category("Filter")]
        [ProtoMember(12)]
        public RangeD TemperatureFilter
        {
            get { return _temperatureFilter; }
            set { _temperatureFilter = value; RefreshWeatherHash(); }
        }

        [Description("Moisture range [0.00 to 1.00] inside this biome"), Category("Filter")]
        [ProtoMember(13)]
        public RangeD MoistureFilter
        {
            get { return _moistureFilter; }
            set { _moistureFilter = value; RefreshWeatherHash(); }
        }

        [Description("Influence in case of zone"), Category("Filter")]
        [ProtoMember(14)]
        public int ZoneWeight { get; set; }

        [Description("Entities that can spawn inside chunk"), Category("Population")]
        [ProtoMember(15)]
        public List<ChunkSpawnableEntity> SpawnableEntities
        {
            get { return _spawnableEntities; }
            set { _spawnableEntities = value; }
        }

        [Browsable(false)]
        public WorldConfiguration Configuration
        {
            get { return _config; }
            set { _config = value; }
        }

        [Browsable(false)]
        public byte Id { get; set; }
        [Browsable(false)]
        public bool isWeatherBiomes { get; set; }
        [Browsable(false)]
        public int WeatherHash { get; set; }


        private void RefreshWeatherHash()
        {
            if (MoistureFilter.Min == 0 && MoistureFilter.Max == 1 && TemperatureFilter.Min == 0 && TemperatureFilter.Max == 1)
            {
                isWeatherBiomes = false;
                WeatherHash = 0;
                return;
            }

            string resultStr = TemperatureFilter.Min.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                                   TemperatureFilter.Max.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                                   MoistureFilter.Min.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                                   MoistureFilter.Max.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
            isWeatherBiomes = true;
            WeatherHash = resultStr.GetHashCode();
        }
        #endregion

        public Biome(WorldConfiguration config)
        {
            _config = config;
        }

        #region Public Methods

        public void GenerateChunkResources(ByteChunkCursor cursor, FastRandom rnd)
        {
            //Create the various Veins in the chunk
            foreach (CubeVein vein in CubeVeins)
            {
                //Generate the vein X time
                for (int i = 0; i < vein.VeinPerChunk; i++)
                {
                    if (vein.Cube != UtopiaProcessorParams.CubeId.LavaFlow &&
                        vein.Cube != UtopiaProcessorParams.CubeId.WaterFlow)
                    {
                        //Get Rnd chunk Location.
                        int x = rnd.Next(0, 16);
                        int y = rnd.Next(vein.SpawningHeight.Min, vein.SpawningHeight.Max);
                        int z = rnd.Next(0, 16);

                        PopulateChunkWithResource(vein.Cube, cursor, x, y, z, vein.VeinSize, rnd);
                    }
                    else
                    {
                        if (vein.Cube == UtopiaProcessorParams.CubeId.LavaFlow)
                        {
                            //Get Rnd chunk Location.
                            int x = rnd.Next(vein.VeinSize, 16 - vein.VeinSize);
                            int y = rnd.Next(vein.SpawningHeight.Min, vein.SpawningHeight.Max);
                            int z = rnd.Next(vein.VeinSize, 16 - vein.VeinSize);

                            PopulateChunkWithLiquidSources(vein.Cube, cursor, x, y, z, vein.VeinSize);
                        }
                    }
                }
            }
        }

        public void GenerateChunkCaverns(ByteChunkCursor cursor, FastRandom rnd)
        {
            //Create the various Cavern in the chunk
            foreach (Cavern cavern in Caverns)
            {
                //Nbr of cavern to generate
                for (int i = 0; i < cavern.CavernPerChunk; i++)
                {
                    if (rnd.NextDouble() <= cavern.ChanceOfSpawning)
                    {
                        //Get Rnd chunk Location.
                        int x = rnd.Next(7, 9);
                        int y = rnd.Next(cavern.SpawningHeight.Min, cavern.SpawningHeight.Max);
                        int z = rnd.Next(7, 9);
                        int layer = rnd.Next(cavern.CavernHeightSize.Min, cavern.CavernHeightSize.Max + 1);
                        PopulateChunkWithCave(cursor, x, y, z, layer, cavern.Cube, rnd);
                    }
                }
            }
        }

        //Will send back a dictionnary with the entity amount that have been generated
        public Dictionary<ushort, int> GenerateChunkStaticItems(ByteChunkCursor cursor, GeneratedChunk chunk, Biome biome, FastRandom rnd, EntityFactory entityFactory, UtopiaEntitySpawningControler spawnControler)
        {
            Dictionary<ushort, int> entityAmount = new Dictionary<ushort, int>();

            foreach (ChunkSpawnableEntity entity in SpawnableEntities.Where(x => x.IsChunkGenerationSpawning))
            {
                for (int i = 0; i < entity.MaxEntityAmount; i++)
                {
                    Vector3D entityPosition;
                    if (spawnControler.TryGetSpawnLocation(entity, chunk, cursor, rnd, out entityPosition))
                    {
                        //Create the entity
                        var createdEntity = entityFactory.CreateFromBluePrint(entity.BluePrintId);
                        var chunkWorldPosition = chunk.BlockPosition;

                        //Should take into account the SpawnLocation ! (Ceiling or not !)
                        if (createdEntity is IBlockLinkedEntity)
                        {
                            Vector3I linkedCubePosition = BlockHelper.EntityToBlock(entityPosition);
                            linkedCubePosition.Y--;
                            ((IBlockLinkedEntity)createdEntity).LinkedCube = linkedCubePosition;
                        }

                        if (createdEntity is BlockLinkedItem)
                        {
                            Vector3I LocationCube = BlockHelper.EntityToBlock(entityPosition);
                            ((BlockLinkedItem)createdEntity).BlockLocationRoot = LocationCube;
                        }

                        createdEntity.Position = entityPosition;

                        chunk.Entities.Add((StaticEntity)createdEntity);
                        if (!entityAmount.ContainsKey(createdEntity.BluePrintId)) entityAmount[createdEntity.BluePrintId] = 0;
                        entityAmount[createdEntity.BluePrintId]++;
                    }
                }
            }

            return entityAmount;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Will create a resource vein
        /// </summary>
        /// <param name="cubeId">The resource to be created</param>
        /// <param name="cursor">Class helper to move inside the Chunk cube data</param>
        /// <param name="x">InsideChunk X starting position</param>
        /// <param name="y">InsideChunk Y starting position</param>
        /// <param name="z">InsideChunk Z starting position</param>
        /// <param name="qt">Vein size</param>
        /// <param name="rnd">Random generator for vein creation</param>
        private void PopulateChunkWithResource(byte cubeId, ByteChunkCursor cursor, int x, int y, int z, int qt, FastRandom rnd)
        {
            cursor.SetInternalPosition(x, y, z);
            int nbrCubePlaced;
            if (cursor.Read() == UtopiaProcessorParams.CubeId.Stone)
            {
                cursor.Write(cubeId);
                nbrCubePlaced = 1;
                for (int i = 0; i < qt + 10 && nbrCubePlaced < qt; i++)
                {
                    int relativeMove = rnd.Next(1, 7);
                    cursor.Move(relativeMove);
                    if (cursor.Read() == UtopiaProcessorParams.CubeId.Stone)
                    {
                        cursor.Write(cubeId);
                        nbrCubePlaced++;
                    }
                }
            }
        }

        /// <summary>
        /// Will create a single Liquid "Source"
        /// </summary>
        /// <param name="cubeId">the liquid CubeId</param>
        /// <param name="cursor">Class helper to move inside the chunk</param>
        /// <param name="x">InsideChunk X starting position</param>
        /// <param name="y">InsideChunk Y starting position</param>
        /// <param name="z">InsideChunk Z starting position</param>
        private void PopulateChunkWithLiquidSources(byte cubeId, ByteChunkCursor cursor, int x, int y, int z, int liquidPower)
        {
            cursor.SetInternalPosition(x, y, z);

            //Check if this source is candidate as valid source = Must be surrended by 5 solid blocks and ahave one side block going to Air
            if (cursor.Read() != UtopiaProcessorParams.CubeId.Air)
            {
                //Looking Up for Air
                if (_config.BlockProfiles[cursor.Peek(CursorRelativeMovement.Up)].IsBlockingWater == false || cursor.Peek(CursorRelativeMovement.Up) == UtopiaProcessorParams.CubeId.Snow) return;
                if (_config.BlockProfiles[cursor.Peek(CursorRelativeMovement.Down)].IsBlockingWater == false) return;
                int cpt = 0;
                //Counting the number of holes arround the source
                if (_config.BlockProfiles[cursor.Peek(CursorRelativeMovement.East)].IsBlockingWater == false) cpt++;
                if (_config.BlockProfiles[cursor.Peek(CursorRelativeMovement.West)].IsBlockingWater == false) cpt++;
                if (_config.BlockProfiles[cursor.Peek(CursorRelativeMovement.North)].IsBlockingWater == false) cpt++;
                if (_config.BlockProfiles[cursor.Peek(CursorRelativeMovement.South)].IsBlockingWater == false) cpt++;

                //Only one face touching air ==> Createing the Liquid Source !
                if (cpt != 1) return;

                cursor.Write(cubeId);
                Queue<Tuple<ByteChunkCursor, int>> sourcesWithPower = new Queue<Tuple<ByteChunkCursor, int>>();
                sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(cursor, liquidPower));
                PropagateLiquidSources(sourcesWithPower, cubeId);
            }
        }
        private void PropagateLiquidSources(Queue<Tuple<ByteChunkCursor, int>> sourcesWithPower, byte cubeId)
        {
            Tuple<ByteChunkCursor, int> liquidSource = sourcesWithPower.Dequeue();

            //Can Fall, falling doesn't remove Power at propagation
            bool isFalling = false;
            if (_config.BlockProfiles[liquidSource.Item1.Peek(CursorRelativeMovement.Down)].CubeFamilly == Enums.enuCubeFamilly.Liquid) return;
            while (_config.BlockProfiles[liquidSource.Item1.Peek(CursorRelativeMovement.Down)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == UtopiaProcessorParams.CubeId.Snow)
            {
                liquidSource.Item1.Move(CursorRelativeMovement.Down);
                liquidSource.Item1.Write(cubeId);
                isFalling = true;
            }
            if (isFalling)
            {
                sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), liquidSource.Item2));
            }
            else
            {
                int power = liquidSource.Item2 - 1;
                if (power >= 0)
                {
                    if (_config.BlockProfiles[liquidSource.Item1.Peek(CursorRelativeMovement.East)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == UtopiaProcessorParams.CubeId.Snow)
                    {
                        liquidSource.Item1.Move(CursorRelativeMovement.East);
                        liquidSource.Item1.Write(cubeId);
                        sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), power));
                        liquidSource.Item1.Move(CursorRelativeMovement.West);
                    }

                    if (_config.BlockProfiles[liquidSource.Item1.Peek(CursorRelativeMovement.West)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == UtopiaProcessorParams.CubeId.Snow)
                    {
                        liquidSource.Item1.Move(CursorRelativeMovement.West);
                        liquidSource.Item1.Write(cubeId);
                        sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), power));
                        liquidSource.Item1.Move(CursorRelativeMovement.East);
                    }

                    if (_config.BlockProfiles[liquidSource.Item1.Peek(CursorRelativeMovement.North)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == UtopiaProcessorParams.CubeId.Snow)
                    {
                        liquidSource.Item1.Move(CursorRelativeMovement.North);
                        liquidSource.Item1.Write(cubeId);
                        sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), power));
                        liquidSource.Item1.Move(CursorRelativeMovement.South);
                    }

                    if (_config.BlockProfiles[liquidSource.Item1.Peek(CursorRelativeMovement.South)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == UtopiaProcessorParams.CubeId.Snow)
                    {
                        liquidSource.Item1.Move(CursorRelativeMovement.South);
                        liquidSource.Item1.Write(cubeId);
                        sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), power));
                    }
                }
            }

            while (sourcesWithPower.Count > 0)
            {
                PropagateLiquidSources(sourcesWithPower, cubeId);
            }
        }

        private void PopulateChunkWithCave(ByteChunkCursor cursor, int x, int y, int z, int layers, byte cubeId, FastRandom rnd)
        {
            cursor.SetInternalPosition(x, y, z);

            int caveRadius = rnd.Next(3, 8);

            int layerRadiusModifier = 0;

            for (int l = 0; l < layers; l++)
            {
                //Generate Lake Layers
                for (int X = x - (caveRadius - layerRadiusModifier); X <= x + (caveRadius - layerRadiusModifier); X++)
                {
                    for (int Z = z - (caveRadius - layerRadiusModifier); Z <= z + (caveRadius - layerRadiusModifier); Z++)
                    {
                        //Create "Noise" at Cave border
                        if ((X == x - (caveRadius - layerRadiusModifier) ||
                             X == x + (caveRadius - layerRadiusModifier) ||
                             Z == z - (caveRadius - layerRadiusModifier) ||
                             Z == z + (caveRadius - layerRadiusModifier))
                             && rnd.NextDouble() < 0.2)
                        {
                            continue;
                        }

                        cursor.SetInternalPosition(X, y + l, Z);
                        if (l <= 1 && rnd.NextDouble() < 0.3)
                        {
                            cursor.Write(cubeId);
                        }
                        else
                        {
                            if (l != 0)
                            {
                                if (l == layers - 1)
                                {
                                    if (cursor.Read() == UtopiaProcessorParams.CubeId.Stone) cursor.Write(UtopiaProcessorParams.CubeId.LightWhite);
                                }
                                else cursor.Write(UtopiaProcessorParams.CubeId.Air);

                            }
                        }
                    }
                }
                if (layerRadiusModifier < caveRadius) layerRadiusModifier++;
            }
        }

        private void PopulateChunkWithItems(ByteChunkCursor cursor, GeneratedChunk chunk, ref Vector3D chunkWorldPosition, ushort bluePrintId, int x, int y, int z, FastRandom rnd, EntityFactory entityFactory, bool isBlockCentered = true)
        {
            cursor.SetInternalPosition(x, y, z);

            //Check that the block above is "Air"
            if (cursor.Peek(CursorRelativeMovement.Up) != UtopiaProcessorParams.CubeId.Air) return;
            //Check that the block below is "solid"
            byte blockBelow = cursor.Read();
            BlockProfile blockBelowProfile = _config.BlockProfiles[blockBelow];
            if (blockBelowProfile.IsSolidToEntity)
            {
                //Cloning the Entity Blue Print !
                var entity = entityFactory.CreateFromBluePrint(bluePrintId);

                if (entity is IBlockLinkedEntity)
                {
                    Vector3I linkedCubePosition = new Vector3I(chunkWorldPosition.X + x, y, chunkWorldPosition.Z + z);
                    ((IBlockLinkedEntity)entity).LinkedCube = linkedCubePosition;
                }

                if (entity is BlockLinkedItem)
                {
                    Vector3I LocationCube = new Vector3I(chunkWorldPosition.X + x, y + 1, chunkWorldPosition.Z + z);
                    ((BlockLinkedItem)entity).BlockLocationRoot = LocationCube;
                }

                double XOffset = 0.5;
                double ZOffset = 0.5;
                if (isBlockCentered == false)
                {
                    XOffset = rnd.NextDouble(0.2, 0.8);
                    ZOffset = rnd.NextDouble(0.2, 0.8);
                }

                entity.Position = new Vector3D(chunkWorldPosition.X + x + XOffset, y + 1, chunkWorldPosition.Z + z + ZOffset);

                chunk.Entities.Add((StaticEntity)entity);
            }

        }

        #endregion
    }
}
