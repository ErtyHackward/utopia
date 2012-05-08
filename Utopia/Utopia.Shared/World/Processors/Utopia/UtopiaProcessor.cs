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
            Initialize();
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
                TerraForming(chunkBytes);

                chunk.BlockData.SetBlockBytes(chunkBytes);
            });
        }
        #endregion

        #region Private Methods
        INoise noise;
        private void Initialize()
        {
            _rnd = new Random(_worldParameters.Seed);

            noise = CreateLandFormFct(new Gradient(0, 0, 0.45, 0));
        }

        private void GenerateLandscape(byte[] ChunkCubes, ref Range3I chunkWorldRange)
        {
            //Create of a test Noise

            //Create value from Noise Fct sampling
            double[] noiseValue = NoiseSampler.NoiseSampling(noise, new Vector3I(AbstractChunk.ChunkSize.X /4 , AbstractChunk.ChunkSize.Y /8 , AbstractChunk.ChunkSize.Z /4 ),
                                                            chunkWorldRange.Position.X / 320.0, (chunkWorldRange.Position.X / 320.0) + 0.05, AbstractChunk.ChunkSize.X,
                                                            chunkWorldRange.Position.Y / 2560.0, (chunkWorldRange.Position.Y / 2560.0) + 0.4, AbstractChunk.ChunkSize.Y,
                                                            chunkWorldRange.Position.Z / 320.0, (chunkWorldRange.Position.Z / 320.0) + 0.05, AbstractChunk.ChunkSize.Z);

            //Create the chunk Block byte from noiseResult

            int noiseValueIndex = 0;
            byte cube;
            for (int X = 0; X < AbstractChunk.ChunkSize.X; X++)
            {
                for (int Z = 0; Z < AbstractChunk.ChunkSize.Z; Z++)
                {
                    for (int Y = 0; Y < AbstractChunk.ChunkSize.Y; Y++)
                    {
                        double value = noiseValue[noiseValueIndex];

                        cube = CubeId.Air;
                        if (value > 0.5)
                        {
                            cube = CubeId.Stone;
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

        private void TerraForming(byte[] ChunkCubes)
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

                    for (int Y = AbstractChunk.ChunkSize.Y - 1; Y >= 1; Y--) //Y
                    {
                        index = ((Z * AbstractChunk.ChunkSize.X) + X) * AbstractChunk.ChunkSize.Y + Y;
                        byte cubeId = ChunkCubes[index];


                        if (surfaceMudLayer > 0 && cubeId == CubeId.Air) surfaceMudLayer = 0;

                        //Be sure that the lowest Y level is "Solid"
                        if (Y == 0)
                        {
                            ChunkCubes[index] = CubeId.Rock;
                            continue;
                        }

                        //Be sure that the Highest Y level is Air
                        if (Y == 128)
                        {
                            ChunkCubes[index] = CubeId.Air;
                            continue;
                        }


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
            //Get Basic landscape forms
            ITerrainGenerator plainBase = new Plain(_rnd.Next(), ground_gradient);
            ITerrainGenerator midlandBase = new Midland(_rnd.Next(), ground_gradient);
            ITerrainGenerator montainBase = new Montain(_rnd.Next(), ground_gradient);

            //Anomalies landscape forms
            ITerrainGenerator plateau = new Plateau(_rnd.Next(), ground_gradient); //Plain anomaly

            //Terrain Type controler
            ITerrainGenerator terrainType = new TerrainType(_rnd.Next());

            //Anomalies Controler
            ITerrainGenerator anomaliesZone = new AnomaliesZones(_rnd.Next());
            ITerrainGenerator anomaliesType = new AnomaliesType(_rnd.Next());

            
            INoise plainBaseFct = plainBase.GetLandFormFct();
            INoise midlandBaseFct = midlandBase.GetLandFormFct();
            INoise montainBaseFct = montainBase.GetLandFormFct();

            INoise plateauFct = plateau.GetLandFormFct();

            INoise terrainTypeFct = terrainType.GetLandFormFct();
            
            INoise anomaliesZoneFct = anomaliesZone.GetLandFormFct();
            INoise anomaliesTypeFct = anomaliesType.GetLandFormFct();

            //Plain landscape generation fct with anomalies
            INoise plainWithAnomalies = plateauFct; 
            INoise plainFinal = new Select(plainBaseFct, plainWithAnomalies, anomaliesZoneFct, 0.55, 0.0);

            //midland landscape generation fct with anomalies
            INoise midLandFinal = midlandBaseFct;

            //0.0 => 0.3 Montains
            //0.3 => 0.6 MidLand
            //0.6 => 1 Plain
            INoise mountain_midland_select = new Select(montainBaseFct, midLandFinal, terrainTypeFct, 0.45, 0.05);
            INoise midland_plain_select = new Select(mountain_midland_select, plainFinal, terrainTypeFct, 0.65, 0.10);

            return midland_plain_select;
        }

        #endregion
    }
}
