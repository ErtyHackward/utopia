using System;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.NoiseResultCombiner;
using S33M3CoreComponents.Noise.Sampler;
using S33M3CoreComponents.Noise.Various;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.World.Processors.Utopia.LandformFct;
using Utopia.Shared.World.Processors.Utopia.Biomes;
using S33M3CoreComponents.Maths;
using Utopia.Shared.World.Processors.Utopia.ClimateFct;
using System.Linq;
using Utopia.Shared.Entities;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.World.Processors.Utopia
{
    public class UtopiaProcessor : IWorldProcessor
    {
        #region Private Variables
        private WorldParameters _worldParameters;
        private EntityFactory _entityFactory;
        #endregion

        #region Public Properties
        public int PercentCompleted
        {
            get { return 0; }
        }

        public string ProcessorName
        {
            get { return "Utopia Landscape"; }
        }

        public string ProcessorDescription
        {
            get { return "New lanscape generation algo. using s33m3 engine noise framework"; }
        }
        #endregion

        public UtopiaProcessor(WorldParameters worldParameters, EntityFactory entityFactory)
        {
            _worldParameters = worldParameters;
            _entityFactory = entityFactory;
        }

        public void Dispose()
        {
        }
        #region Public Methods

        public void Generate(Structs.Range2I generationRange, Chunks.GeneratedChunk[,] chunks)
        {
            Range3I chunkWorldRange;
            generationRange.Foreach(pos =>
            {
                //Get the chunk
                GeneratedChunk chunk = chunks[pos.X - generationRange.Position.X, pos.Y - generationRange.Position.Y];
                //Create the Rnd component to be used by the landscape creator
                FastRandom chunkRnd = new FastRandom(_worldParameters.Seed + chunk.Position.GetHashCode());
                //Create a byte array that will receive the landscape generated
                byte[] chunkBytes = new byte[AbstractChunk.ChunkBlocksByteLength];
                //Create an array that wll receive the ColumnChunk Informations
                ChunkColumnInfo[] columnsInfo = new ChunkColumnInfo[AbstractChunk.ChunkSize.X * AbstractChunk.ChunkSize.Z];

                chunkWorldRange = new Range3I()
                {
                    Position = new Vector3I(pos.X * AbstractChunk.ChunkSize.X, 0, pos.Y * AbstractChunk.ChunkSize.Z),
                    Size = AbstractChunk.ChunkSize
                };

                double[,] biomeMap;
                GenerateLandscape(chunkBytes, ref chunkWorldRange,out biomeMap);
                TerraForming(chunkBytes, columnsInfo, ref chunkWorldRange, biomeMap, chunkRnd);
                ChunkMetaData metaData = CreateChunkMetaData(columnsInfo);
                Vector3D chunkWorldPosition = new Vector3D(chunk.Position.X * AbstractChunk.ChunkSize.X, 0.0 , chunk.Position.Y * AbstractChunk.ChunkSize.Z);

                PopulateChunk(chunk, chunkBytes, ref chunkWorldPosition, columnsInfo, metaData, chunkRnd, _entityFactory);

                chunk.BlockData.SetBlockBytes(chunkBytes); //Save block array
                chunk.BlockData.ColumnsInfo = columnsInfo; //Save Columns info Array
                chunk.BlockData.ChunkMetaData = metaData;  //Save the metaData Informations
            });
        }
        #endregion

        #region Private Methods

        private void CreateNoises(out INoise mainLandscape, 
                                  out INoise underground, 
                                  out INoise landScapeType, 
                                  out INoise temperature, 
                                  out INoise moisture)
        {

            INoise terrainDelimiter;
            switch (_worldParameters.Configuration.UtopiaProcessorParam.WorldType)
            {
                case enuWorldType.Island:
                    terrainDelimiter = new Cache<INoise>(new IslandCtrl(_worldParameters.Seed + 1233, _worldParameters.Configuration.UtopiaProcessorParam.IslandCtrlSize).GetLandFormFct());
                    break;
                case enuWorldType.Normal:
                default:
                    terrainDelimiter = new Cache<INoise>(new WorldCtrl(_worldParameters.Seed + 1698, _worldParameters.Configuration.UtopiaProcessorParam.WorldCtrlOctave,
                                                                                                     _worldParameters.Configuration.UtopiaProcessorParam.WorldCtrlFrequency).GetLandFormFct());
                    break;
            }

            //Normal Landscape creation
            //A gradiant is created here, on the Z component, adjusted around 0.45
            Gradient mainGradiant = new Gradient(0, 0, 0.45, 0);
            mainLandscape = new Cache<INoise>(CreateLandFormFct(mainGradiant, terrainDelimiter, out landScapeType));
            //Activate it in order to test single landscape configuration
            //mainLandscape = new Cache<INoise>(FonctionTesting(new Gradient(0, 0, 0.45, 0), terrainDelimiter, out landScapeType));

            underground = new UnderGround(_worldParameters.Seed + 999, mainLandscape, terrainDelimiter).GetLandFormFct();
            temperature = new Temperature(_worldParameters.Seed - 963).GetLandFormFct();
            moisture = new Moisture(_worldParameters.Seed - 96).GetLandFormFct();
        }

        public INoise CreateLandFormFct(Gradient ground_gradient, INoise terrainDelimiter, out INoise landScapeTypeFct)
        {
            //Create various landcreation Algo. ===================================================================
            //Montains forms
            INoise montainFct = new Montain(_worldParameters.Seed + 28051979, ground_gradient).GetLandFormFct();

            //MidLand forms
            INoise midlandFct = new Midland(_worldParameters.Seed + 08092007, ground_gradient).GetLandFormFct();

            //Plains forms ===========================================================
            INoise hillFct = new Hill(_worldParameters.Seed + 1, ground_gradient).GetLandFormFct();
            INoise plainFct = new Plain(_worldParameters.Seed, ground_gradient).GetLandFormFct();
            INoise flatFct = new Flat(_worldParameters.Seed + 96, ground_gradient).GetLandFormFct();
            //Controler to manage Plains form transitions
            INoise plainsCtrl = new PlainCtrl(_worldParameters.Seed + 96, _worldParameters.Configuration.UtopiaProcessorParam.PlainCtrlOctave,
                                                                          _worldParameters.Configuration.UtopiaProcessorParam.PlainCtrlFrequency).GetLandFormFct();         //Noise That will merge hillFct, plainFct and flatFct together

            //Oceans forms
            INoise oceanBaseFct = new Ocean(_worldParameters.Seed + 10051956, ground_gradient).GetLandFormFct();

            //Surface main controler
            INoise surfaceCtrl = new SurfaceCtrl(_worldParameters.Seed + 123, _worldParameters.Configuration.UtopiaProcessorParam.GroundCtrlOctave,
                                                                              _worldParameters.Configuration.UtopiaProcessorParam.GroundCtrlFrequency).GetLandFormFct();

            var param = _worldParameters.Configuration.UtopiaProcessorParam;

            //=====================================================================================================
            //Plains Noise selecting based on plainsCtrl controler
            //Merge flat with Plain *************************************************************
            LandscapeRange flatBasicParam = param.BasicPlain[0];
            LandscapeRange plainBasicParam = param.BasicPlain[1];
            LandscapeRange hillBasicParam = param.BasicPlain[2];
            
            //Select flat_Plain_select = new Select(flatFct, plainFct, plainsCtrl, 0.4, 0.1);
            Select flat_Plain_select = new Select(flatFct, plainFct, plainsCtrl, flatBasicParam.Size, flatBasicParam.MixedNextArea + plainBasicParam.MixedPreviousArea) { Name = "flat_Plain_select" };
            INoise flat_Plain_result = new Select(enuLandFormType.Flat, enuLandFormType.Plain, plainsCtrl, flat_Plain_select.Threshold) { Name = "flat_Plain_result" };   //Biome composition    

            //Merge Plain with hill *************************************************************
            //Select mergedPlainFct = new Select(flat_Plain_select, hillFct, plainsCtrl, 0.65, 0.1);
            Select mergedPlainFct = new Select(flat_Plain_select, hillFct, plainsCtrl, flat_Plain_select.Threshold.ScalarParam + plainBasicParam.Size, plainBasicParam.MixedNextArea + hillBasicParam.MixedPreviousArea) { Name = "mergedPlainFct" };
            INoise mergedPlainFct_result = new Select(flat_Plain_result, enuLandFormType.Hill, plainsCtrl, mergedPlainFct.Threshold) { Name = "mergedPlainFct_result" };     //Biome composition 

            //=====================================================================================================
            //Surface Noise selecting based on surfaceCtrl controler
            //Merge Plains with Midland *********************************************************
            LandscapeRange plainGroundParam = param.Ground[0];
            LandscapeRange midLandGroundParam = param.Ground[1];
            LandscapeRange MontainGroundParam = param.Ground[2];

            //Select Plain_midland_select = new Select(mergedPlainFct, midlandFct, surfaceCtrl, 0.45, 0.10);
            Select Plain_midland_select = new Select(mergedPlainFct, midlandFct, surfaceCtrl, plainGroundParam.Size, plainGroundParam.MixedNextArea + midLandGroundParam.MixedPreviousArea) { Name = "Plain_midland_select" };
            INoise Plain_midland_result = new Select(mergedPlainFct_result, enuLandFormType.Midland, surfaceCtrl, Plain_midland_select.Threshold) { Name = "Plain_midland_result" }; //Biome composition

            //Merge MidLand with Montain ********************************************************
            //Select midland_montain_select = new Select(Plain_midland_select, montainFct, surfaceCtrl, 0.55, 0.05);
            Select midland_montain_select = new Select(Plain_midland_select, montainFct, surfaceCtrl, Plain_midland_select.Threshold.ScalarParam + midLandGroundParam.Size, midLandGroundParam.MixedNextArea + MontainGroundParam.MixedPreviousArea) { Name = "midland_montain_select" };
            INoise midland_montain_result = new Select(Plain_midland_result, enuLandFormType.Montain, surfaceCtrl, midland_montain_select.Threshold) { Name = "midland_montain_result" }; //Biome composition

            //=====================================================================================================
            //Surface Noise selecting based on terrainDelimiter controler
            //Merge the Water landForm with the surface landForm
            LandscapeRange waterWorldParam = param.World[0];
            LandscapeRange groundWorldParam = param.World[1];

            //Select world_select = new Select(oceanBaseFct, midland_montain_select, terrainDelimiter, 0.009, 0.20);
            Select world_select = new Select(oceanBaseFct, midland_montain_select, terrainDelimiter, waterWorldParam.Size, waterWorldParam.MixedNextArea + groundWorldParam.MixedPreviousArea) { Name = "world_select" };
            INoise world_select_result = new Select(enuLandFormType.Ocean, midland_montain_result, terrainDelimiter, world_select.Threshold) { Name = "world_select_result" };         //Biome composition

            landScapeTypeFct = world_select_result;

            return world_select;
        }

        //public INoise FonctionTesting(Gradient ground_gradient, INoise terrainDelimiter, out INoise landScapeTypeFct)
        //{
        //    //Create various landcreation Algo. ===================================================================
        //    //Montains forms
        //    INoise montainFct = new Montain(_worldParameters.Seed + 28051979, ground_gradient).GetLandFormFct();

        //    //MidLand forms
        //    INoise midlandFct = new Midland(_worldParameters.Seed + 08092007, ground_gradient).GetLandFormFct();

        //    //Plains forms
        //    INoise hillFct = new Hill(_worldParameters.Seed + 1, ground_gradient).GetLandFormFct();
        //    INoise plainFct = new Plain(_worldParameters.Seed, ground_gradient).GetLandFormFct();
        //    INoise flatFct = new Flat(_worldParameters.Seed + 96, ground_gradient).GetLandFormFct();
        //    INoise plainsCtrl = new PlainCtrl(_worldParameters.Seed + 96).GetLandFormFct();         //Noise That will merge hillFct, plainFct and flatFct together

        //    //Oceans forms
        //    INoise oceanBaseFct = new Ocean(_worldParameters.Seed + 10051956, ground_gradient).GetLandFormFct();

        //    //Surface main controler
        //    INoise surfaceCtrl = new SurfaceCtrl(_worldParameters.Seed + 123).GetLandFormFct();

        //    var param = _worldParameters.Configuration.UtopiaProcessorParam;
        //    //=====================================================================================================
        //    // TESTING PURPOSE
        //    landScapeTypeFct = new Constant((double)enuLandFormType.Hill);
        //    return hillFct;
        //}

        private void GenerateLandscape(byte[] ChunkCubes, ref Range3I chunkWorldRange, out double[,] biomeMap)
        {
            //Create the test Noise, A new object must be created each time
            INoise mainLandscape, underground, landScapeType, temperature, moisture;
            CreateNoises(out mainLandscape, out underground, out landScapeType, out temperature, out moisture);

            //Create value from Noise Fct sampling
            //noiseLandscape[x,0] = MainLandscape
            //noiseLandscape[x,1] = underground mask
            double[,] noiseLandscape = NoiseSampler.NoiseSampling(new Vector3I(AbstractChunk.ChunkSize.X /4 , AbstractChunk.ChunkSize.Y /8 , AbstractChunk.ChunkSize.Z /4 ),
                                                            chunkWorldRange.Position.X / 320.0, (chunkWorldRange.Position.X / 320.0) + 0.05, AbstractChunk.ChunkSize.X,
                                                            chunkWorldRange.Position.Y / 2560.0, (chunkWorldRange.Position.Y / 2560.0) + 0.4, AbstractChunk.ChunkSize.Y,
                                                            chunkWorldRange.Position.Z / 320.0, (chunkWorldRange.Position.Z / 320.0) + 0.05, AbstractChunk.ChunkSize.Z,
                                                            mainLandscape, 
                                                            underground);
            //biomeMap[x,0] = Biome Type
            //biomeMap[x,1] = temperature
            //biomeMap[x,2] = Moisture
            biomeMap = NoiseSampler.NoiseSampling(new Vector2I(AbstractChunk.ChunkSize.X, AbstractChunk.ChunkSize.Z),
                                                            chunkWorldRange.Position.X / 320.0, (chunkWorldRange.Position.X / 320.0) + 0.05, AbstractChunk.ChunkSize.X,
                                                            chunkWorldRange.Position.Z / 320.0, (chunkWorldRange.Position.Z / 320.0) + 0.05, AbstractChunk.ChunkSize.Z,
                                                            landScapeType,
                                                            temperature,
                                                            moisture);

            //Create the chunk Block byte from noiseResult
            int noiseValueIndex = 0;
            byte cube;
            for (int X = 0; X < AbstractChunk.ChunkSize.X; X++)
            {
                for (int Z = 0; Z < AbstractChunk.ChunkSize.Z; Z++)
                {
                    for (int Y = 0; Y < AbstractChunk.ChunkSize.Y; Y++)
                    {
                        double value = noiseLandscape[noiseValueIndex, 0];              //Get landScape value
                        double valueUnderground = noiseLandscape[noiseValueIndex, 1];   //Get underground value
                        //Create underground "Mask"
                        //0 => Will create a Hole in the landscape = Create a underground cave
                        //1 => Will leave the landscape as is.
                        valueUnderground = valueUnderground > 0.6 ? 0.0 : 1.0;

                        value *= valueUnderground;

                        cube = RealmConfiguration.CubeId.Air;
                        if (value > 0.5)
                        {
                            cube = RealmConfiguration.CubeId.Stone;
                        }

                        //BedRock
                        if (Y == 0)
                        {
                            cube = RealmConfiguration.CubeId.Rock;
                        }
                        //Be sure that the last landscape row is composed of air
                        if (Y == AbstractChunk.ChunkSize.Y - 1)
                        {
                            cube = RealmConfiguration.CubeId.Air;
                        }

                        //Create Bottom Lava lake
                        if (Y <= 3 && cube == RealmConfiguration.CubeId.Air)
                        {
                            cube = RealmConfiguration.CubeId.StillLava;
                        }
                        
                        //Place "StillWater" block at SeaLevel
                        if (Y == _worldParameters.Configuration.UtopiaProcessorParam.WaterLevel && cube == RealmConfiguration.CubeId.Air && valueUnderground == 1)
                        {
                            cube = RealmConfiguration.CubeId.StillWater;
                        }                       

                        //Save block if changed
                        if(cube != RealmConfiguration.CubeId.Air)
                        {
                            ChunkCubes[((Z * AbstractChunk.ChunkSize.X) + X) * AbstractChunk.ChunkSize.Y + Y] = cube;
                        }
                        noiseValueIndex++;
                    }
                }
            }
        }

        private void TerraForming(byte[] ChunkCubes, ChunkColumnInfo[] columnsInfo, ref Range3I chunkWorldRange, double[,] biomeMap, FastRandom chunkRnd)
        {
            int surface, surfaceLayer;
            int inWaterMaxLevel = 0;

            Biome currentBiome;
            ChunkColumnInfo columnInfo;

            int index = 0;
            int noise2DIndex = 0;

            for (int X = 0; X < AbstractChunk.ChunkSize.X; X++) 
            {
                for (int Z = 0; Z < AbstractChunk.ChunkSize.Z; Z++)
                {
                    //Get Biomes informations for this Column============================================
                    bool mustPlacedSnow;
                    double temperature = biomeMap[noise2DIndex, 1];
                    double moisture = biomeMap[noise2DIndex, 2];
                    byte biomeId = Biome.GetBiome(biomeMap[noise2DIndex, 0], temperature, moisture);
                    //Get this landscape Column Biome value
                    currentBiome = RealmConfiguration.Biomes[biomeId];

                    //Get Temperature and Moisture
                    columnInfo = new ChunkColumnInfo()
                    {
                        Biome = biomeId,
                        Moisture = (byte)(moisture * 255),
                        Temperature = (byte)(temperature * 255),
                        MaxHeight = byte.MaxValue
                    };
                    mustPlacedSnow =  (temperature < 0.2 && moisture > 0.5);
                    //====================================================================================
                    surface = chunkRnd.Next(currentBiome.UnderSurfaceLayers.Min, currentBiome.UnderSurfaceLayers.Max + 1);
                    inWaterMaxLevel = 0;
                    surfaceLayer = 0;
                    bool solidGroundHitted = false;

                    for (int Y = AbstractChunk.ChunkSize.Y - 1; Y >= 0; Y--) //Y
                    {
                        index = ((Z * AbstractChunk.ChunkSize.X) + X) * AbstractChunk.ChunkSize.Y + Y;
                        byte cubeId = ChunkCubes[index];

                        //Restart Surface layer if needed
                        if (surfaceLayer > 0 && cubeId == RealmConfiguration.CubeId.Air && Y > (_worldParameters.Configuration.UtopiaProcessorParam.WaterLevel - 5)) surfaceLayer = 1;

                        if (cubeId == RealmConfiguration.CubeId.Stone)
                        {
                            if (solidGroundHitted == false)
                            {
                                columnInfo.MaxHeight = (byte)Y;
                                solidGroundHitted = true;
                            }

                            cubeId = currentBiome.GroundCube;

                            //Under water soil
                            if (Y < 64 && inWaterMaxLevel != 0)
                            {
                                if (cubeId == currentBiome.GroundCube)
                                {
                                    ChunkCubes[index] = currentBiome.UnderSurfaceCube;
                                }
                                break;
                            }

                            inWaterMaxLevel = 0;

                            //Surface Layer handling
                            if (surface > surfaceLayer)
                            {
                                if (surfaceLayer == 0)
                                {
                                    if (mustPlacedSnow)
                                    {
                                        //Get cube index above this one
                                        //Place a snow block on it
                                        ChunkCubes[((Z * AbstractChunk.ChunkSize.X) + X) * AbstractChunk.ChunkSize.Y + (Y + 1)] = RealmConfiguration.CubeId.Snow;
                                        mustPlacedSnow = false;
                                    }

                                    ChunkCubes[index] = currentBiome.SurfaceCube;
                                }
                                else
                                {
                                    ChunkCubes[index] = currentBiome.UnderSurfaceCube;
                                }
                                surfaceLayer++;
                            }

                        }
                        else //This block is not Stone (Air, Water, or BedRock)
                        {
                            if (cubeId == RealmConfiguration.CubeId.StillWater)
                            {
                                if (mustPlacedSnow)
                                {
                                    //Get cube index above this one
                                    //Place a snow block on it
                                    ChunkCubes[index] = RealmConfiguration.CubeId.Ice;
                                }

                                inWaterMaxLevel = Y;
                            }
                            else
                            {
                                if (inWaterMaxLevel > 0 && cubeId == RealmConfiguration.CubeId.Air)
                                {
                                    ChunkCubes[index] = RealmConfiguration.CubeId.StillWater;
                                }
                            }
                        }
                    }

                    columnsInfo[noise2DIndex] = columnInfo;
                    noise2DIndex++;

                }
            }
        }

        /// <summary>
        /// Will Populate the chunks with various resources
        /// </summary>
        /// <param name="ChunkCubes"></param>
        /// <param name="chunkMetaData"></param>
        private void PopulateChunk(GeneratedChunk chunk, byte[] chunkData, ref Vector3D chunkWorldPosition, ChunkColumnInfo[] columnInfo, ChunkMetaData chunkMetaData, FastRandom chunkRnd, EntityFactory entityFactory)
        {
            //Get Chunk Master Biome
            var masterBiome = RealmConfiguration.Biomes[chunkMetaData.ChunkMasterBiomeType];
            ByteChunkCursor dataCursor = new ByteChunkCursor(chunkData);

            masterBiome.GenerateChunkCaverns(dataCursor, chunkRnd);
            masterBiome.GenerateChunkResources(dataCursor, chunkRnd);
            masterBiome.GenerateChunkTrees(dataCursor, chunk, ref chunkWorldPosition, columnInfo, masterBiome, chunkRnd, entityFactory);
            masterBiome.GenerateChunkItems(dataCursor, chunk, ref chunkWorldPosition, columnInfo, masterBiome, chunkRnd, entityFactory);
        }

        private ChunkMetaData CreateChunkMetaData(ChunkColumnInfo[] columnsInfo)
        {
            ChunkMetaData metaData = new ChunkMetaData();
            //Compute the Master Biome for the chunk.
            metaData.ChunkMasterBiomeType = columnsInfo.GroupBy(item => item.Biome).OrderByDescending(x => x.Count()).First().Key;
            return metaData;
        }
        
        #endregion
    }
}
