using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;
using S33M3Engines.Shared.Math.Noises;
using Utopia.Shared.Structs;
using Utopia.Shared.Chunks;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Cubes;

namespace Utopia.Shared.World.Processors
{
    public class s33m3WorldProcessor : IWorldProcessor
    {
        #region Private variables
        private int _totalChunks;
        private int _chunksDone;
        //Noise functions used to create the landscape
        private SimplexNoise _baseTerran, _landHeight, _landNoise, _landRiver, _landOcean;
        private Random _rnd;
        private WorldParameters _worldParameters;
        #endregion

        #region Public properties/Variables
        /// <summary>
        /// Gets overall operation progress [0; 100]
        /// </summary>
        public int PercentCompleted
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets current processor name
        /// </summary>
        public string ProcessorName
        {
            get { return "Terrain generator based on simplex Algo"; }
        }

        /// <summary>
        /// Gets current processor description
        /// </summary>
        public string ProcessorDescription
        {
            get { return "Terrain generator based on simplex Algo"; }
        }
        #endregion

        public s33m3WorldProcessor(WorldParameters worldParameters)
        {
            _worldParameters = worldParameters;
            Initialize();
        }

        #region Public Methods
        public void Generate(Structs.Range2 generationRange, Chunks.GeneratedChunk[,] chunks)
        {
            Range<int> chunkWorldRange;
            _totalChunks = generationRange.Count;
            _chunksDone = 0;
            generationRange.Foreach(pos =>
            {
                var chunk = chunks[pos.X - generationRange.Position.X, pos.Y - generationRange.Position.Y];
                var chunkBytes = new byte[AbstractChunk.ChunkBlocksByteLength];

                chunkWorldRange = new Range<int>() { Min = new Vector3I(pos.X * AbstractChunk.ChunkSize.X, 0, pos.Y * AbstractChunk.ChunkSize.Z), Max = new Vector3I((pos.X * AbstractChunk.ChunkSize.X) + AbstractChunk.ChunkSize.X, AbstractChunk.ChunkSize.Y, (pos.Y * AbstractChunk.ChunkSize.Z) + AbstractChunk.ChunkSize.Z) };

                GenerateLayoutFrom3DNoise(chunkBytes, ref chunkWorldRange);

                chunk.BlockData.SetBlockBytes(chunkBytes);
                _chunksDone++;
            });
        }

