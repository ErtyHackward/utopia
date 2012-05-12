using System;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.NoiseResultCombiner;
using S33M3CoreComponents.Noise.Sampler;
using S33M3CoreComponents.Noise.Various;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Cubes;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.World.Processors.Utopia.LandformFct;
using Utopia.Shared.World.Processors.Utopia.Biomes;

namespace Utopia.Shared.World.Processors.Utopia
{
    public class UtopiaProcessor : IWorldProcessor
    {
        #region Private Variables
        private WorldParameters _worldParameters;
        private Random _rnd;
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

        public UtopiaProcessor(WorldParameters worldParameters)
        {
            _worldParameters = worldParameters;

            _rnd = new Random(_worldParameters.Seed);
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
                var chunk = chunks[pos.X - generationRange.Position.X, pos.Y - generationRange.Position.Y];
                var chunkBytes = new byte[AbstractChunk.ChunkBlocksByteLength];

                chunkWorldRange = new Range3I()
                {
                    Position = new Vector3I(pos.X * AbstractChunk.ChunkSize.X, 0, pos.Y * AbstractChunk.ChunkSize.Z),
                    Size = AbstractChunk.ChunkSize
                };

                double[,] biomeMap;
                GenerateLandscape(chunkBytes, ref chunkWorldRange,out biomeMap);
                TerraForming(chunkBytes, ref chunkWorldRange, biomeMap);

                chunk.BlockData.SetBlockBytes(chunkBytes);
            });
        }
        #endregion

        #region Private Methods

        private void CreateNoises(out INoise mainLandscape, out INoise underground, out INoise landScapeType) //, out INoise uncommonCubeDistri, out INoise rareCubeDistri)
        {           
            INoise terrainDelimiter = new Cache<INoise>(new IslandCtrl(_worldParameters.Seed + 1233).GetLandFormFct());
            mainLandscape = new Cache<INoise>(CreateLandFormFct(new Gradient(0, 0, 0.45, 0), terrainDelimiter, out landScapeType));
            underground = new UnderGround(_worldParameters.Seed + 999, mainLandscape, terrainDelimiter).GetLandFormFct();
            //uncommonCubeDistri = new UncommonCubeDistri(_worldParameters.Seed - 6, mainLandscape).GetLandFormFct();
            //rareCubeDistri = new RareCubeDistri(_worldParameters.Seed - 63, mainLandscape).GetLandFormFct();
        }

        public INoise CreateLandFormFct(Gradient ground_gradient, INoise islandCtrl, out INoise landScapeTypeFct)
        {
            //Create various landcreation Algo. ===================================================================
            //Montains forms
            INoise montainFct = new Montain(_worldParameters.Seed + 28051979, ground_gradient).GetLandFormFct();

            //MidLand forms
            INoise midlandFct = new Midland(_worldParameters.Seed + 08092007, ground_gradient).GetLandFormFct();

            //Plains forms
            INoise hillFct = new Hill(_worldParameters.Seed + 1, ground_gradient).GetLandFormFct();
            INoise plainFct = new Plain(_worldParameters.Seed, ground_gradient).GetLandFormFct();
            INoise flatFct = new Flat(_worldParameters.Seed + 96, ground_gradient).GetLandFormFct();
            INoise plainsCtrl = new PlainCtrl(_worldParameters.Seed + 96).GetLandFormFct();         //Noise That will merge the plains type together

            //Oceans forms
            INoise oceanBaseFct = new Ocean(_worldParameters.Seed + 10051956, ground_gradient).GetLandFormFct();

            //Surface main controler
            INoise surfaceCtrl = new SurfaceCtrl(_worldParameters.Seed + 123).GetLandFormFct();

            //=====================================================================================================
            //Plains Noise selecting based on plainsCtrl controler
            Select flat_Plain_select = new Select(flatFct, plainFct, plainsCtrl, 0.25, 0.1);         //Merge flat with Plain
            INoise flat_Plain_result = new Select(enuLandFormType.Flat, enuLandFormType.Plain, plainsCtrl, flat_Plain_select.Threshold);

            Select mergedPlainFct = new Select(flat_Plain_select, hillFct, plainsCtrl, 0.60, 0.1);   //Merge Plain with hill
            INoise mergedPlainFct_result = new Select(flat_Plain_result, enuLandFormType.Hill, plainsCtrl, mergedPlainFct.Threshold);

            //=====================================================================================================
            //Surface Noise selecting based on surfaceCtrl controler
            Select Plain_midland_select = new Select(mergedPlainFct, midlandFct, surfaceCtrl, 0.45, 0.10);         //Merge Plains with Midland
            INoise Plain_midland_result = new Select(mergedPlainFct_result, enuLandFormType.Midland, surfaceCtrl, Plain_midland_select.Threshold);
                    
            Select midland_montain_select = new Select(Plain_midland_select, montainFct, surfaceCtrl, 0.55, 0.05); //Merge MidLand with Montain
            INoise midland_montain_result = new Select(Plain_midland_result, enuLandFormType.Montain, surfaceCtrl, midland_montain_select.Threshold); //Merge MidLand with Montain

            //Merge the Water landForm with the surface landForm
            Select world_select = new Select(oceanBaseFct, midland_montain_select, islandCtrl, 0.01, 0.20);         //Merge Plains with Midland
            INoise world_select_result = new Select(enuLandFormType.Ocean, midland_montain_result, islandCtrl, world_select.Threshold);         //Merge Plains with Midland

            landScapeTypeFct = world_select_result;

            return world_select;
        }

        private void GenerateLandscape(byte[] ChunkCubes, ref Range3I chunkWorldRange, out double[,] biomeMap)
        {
            //Create the test Noise, A new object must be created each time
            INoise mainLandscape, underground, landScapeType;
            CreateNoises(out mainLandscape, out underground, out landScapeType);

            //Create value from Noise Fct sampling
            double[,] noiseLandscape = NoiseSampler.NoiseSampling(new Vector3I(AbstractChunk.ChunkSize.X /4 , AbstractChunk.ChunkSize.Y /8 , AbstractChunk.ChunkSize.Z /4 ),
                                                            chunkWorldRange.Position.X / 320.0, (chunkWorldRange.Position.X / 320.0) + 0.05, AbstractChunk.ChunkSize.X,
                                                            chunkWorldRange.Position.Y / 2560.0, (chunkWorldRange.Position.Y / 2560.0) + 0.4, AbstractChunk.ChunkSize.Y,
                                                            chunkWorldRange.Position.Z / 320.0, (chunkWorldRange.Position.Z / 320.0) + 0.05, AbstractChunk.ChunkSize.Z,
                                                            mainLandscape, 
                                                            underground);

            biomeMap = NoiseSampler.NoiseSampling(new Vector2I(AbstractChunk.ChunkSize.X, AbstractChunk.ChunkSize.Z),
                                                            chunkWorldRange.Position.X / 320.0, (chunkWorldRange.Position.X / 320.0) + 0.05, AbstractChunk.ChunkSize.X,
                                                            chunkWorldRange.Position.Z / 320.0, (chunkWorldRange.Position.Z / 320.0) + 0.05, AbstractChunk.ChunkSize.Z,
                                                            landScapeType);

            //Get 2D 

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

                        cube = CubeId.Air;
                        if (value > 0.5)
                        {
                            cube = CubeId.Stone;
                        }

                        //BedRock
                        if (Y == 0)
                        {
                            cube = CubeId.Rock;
                        }

                        if (Y == AbstractChunk.ChunkSize.Y - 1)
                        {
                            cube = CubeId.Air;
                        }

                        if ( Y == _worldParameters.SeaLevel && cube == CubeId.Air)
                        {
                            cube = CubeId.WaterSource;
                        }

                        if(cube != CubeId.Air)
                        {
                            ChunkCubes[((Z * AbstractChunk.ChunkSize.X) + X) * AbstractChunk.ChunkSize.Y + Y] = cube;
                        }
                        noiseValueIndex++;
                    }
                }
            }
        }

        private void TerraForming(byte[] ChunkCubes, ref Range3I chunkWorldRange, double[,] biomeMap)
        {
            int surfaceMud, surfaceMudLayer;
            int inWaterMaxLevel = 0;

            Biome currentBiome;

            int index = 0;
            int noise2DIndex = 0;

            for (int X = 0; X < AbstractChunk.ChunkSize.X; X++) 
            {
                for (int Z = 0; Z < AbstractChunk.ChunkSize.Z; Z++)
                {
                    //Get this landscape Column Biome value
                    currentBiome = Biome.BiomeList[Biome.GetBiome(biomeMap[noise2DIndex, 0], 0, 0)];

                    surfaceMud = currentBiome.UnderSurfaceLayers.Max;
                    inWaterMaxLevel = 0;
                    surfaceMudLayer = 0;

                    for (int Y = AbstractChunk.ChunkSize.Y - 1; Y >= 0; Y--) //Y
                    {
                        index = ((Z * AbstractChunk.ChunkSize.X) + X) * AbstractChunk.ChunkSize.Y + Y;
                        byte cubeId = ChunkCubes[index];

                        if (surfaceMudLayer > 0 && cubeId == CubeId.Air) surfaceMudLayer = 0;

                        if (cubeId == CubeId.Stone)
                        {
                            cubeId = currentBiome.GroundCube;
                            //Under water soil
                            if (Y < _worldParameters.SeaLevel && inWaterMaxLevel != 0)
                            {
                                if (cubeId == currentBiome.GroundCube)
                                {
                                    ChunkCubes[index] = currentBiome.UnderSurfaceCube;
                                }
                                break;
                            }

                            inWaterMaxLevel = 0;

                            if (surfaceMud > surfaceMudLayer)
                            {
                                if (surfaceMudLayer == 0)
                                {
                                    ChunkCubes[index] = currentBiome.SurfaceCube;
                                }
                                else
                                {
                                    ChunkCubes[index] = currentBiome.UnderSurfaceCube;
                                }
                                surfaceMudLayer++;
                            }

                        }
                        else
                        {
                            if (cubeId == CubeId.WaterSource)
                            {
                                inWaterMaxLevel = Y;
                            }
                            else
                            {
                                if (inWaterMaxLevel > 0 && cubeId == CubeId.Air)
                                {
                                    ChunkCubes[index] = CubeId.WaterSource;
                                }
                            }
                        }
                    }
                    noise2DIndex++;
                }
            }
        }
        #endregion
    }
}
