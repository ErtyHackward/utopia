using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Math.Noises;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Math;

namespace Utopia.Shared.Landscaping
{
    /// <summary>
    /// Class responsible for generating the landscape
    /// This class is thread safe.
    /// </summary>
    public static class LandscapeBuilder
    {
        //Static Properties linked to the world
        public static byte Chunksize = 16;               //Size of the chunk
        public static int WorldStartUpX = 0 * Chunksize; // World X startup Offset - MUST BE MULTIPLE OF Chunksize !! 
        public static int WorldStartUpZ = 0 * Chunksize; // World Z startup Offset - MUST BE MULTIPLE OF Chunksize !!
        public static byte ChunkPOWsize = 4;             // It n in the formula : 2^n , where the result is the chunksize ! So for a chunk size of 16 it give 4 !

        public static byte SeaLevel = 64;                //Sea level
        public static byte WorldHeight = 128;            // World Max Height
        public static byte MaxYWorldGeneration = 128;    // Use by the world generator, cannot be change without doing change to the generator (Nothing above 128 will be generated even if the world height is double that number)
        public static int Seed = 12695360;               // The seed value use to generate the landscape (Another, will give another landscape)              

        public static int ChunkGridSize;                 // Visible world size (Defined inside config file) as number of chunks on a side (ChunkGridSize * ChunkGridSize == Total numbers chunks)
        public static int ChunkGridSurface;              // = ChunkGridSize * ChunkGridSize
        public static Location3<int> Worldsize;          // Derived from ChunkGridSize
        //====================================================================================================

        //Noise functions used to create the landscape
        private static SimplexNoise _baseTerran, _landHeight, _landNoise, _landRiver, _landOcean;
        private static SimplexNoise _sand, _gravel;
        private static Random _rnd;

        /// <summary>
        /// Initialization
        /// </summary>
        public static void Initialize(int worldSize)
        {
            //Get the number of visible chunks from the config size 
            ChunkGridSize = worldSize;
            ChunkGridSurface = ChunkGridSize * ChunkGridSize;
            Worldsize = new Location3<int>(Chunksize * ChunkGridSize, WorldHeight, Chunksize * ChunkGridSize); 

            //Create a rnd generator based on the seed.
            _rnd = new Random(LandscapeBuilder.Seed);
            _baseTerran = new SimplexNoise(_rnd);
            _landHeight = new SimplexNoise(_rnd);
            _landNoise = new SimplexNoise(_rnd);
            _landRiver = new SimplexNoise(_rnd);
            _landOcean = new SimplexNoise(_rnd);

            _sand = new SimplexNoise(_rnd);
            _gravel = new SimplexNoise(_rnd);

            //Give parameters to the various noise functions
            _landHeight.SetParameters(0.0025, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.MinOneToOne);
            _landNoise.SetParameters(0.0025, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.MinOneToOne);

            _landRiver.SetParameters(0.0025, SimplexNoise.InflectionMode.ABSFct, SimplexNoise.ResultScale.MinOneToOne);
            _landOcean.SetParameters(0.003, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.MinOneToOne);

            _baseTerran.SetParameters(0.009, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.MinOneToOne);

            _sand.SetParameters(0.009, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);
            _gravel.SetParameters(0.008, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);
        }

        /// <summary>
        /// Chunk generation entry point specific specific for Client.
        /// </summary>
        /// <param name="TerraCubes>The Array of terracubes representing the world visible</param>
        /// <param name="workingRange">The chunk Cube range in world coordinate</param>
        /// <param name="withRangeClearing">The specified range will be reset to block type AIR before beginning the generation</param>
        public static void CreateChunkLandscape(TerraCube[] TerraCubes, ref Range<int> workingRange, bool withRangeClearing)
        {
            CreateChunkLandscape(null, TerraCubes, ref workingRange, withRangeClearing);
        }