        public void Dispose()
        {
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Initialization
        /// </summary>
        private void Initialize()
        {
            //Create a rnd generator based on the seed.
            _rnd = new Random(_worldParameters.Seed);
            _baseTerran = new SimplexNoise(_rnd);
            _landHeight = new SimplexNoise(_rnd);
            _landNoise = new SimplexNoise(_rnd);
            _landRiver = new SimplexNoise(_rnd);
            _landOcean = new SimplexNoise(_rnd);

            //Give parameters to the various noise functions
            _landHeight.SetParameters(0.0025, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.MinOneToOne);
            _landNoise.SetParameters(0.0025, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.MinOneToOne);

            _landRiver.SetParameters(0.0025, SimplexNoise.InflectionMode.ABSFct, SimplexNoise.ResultScale.MinOneToOne);
            _landOcean.SetParameters(0.003, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.MinOneToOne);

            _baseTerran.SetParameters(0.009, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.MinOneToOne);
        }


        /// <summary>
        /// Generate the Landscape, but only every X blocks, the result between will be linearly interpolated (Faster this way). 
        /// </summary>
        /// <param name="Cubes">Cube array result</param>
        /// <param name="TerraCubes">TerraCube array result</param>
        /// <param name="workingRange">The chunk working range</param>
        private void GenerateLayoutFrom3DNoise(byte[] Cubes, ref Range<int> workingRange)
        {
            double[] _baseTerranResult = null;
            double[] _landHeightResult = null;
            double[] _landNoiseResult = null;
            double[] _landOceanResult = null;
            double[] _landRiverResult = null;

            double[] _mergedNoises = null;

            int XSamplingCount = 4;
            int ZSamplingCount = 4;
            int YSamplingCount = 16;

            int XPointLerpedCount = (workingRange.Max.X - workingRange.Min.X) / XSamplingCount;
            int ZPointLerpedCount = (workingRange.Max.X - workingRange.Min.X) / ZSamplingCount;
            int YPointLerpedCount = workingRange.Max.Y / YSamplingCount; //LandscapeBuilder.MaxYWorldGeneration / YSamplingCount;

            NoiseResult terranRange, landHeightRange, landNoiseRange, landRiverRange, landOceanRange;
            _mergedNoises = new double[(XSamplingCount + 1) * (YSamplingCount + 1) * (ZSamplingCount + 1)];
            // !!!
            // The sampling count are linked to the chunk sized !
            //
            // Chunk X size = 16 ==> 4 sampling instead of 16 ! (4+1 for lerping)
            //
            // !!!

            //Get all the noise at once inside an array ==> Aim here is to sample less points than the workingRange
            //Asking to sample 4 points you will need to pass one more to return 5 points to give the possibility to make a Lerp on the result
            _baseTerran.GetNoise3DValueWithAccumulation(ref _baseTerranResult, workingRange.Min.X, workingRange.Max.X, XSamplingCount,
                                                                    workingRange.Min.Y, workingRange.Max.Y, YSamplingCount,
                                                                    workingRange.Min.Z, workingRange.Max.Z, ZSamplingCount,
                                                                    12, 0.50, out terranRange);

            _landHeight.GetNoise2DValueWithAccumulation(ref _landHeightResult, workingRange.Min.X, workingRange.Max.X, XSamplingCount,
                                                             workingRange.Min.Z, workingRange.Max.Z, ZSamplingCount,
                                                             10, 0.50, out landHeightRange);

            _landNoise.GetNoise2DValueWithAccumulation(ref _landNoiseResult, workingRange.Min.X, workingRange.Max.X, XSamplingCount,
                                                             workingRange.Min.Z, workingRange.Max.Z, ZSamplingCount,
                                                             16, 0.50, out landNoiseRange);

            _landRiver.GetNoise2DValueWithAccumulation(ref _landRiverResult, workingRange.Min.X, workingRange.Max.X, XSamplingCount,
                                                             workingRange.Min.Z, workingRange.Max.Z, ZSamplingCount,
                                                             1, 0.25, out landRiverRange);

            _landOcean.GetNoise2DValueWithAccumulation(ref _landOceanResult, workingRange.Min.X, workingRange.Max.X, XSamplingCount,
                                                 workingRange.Min.Z, workingRange.Max.Z, ZSamplingCount,
                                                 3, 0.5, out landOceanRange);

            //Loop through the result, and merge them !
            int Current3DIndice = 0;
            int Current2DIndice = 0;

            double height2D;
            double Noise;
            double fallOff;
            double basesealevel, sealevel; // + Noise * coef ==> Pour faire varier la hauteur !
            double terran;
            double RiverOceanDepth = 0;
            double Ocean = 0;
            double endOfWorld;
            //double endOfWorldX, endOfWorldZ;

            for (int X = 0; X <= XSamplingCount; X++)
            {
                for (int Z = 0; Z <= ZSamplingCount; Z++)
                {
                    //Make the height in the range 0.5 to 1.5;
                    height2D = MathHelper.FullLerp(0.5f, 1.5f, landHeightRange.MinValue, landHeightRange.MaxValue, _landHeightResult[Current2DIndice], true);
                    Noise = MathHelper.FullLerp(-1.5f, 1f, landNoiseRange.MinValue, landNoiseRange.MaxValue, _landNoiseResult[Current2DIndice], true);
                    RiverOceanDepth = MathHelper.FullLerp(1f, 65f, landRiverRange.MinValue, landRiverRange.MaxValue, _landRiverResult[Current2DIndice], true);
                    Ocean = MathHelper.FullLerp(-1f, 1f, landOceanRange.MinValue, landOceanRange.MaxValue, _landOceanResult[Current2DIndice], true);


                    if (RiverOceanDepth > 15) RiverOceanDepth = 0; else RiverOceanDepth /= 7;

                    if (Noise < 0)
                    {
                        Noise = -Noise * 1.5;
                    }

                    //End of world FAllOff !
                    endOfWorld = 0;
                    //if ((Math.Abs(workingRange.Min.X) + (X * XPointLerpedCount) >= TerraWorld.Worldsize.X + TerraWorld.WorldStartUpX ||
                    //    Math.Abs(workingRange.Min.Z) + (Z * ZPointLerpedCount) >= TerraWorld.Worldsize.Z + TerraWorld.WorldStartUpZ))
                    //{
                    //    endOfWorldX = (Math.Abs(workingRange.Min.X + (X * XPointLerpedCount))) - (TerraWorld.Worldsize.X + TerraWorld.WorldStartUpX);
                    //    endOfWorldZ = (Math.Abs(workingRange.Min.Z + (Z * ZPointLerpedCount))) - (TerraWorld.Worldsize.Z + TerraWorld.WorldStartUpZ);
                    //    endOfWorld = endOfWorldX < endOfWorldZ ? endOfWorldZ : endOfWorldX;

                    //    endOfWorld = MathHelper.Clamp(endOfWorld, 0, 40);

                    //    if (RiverOceanDepth < endOfWorld / 8)
                    //    {
                    //        RiverOceanDepth = endOfWorld / 8;
                    //    }
                    //}

                    if (Ocean > 0)
                    {
                        Ocean += 5 * Ocean;
                    }

                    basesealevel = (YSamplingCount / 2 * 1.1) + Noise;// -RiverOceanDepth;

                    for (int Y = 0; Y <= YSamplingCount; Y++)
                    {
                        //Si mon altitude est en dessous de la moitire de ma hauteur max + 5 && JE suis sur une rivière
                        if ((Y * YPointLerpedCount <= (_worldParameters.SeaLevel) + 5 && RiverOceanDepth > 0) || endOfWorld > 0)
                        {
                            sealevel = basesealevel - RiverOceanDepth;
                        }
                        else
                        {
                            sealevel = basesealevel - Ocean;
                        }

                        fallOff = ((Y - sealevel) * 0.3) * height2D; //Plus je m'éloigne du niveau de la mer plus la densité de mon bloc diminue !

                        //Si négatif ==> Va augmenter la densité du bloc !
                        if (fallOff < 0)
                        {
                            fallOff *= 4;
                        }

                        terran = _baseTerranResult[Current3DIndice];

                        terran -= fallOff;

                        //Si je suis sur une surface de rivière et que mon altitude est plus haut que le niveau de la rivière et que la densité de mon terrain est positive ==> Rendre négatif ! (Faire disparaitre les blocs au dessus de la rivière
                        if (RiverOceanDepth > 0 && Y > sealevel && terran > 0)
                        {
                            terran = -(terran / 3);
                        }

                        _mergedNoises[Current3DIndice] = terran;
                        Current3DIndice++;
                    }

                    Current2DIndice++;
                }
            }

            CreateLandscapeFromNoisesResult(XSamplingCount, ZSamplingCount, YSamplingCount, ref workingRange, ref Cubes, ref _mergedNoises);
        }

             /// <summary>
        /// Do the interpolation of the generated landscape
        /// </summary>
        /// <param name="XSamplingCount"></param>
        /// <param name="ZSamplingCount"></param>
        /// <param name="YSamplingCount"></param>
        /// <param name="workingRange"></param>
        /// <param name="Cubes"></param>
        /// <param name="TerraCubes"></param>
        /// <param name="dataNoises"></param>
        private  void CreateLandscapeFromNoisesResult(int XSamplingCount, int ZSamplingCount, int YSamplingCount,
                                                     ref Range<int> workingRange, ref byte[] Cubes, ref double[] dataNoises)
        {
            int XSamplingCount2 = XSamplingCount + 1;
            int ZSamplingCount2 = ZSamplingCount + 1;
            int YSamplingCount2 = YSamplingCount + 1;

            int XPointLerpedCount = AbstractChunk.ChunkSize.X / XSamplingCount;
            int ZPointLerpedCount = AbstractChunk.ChunkSize.Z / ZSamplingCount;
            int YPointLerpedCount = AbstractChunk.ChunkSize.Y / YSamplingCount;  // TerraWorld.Worldsize.Y / YSamplingCount;

            byte cube = CubeId.Air;

            //Cubic Lerping on the values returned !
            for (int SampledpointX = 0; SampledpointX < XSamplingCount; SampledpointX++)
            {
                for (int SampledpointZ = 0; SampledpointZ < ZSamplingCount; SampledpointZ++)
                {
                    for (int SampledpointY = 0; SampledpointY < YSamplingCount; SampledpointY++)
                    {
                        double NoiseX0Z0Y0 = dataNoises[((SampledpointX + 0) * ZSamplingCount2 + (SampledpointZ + 0)) * YSamplingCount2 + (SampledpointY + 0)];
                        double NoiseX0Z1Y0 = dataNoises[((SampledpointX + 0) * ZSamplingCount2 + (SampledpointZ + 1)) * YSamplingCount2 + (SampledpointY + 0)];
                        double NoiseX1Z0Y0 = dataNoises[((SampledpointX + 1) * ZSamplingCount2 + (SampledpointZ + 0)) * YSamplingCount2 + (SampledpointY + 0)];
                        double NoiseX1Z1Y0 = dataNoises[((SampledpointX + 1) * ZSamplingCount2 + (SampledpointZ + 1)) * YSamplingCount2 + (SampledpointY + 0)];

                        double DeltaX0Z0 = (dataNoises[((SampledpointX + 0) * ZSamplingCount2 + (SampledpointZ + 0)) * YSamplingCount2 + (SampledpointY + 1)] - NoiseX0Z0Y0) / YPointLerpedCount; // 128 / 16 = 8 points need to be lerped 4 times !
                        double DeltaX0Z1 = (dataNoises[((SampledpointX + 0) * ZSamplingCount2 + (SampledpointZ + 1)) * YSamplingCount2 + (SampledpointY + 1)] - NoiseX0Z1Y0) / YPointLerpedCount; // 128 / 16 = 8 points need to be lerped 4 times !
                        double DeltaX1Z0 = (dataNoises[((SampledpointX + 1) * ZSamplingCount2 + (SampledpointZ + 0)) * YSamplingCount2 + (SampledpointY + 1)] - NoiseX1Z0Y0) / YPointLerpedCount; // 128 / 16 = 8 points need to be lerped 4 times !
                        double DeltaX1Z1 = (dataNoises[((SampledpointX + 1) * ZSamplingCount2 + (SampledpointZ + 1)) * YSamplingCount2 + (SampledpointY + 1)] - NoiseX1Z1Y0) / YPointLerpedCount; // 128 / 16 = 8 points need to be lerped 4 times !

                        for (int Y = 0; Y < YPointLerpedCount; Y++)
                        {
                            double NoiseZ0 = NoiseX0Z0Y0;
                            double NoiseZ1 = NoiseX1Z0Y0;
                            double DeltaZ0 = (NoiseX0Z1Y0 - NoiseX0Z0Y0) / ZPointLerpedCount; // Chunk X lenght = 16 / 4 = 4 points needs to be lerped Twice!
                            double DeltaZ1 = (NoiseX1Z1Y0 - NoiseX1Z0Y0) / ZPointLerpedCount;

                            for (int Z = 0; Z < ZPointLerpedCount; Z++)
                            {
                                double Noise3 = NoiseZ0;
                                double DeltaX = (NoiseZ1 - NoiseZ0) / XPointLerpedCount; // Chunk Z lenght = 16 / 4 = 4 points needs to be lerped Once!

                                for (int X = 0; X < XPointLerpedCount; X++)
                                {

                                    cube = CubeId.Air;

                                    //Don't put block at Higest level Y ==> Will be air !
                                    if ((SampledpointY * YPointLerpedCount) + Y == AbstractChunk.ChunkSize.Y - 1) continue;

                                    if (Noise3 > 0 || (SampledpointY * YPointLerpedCount) + Y == 0)
                                    {
                                        cube = CubeId.Stone;
                                    }

                                    if ((SampledpointY * YPointLerpedCount) + Y == _worldParameters.SeaLevel && cube == CubeId.Air)
                                    {
                                        cube = CubeId.WaterSource;
                                    }

                                    if (cube != CubeId.Air)
                                    {
                                        if (Cubes != null)
                                        {
                                            int nX = (SampledpointX * XPointLerpedCount) + X;
                                            int nY = (SampledpointY * YPointLerpedCount) + Y;
                                            int nZ = (SampledpointZ * ZPointLerpedCount) + Z;
                                            Cubes[nX * AbstractChunk.ChunkSize.Y + nY + nZ * AbstractChunk.ChunkSize.X * AbstractChunk.ChunkSize.Y] = cube;
                                        }
                                    }
                                    //else
                                    //{
                                    //    Console.WriteLine("");
                                    //}

                                    Noise3 += DeltaX;
                                }
                                NoiseZ0 += DeltaZ0;
                                NoiseZ1 += DeltaZ1;
                            }

                            NoiseX0Z0Y0 += DeltaX0Z0;
                            NoiseX0Z1Y0 += DeltaX0Z1;
                            NoiseX1Z0Y0 += DeltaX1Z0;
                            NoiseX1Z1Y0 += DeltaX1Z1;
                        }
                    }
                }
            }
        }

    }
        #endregion
}

