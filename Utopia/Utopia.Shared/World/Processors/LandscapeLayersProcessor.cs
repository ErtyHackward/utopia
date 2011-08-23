using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Landscaping;
using Utopia.Shared.Structs;
using S33M3Engines.Shared.Math.Noises;
using System;

namespace Utopia.Shared.World.Processors
{
    /// <summary>
    /// Sample world processor that creates simple flat world
    /// </summary>
    public class LandscapeLayersProcessor : IWorldProcessor
    {
        private int _totalChunks;
        private int _chunksDone;


        private SimplexNoise _sand, _gravel;
        private Random _rnd;
        private WorldParameters _worldParameters;

        /// <summary>
        /// Gets overall operation progress [0; 100]
        /// </summary>
        public int PercentCompleted
        {
            get { return (_chunksDone * 100) / _totalChunks; }
        }

        /// <summary>
        /// Gets current processor name
        /// </summary>
        public string ProcessorName
        {
            get { return "Flat terrain generator"; }
        }

        /// <summary>
        /// Gets current processor description
        /// </summary>
        public string ProcessorDescription
        {
            get { return "Generates a flat terrain"; }
        }

        public LandscapeLayersProcessor(WorldParameters worldParameters)
        {
            _worldParameters = worldParameters;
            Initialize();
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public virtual void Initialize()
        {
            //Create a rnd generator based on the seed.
            _rnd = new Random(LandscapeBuilder.Seed);

            _sand = new SimplexNoise(_rnd);
            _gravel = new SimplexNoise(_rnd);

            //Give parameters to the various noise functions
            _sand.SetParameters(0.009, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);
            _gravel.SetParameters(0.008, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);
        }

        /// <summary>
        /// Starts generation process.
        /// </summary>
        public void Generate(Range2 generationRange, GeneratedChunk[,] chunks)
        {
            _totalChunks = generationRange.Count;
            _chunksDone = 0;
            Range<int> chunkWorldRange;

            generationRange.Foreach(pos =>
            {
                var chunk = chunks[pos.X - generationRange.Min.X, pos.Y - generationRange.Min.Y];

                //var chunkBytes = new byte[AbstractChunk.ChunkBlocksByteLength];

                chunkWorldRange = new Range<int>() { Min = new Location3<int>(pos.X, 0, pos.Y), Max = new Location3<int>(pos.X + AbstractChunk.ChunkSize.X, AbstractChunk.ChunkSize.Y, pos.Y + AbstractChunk.ChunkSize.Z) };

                TerraForming(chunk.BlockData.GetBlocksBytes(), ref chunkWorldRange);

                //chunk.BlockData.SetBlockBytes(chunkBytes);
                _chunksDone++;
            });
        }

        private void TerraForming(byte[] Cubes, ref Range<int> workingRange)
        {
            byte cubeId;
            int index;
            bool sandPlaced;
            int surfaceMud, surfaceMudLayer;
            int inWaterMaxLevel = 0;
            NoiseResult sandResult, gravelResult;
            int localX, localY, localZ;
            //Parcourir le _landscape pour changer les textures de surfaces
            for (int X = workingRange.Min.X; X < workingRange.Max.X; X++) //X
            {
                localX = X - workingRange.Min.X;
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

                    localZ = Z - workingRange.Min.Z;
                    for (int Y = AbstractChunk.ChunkSize.Y - 1; Y >= 1; Y--) //X
                    {
                        localY = Y - workingRange.Min.Y;

                        index = localX * AbstractChunk.ChunkSize.Y + localY + localZ * AbstractChunk.ChunkSize.X * AbstractChunk.ChunkSize.Y;
                        cubeId = Cubes[index];

                        if (surfaceMudLayer > 0 && cubeId == CubeId.Air) surfaceMudLayer = 0;

                        //Be sure that the lowest Y level is "Solid"
                        if (Y <= _rnd.Next(5))
                        {
                            Cubes[index] = CubeId.Rock;
                            continue;
                        }

                        if (cubeId == CubeId.Stone)
                        {

                            if (Y > _worldParameters.SeaLevel - 3 && Y <= _worldParameters.SeaLevel + 1 && sandResult.Value > 0.7)
                            {
                                Cubes[index] = CubeId.Sand;
                                sandPlaced = true;
                                continue;
                            }

                            if (Y < _worldParameters.SeaLevel && inWaterMaxLevel != 0)
                            {
                                if (cubeId == CubeId.Stone)
                                {
                                    Cubes[index] = CubeId.Dirt;

                                }
                                break;
                            }

                            inWaterMaxLevel = 0;

                            if (surfaceMud > surfaceMudLayer)
                            {
                                if (surfaceMudLayer == 0 && sandPlaced == false)
                                {
                                    Cubes[index] = CubeId.Grass;
                                }
                                else
                                {
                                    if (Y > _worldParameters.SeaLevel - 1 && Y <= _worldParameters.SeaLevel + 4 && gravelResult.Value > 1.8)
                                    {
                                        Cubes[index] = CubeId.Gravel;
                                        continue;
                                    }
                                    else
                                    {
                                        Cubes[index] = CubeId.Dirt;
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
                                    Cubes[index] = CubeId.WaterSource;
                                }
                            }
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            // nothing to dispose
        }
    }
}