        /// <summary>
        /// Chunk generation entry point specific specific for Server.
        /// </summary>
        /// <param name="Cubes>The Array of Cube ID from the chunk that will be filled</param>
        /// <param name="workingRange">The chunk Cube range in world coordinate</param>
        /// <param name="withRangeClearing">The specified range will be reset to block type AIR before beginning the generation</param>
        public static void CreateChunkLandscape(byte[] Cubes, ref Range<int> workingRange, bool withRangeClearing)
        {
            CreateChunkLandscape(Cubes, null, ref workingRange, withRangeClearing);
        }

        private static void CreateChunkLandscape(byte[] Cubes, TerraCube[] TerraCubes, ref Range<int> workingRange, bool withRangeClearing)
        {
            if (withRangeClearing) ClearRangeArea(Cubes, TerraCubes, ref workingRange);

            //Create the base landscape
            GenerateLayoutFrom3DNoise(Cubes, TerraCubes, ref workingRange);         //Noise basic generation landscape
            AfterGenerationTerraForming(Cubes, TerraCubes, ref workingRange);       //Noise + Rnd alteration
            //PopulateChunk(ref Cubes, ref workingRange);                           //Should be used to create Specific blocks like Iron, Steel, ...
        }

        /// <summary>
        /// Generate the Landscape, but only every X blocks, the result between will be linearly interpolated (Faster this way). 
        /// </summary>
        /// <param name="Cubes">Cube array result</param>
        /// <param name="TerraCubes">TerraCube array result</param>
        /// <param name="workingRange">The chunk working range</param>
        private static void GenerateLayoutFrom3DNoise(byte[] Cubes, TerraCube[] TerraCubes, ref Range<int> workingRange)
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
            int YPointLerpedCount = LandscapeBuilder.MaxYWorldGeneration / YSamplingCount; //TerraWorld.Worldsize.Y / YSamplingCount;

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
                                                                    workingRange.Min.Y, LandscapeBuilder.MaxYWorldGeneration, YSamplingCount,
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
                        if ((Y * YPointLerpedCount <= (LandscapeBuilder.SeaLevel) + 5 && RiverOceanDepth > 0) || endOfWorld > 0)
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

