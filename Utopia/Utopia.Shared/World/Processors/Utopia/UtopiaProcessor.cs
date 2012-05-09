using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.Chunks;
using S33M3Resources.Structs;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Sampler;
using Utopia.Shared.Cubes;
using S33M3CoreComponents.Noise.NoiseResultCombiner;
using S33M3CoreComponents.Noise.ResultModifier;
using S33M3CoreComponents.Noise.DomainModifier;
using S33M3CoreComponents.Noise.Various;
using Utopia.Shared.World.Processors.Utopia.LandformFct;

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

                GenerateLandscape(chunkBytes, ref chunkWorldRange);
                TerraForming(chunkBytes, ref chunkWorldRange);

                chunk.BlockData.SetBlockBytes(chunkBytes);
            });
        }
        #endregion

        #region Private Methods

        private void CreateNoises(out INoise mainLandscape, out INoise underground)
        {           
            mainLandscape = CreateLandFormFct(new Gradient(0, 0, 0.45, 0));
            Cache<INoise> mainLandscapeCaching = new Cache<INoise>(mainLandscape);
            underground = CreateUnderGroundFct(mainLandscapeCaching);
        }

        private void GenerateLandscape(byte[] ChunkCubes, ref Range3I chunkWorldRange)
        {
            //Create the test Noise, A new object must be created each time
            //Because of the caching in a multithreaded situation (The caching system cannot be shared between 2 threads)
            INoise mainLandscape, underground;
            CreateNoises(out mainLandscape, out underground);

            //Create value from Noise Fct sampling
            double[,] noiseLandscape = NoiseSampler.NoiseSampling(new Vector3I(AbstractChunk.ChunkSize.X /4 , AbstractChunk.ChunkSize.Y /8 , AbstractChunk.ChunkSize.Z /4 ),
                                                            chunkWorldRange.Position.X / 320.0, (chunkWorldRange.Position.X / 320.0) + 0.05, AbstractChunk.ChunkSize.X,
                                                            chunkWorldRange.Position.Y / 2560.0, (chunkWorldRange.Position.Y / 2560.0) + 0.4, AbstractChunk.ChunkSize.Y,
                                                            chunkWorldRange.Position.Z / 320.0, (chunkWorldRange.Position.Z / 320.0) + 0.05, AbstractChunk.ChunkSize.Z,
                                                            mainLandscape, 
                                                            underground);

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
                        valueUnderground = valueUnderground > 0.5 ? 0.0 : 1.0;

                        value *= valueUnderground;

                        cube = CubeId.Air;
                        if (value > 0.5)
                        {
                            cube = CubeId.Stone;
                        }

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

        private void TerraForming(byte[] ChunkCubes, ref Range3I chunkWorldRange)
        {
            int surfaceMud, surfaceMudLayer;
            int inWaterMaxLevel = 0;

            int index = 0;
            for (int Z = 0; Z < AbstractChunk.ChunkSize.Z; Z++)
            {
                for (int X = 0; X < AbstractChunk.ChunkSize.X; X++)
                {
                    surfaceMud = 4;
                    inWaterMaxLevel = 0;
                    surfaceMudLayer = 0;

                    for (int Y = AbstractChunk.ChunkSize.Y - 1; Y >= 0; Y--) //Y
                    {
                        index = ((Z * AbstractChunk.ChunkSize.X) + X) * AbstractChunk.ChunkSize.Y + Y;
                        byte cubeId = ChunkCubes[index];

                        if (surfaceMudLayer > 0 && cubeId == CubeId.Air) surfaceMudLayer = 0;

                        if (cubeId == CubeId.Stone)
                        {
                            //Under water soil
                            if (Y < _worldParameters.SeaLevel && inWaterMaxLevel != 0)
                            {
                                if (cubeId == CubeId.Stone)
                                {
                                    ChunkCubes[index] = CubeId.Dirt;

                                }
                                break;
                            }

                            inWaterMaxLevel = 0;

                            if (surfaceMud > surfaceMudLayer)
                            {
                                if (surfaceMudLayer == 0)
                                {
                                    ChunkCubes[index] = CubeId.Grass;
                                }
                                else
                                {
                                    ChunkCubes[index] = CubeId.Dirt;
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
                }

            }
        }

        public INoise CreateLandFormFct(Gradient ground_gradient)
        {
            //Create various landcreation Algo. ===================================================================
            //Get Basic landscape forms
            INoise plainBaseFct = new Plain(_worldParameters.Seed, ground_gradient).GetLandFormFct();
            INoise midlandBaseFct = new Midland(_worldParameters.Seed + 08092007, ground_gradient).GetLandFormFct();
            INoise montainBaseFct = new Montain(_worldParameters.Seed + 28051979, ground_gradient).GetLandFormFct();
            INoise OceanBaseFct = new Ocean(_worldParameters.Seed + 10051956, ground_gradient).GetLandFormFct();

            //Plain Subtype forms
            INoise hillFct = new Hill(_worldParameters.Seed + 1, ground_gradient).GetLandFormFct();
            INoise flatFct = new Flat(_worldParameters.Seed + 96, ground_gradient).GetLandFormFct();

            //Terrain Type controler
            INoise terrainTypeFct = new TerrainType(_worldParameters.Seed + 123).GetLandFormFct();

            //Anomalies Controler
            INoise subTypeZoneFct = new SubTypeZones(_worldParameters.Seed + 96).GetLandFormFct();
            INoise SubTypeFct = new SubType(_worldParameters.Seed + 100).GetLandFormFct();
            
            //=====================================================================================================
            //Plain landscape generation fct with SubType
            //Assign Anomalies to the Plains
            INoise plainSubType = new Select(hillFct, flatFct, SubTypeFct, 0.75, 0.1);
            INoise plainFinal = new Select(plainBaseFct, plainSubType, subTypeZoneFct, 0.55, 0.1);


            //Blend together the various Landforms
            INoise mountain_midland_select = new Select(montainBaseFct, midlandBaseFct, terrainTypeFct, 0.40, 0.05);
            INoise midland_plain_select = new Select(mountain_midland_select, plainFinal, terrainTypeFct, 0.50, 0.05);
            INoise plain_Ocean_select = new Select(midland_plain_select, OceanBaseFct, terrainTypeFct, 0.90, 0.15);


            return plain_Ocean_select;
        }

        public INoise CreateUnderGroundFct(INoise landScape)
        {
            //UnderGround Tunnels and Caves
            INoise undergroundFct = new UnderGround(_worldParameters.Seed + 999, landScape).GetLandFormFct();

            return undergroundFct;
        }

        #endregion
    }
}