            CreateLandscapeFromNoisesResult(XSamplingCount, ZSamplingCount, YSamplingCount, ref workingRange, ref Cubes, ref TerraCubes, ref _mergedNoises);
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
        private static void CreateLandscapeFromNoisesResult(int XSamplingCount, int ZSamplingCount, int YSamplingCount,
                                                     ref Range<int> workingRange, ref byte[] Cubes, ref TerraCube[] TerraCubes, ref double[] dataNoises)
        {
            int XSamplingCount2 = XSamplingCount + 1;
            int ZSamplingCount2 = ZSamplingCount + 1;
            int YSamplingCount2 = YSamplingCount + 1;

            int XPointLerpedCount = LandscapeBuilder.Chunksize / XSamplingCount;
            int ZPointLerpedCount = LandscapeBuilder.Chunksize / ZSamplingCount;
            int YPointLerpedCount = LandscapeBuilder.MaxYWorldGeneration / YSamplingCount;  // TerraWorld.Worldsize.Y / YSamplingCount;

            TerraCube cube = new TerraCube(CubeId.Air);

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

                                    cube = new TerraCube(CubeId.Air);

                                    //Don't put block at Higest level Y ==> Will be air !
                                    if ((SampledpointY * YPointLerpedCount) + Y == LandscapeBuilder.Worldsize.Y - 1) continue;

                                    if (Noise3 > 0 || (SampledpointY * YPointLerpedCount) + Y == 0)
                                    {
                                        cube.Id = CubeId.Stone;
                                    }

                                    if ((SampledpointY * YPointLerpedCount) + Y == LandscapeBuilder.SeaLevel && cube.Id == CubeId.Air)
                                    {
                                        cube.Id = CubeId.WaterSource;
                                        if (TerraCubes != null)
                                        {
                                            cube.MetaData1 = LandscapeBuilder.SeaLevel;
                                            cube.MetaData2 = (byte)CubeProfile.CubesProfile[CubeId.WaterSource].FloodingPropagationPower;
                                        }
                                    }

                                    if (cube.Id != CubeId.Air)
                                    {
                                        if (TerraCubes != null)
                                        {
                                            TerraCubes[RenderIndex(
                                                                            (SampledpointX * XPointLerpedCount) + X + workingRange.Min.X,
                                                                            (SampledpointY * YPointLerpedCount) + Y + workingRange.Min.Y,
                                                                            (SampledpointZ * ZPointLerpedCount) + Z + workingRange.Min.Z)

                                                 ] = cube;
                                        }
                                        if (Cubes != null)
                                        {
                                            Cubes[((SampledpointX * XPointLerpedCount) + X + workingRange.Min.X) * workingRange.Max.X * workingRange.Max.Y +
                                                  ((SampledpointY * YPointLerpedCount) + Y + workingRange.Min.Y) * workingRange.Max.Y +
                                                  ((SampledpointZ * ZPointLerpedCount) + Z + workingRange.Min.Z)
                                                  ] = cube.Id;
                                        }
                                    }

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


        /// <summary>
        /// Responsible to clear the Working range cubes
        /// </summary>
        /// <param name="Cubes"></param>
        /// <param name="TerraCubes"></param>
        /// <param name="workingRange"></param>
        private static void ClearRangeArea(byte[] Cubes, TerraCube[] TerraCubes, ref Range<int> workingRange)
        {
            for (int X = workingRange.Min.X; X < workingRange.Max.X; X++) //X
            {
                for (int Z = workingRange.Min.Z; Z < workingRange.Max.Z; Z++) //Z
                {
                    for (int Y = workingRange.Min.Y; Y < workingRange.Max.Y; Y++) //X
                    {
                        if(TerraCubes != null) TerraCubes[RenderIndex(X, Y, Z)].Id = CubeId.Air;
                        if (Cubes != null) Cubes[X * workingRange.Max.X * workingRange.Max.Y + Y * workingRange.Max.Y + Z] = CubeId.Air;
                    }
                }
            }
        }

        /// <summary>
        /// After generation pass, to create the layers, ...
        /// </summary>
        /// <param name="Cubes"></param>
        /// <param name="TerraCubes"></param>
        /// <param name="workingRange"></param>
        private static void AfterGenerationTerraForming(byte[] Cubes, TerraCube[] TerraCubes, ref Range<int> workingRange)
        {
            byte cubeId;
            int index;
            bool sandPlaced;
            int surfaceMud, surfaceMudLayer;
            int inWaterMaxLevel = 0;
            NoiseResult sandResult, gravelResult;
            //Parcourir le _landscape pour changer les textures de surfaces
            for (int X = workingRange.Min.X; X < workingRange.Max.X; X++) //X
            {
                for (int Z = workingRange.Min.Z; Z < workingRange.Max.Z; Z++) //Z
                {
                    surfaceMud = _rnd.Next(1, 4);
                    inWaterMaxLevel = 0;
                    surfaceMudLayer = 0;

                    //Check for Sand
                    sandResult = _sand.GetNoise2DValue(X, Z, 3, 0.50);
                    gravelResult = _gravel.GetNoise2DValue(X, Z, 3, 0.75);
                    sandPlaced = false;
                    index = -1;

                    for (int Y = LandscapeBuilder.MaxYWorldGeneration - 1; Y >= 1; Y--) //X
                    {
                        if (index == -1) index = RenderIndex(X, Y, Z);
                        else index -= LandscapeBuilder.Worldsize.X * LandscapeBuilder.Worldsize.Z;

                        if(TerraCubes != null) cubeId = TerraCubes[index].Id;
                        else cubeId = Cubes[X * workingRange.Max.X * workingRange.Max.Y + Y * workingRange.Max.Y + Z];

                        if (surfaceMudLayer > 0 && cubeId == CubeId.Air) surfaceMudLayer = 0;

                        //Be sure that the lowest Y level is "Solid"
                        if (Y <= _rnd.Next(5))
                        {
                            if (TerraCubes != null) TerraCubes[index] = new TerraCube(CubeId.Rock);
                            if (Cubes != null) Cubes[X * workingRange.Max.X * workingRange.Max.Y + Y * workingRange.Max.Y + Z] = CubeId.Rock;
                            continue;
                        }

                        if (cubeId == CubeId.Stone)
                        {
                            if (Y > LandscapeBuilder.SeaLevel - 3 && Y <= LandscapeBuilder.SeaLevel + 1 && sandResult.Value > 0.7)
                            {
                                if (TerraCubes != null) TerraCubes[index] = new TerraCube(CubeId.Sand);
                                if (Cubes != null) Cubes[X * workingRange.Max.X * workingRange.Max.Y + Y * workingRange.Max.Y + Z] = CubeId.Sand;
                                sandPlaced = true;
                                continue;
                            }

                            if (Y < LandscapeBuilder.SeaLevel && inWaterMaxLevel != 0)
                            {
                                if (cubeId == CubeId.Stone)
                                {
                                    if (TerraCubes != null) TerraCubes[index] = new TerraCube(CubeId.Dirt);
                                    if (Cubes != null) Cubes[X * workingRange.Max.X * workingRange.Max.Y + Y * workingRange.Max.Y + Z] = CubeId.Dirt;

                                }
                                break;
                            }

                            inWaterMaxLevel = 0;

                            if (surfaceMud > surfaceMudLayer)
                            {
                                if (surfaceMudLayer == 0 && sandPlaced == false)
                                {
                                    if (TerraCubes != null) TerraCubes[index] = new TerraCube(CubeId.Grass);
                                    if (Cubes != null) Cubes[X * workingRange.Max.X * workingRange.Max.Y + Y * workingRange.Max.Y + Z] = CubeId.Grass;
                                }
                                else
                                {
                                    if (Y > LandscapeBuilder.SeaLevel - 1 && Y <= LandscapeBuilder.SeaLevel + 4 && gravelResult.Value > 1.8)
                                    {
                                        if (TerraCubes != null) TerraCubes[index] = new TerraCube(CubeId.Gravel);
                                        if (Cubes != null) Cubes[X * workingRange.Max.X * workingRange.Max.Y + Y * workingRange.Max.Y + Z] = CubeId.Gravel;
                                        continue;
                                    }
                                    else
                                    {
                                        if (TerraCubes != null) TerraCubes[index] = new TerraCube(CubeId.Dirt);
                                        if (Cubes != null) Cubes[X * workingRange.Max.X * workingRange.Max.Y + Y * workingRange.Max.Y + Z] = CubeId.Dirt;
                                    }
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
                                    if (TerraCubes != null) TerraCubes[index] = new TerraCube(CubeId.WaterSource) { MetaData1 = (byte)inWaterMaxLevel };
                                    if (Cubes != null) Cubes[X * workingRange.Max.X * workingRange.Max.Y + Y * workingRange.Max.Y + Z] = CubeId.WaterSource;
                                }
                            }
                        }

                    }
                }
            }
        }

        //Will compute the index from the Client TerraCube array.
        private static int RenderIndex(int X, int Y, int Z)
        {
            return MathHelper.Mod(X, LandscapeBuilder.Worldsize.X) +
                    MathHelper.Mod(Z, LandscapeBuilder.Worldsize.Z) * LandscapeBuilder.Worldsize.X +
                    Y * LandscapeBuilder.Worldsize.X * LandscapeBuilder.Worldsize.Z;
        }

    }


}
