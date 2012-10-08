using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using System;
using Utopia.Shared.Entities.Concrete.Collectible;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities;
using S33M3CoreComponents.Maths.Noises;
using S33M3Resources.Structs;
using S33M3CoreComponents.Maths;
using Utopia.Shared.Configuration;

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
        private readonly EntityFactory _factory;

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

        public LandscapeLayersProcessor(WorldParameters worldParameters, EntityFactory factory)
        {
            _worldParameters = worldParameters;
            _factory = factory;
            Initialize();
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public virtual void Initialize()
        {
            //Create a rnd generator based on the seed.
            _rnd = new Random(_worldParameters.Seed);

            _sand = new SimplexNoise(_rnd);
            _gravel = new SimplexNoise(_rnd);

            //Give parameters to the various noise functions
            _sand.SetParameters(0.009, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);
            _gravel.SetParameters(0.008, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);
        }

        /// <summary>
        /// Starts generation process.
        /// </summary>
        public void Generate(Range2I generationRange, GeneratedChunk[,] chunks)
        {
            _totalChunks = generationRange.Count;
            _chunksDone = 0;
            Range3I chunkWorldRange;

            generationRange.Foreach(pos =>
            {
                var r = new FastRandom(_worldParameters.Seed + pos.GetHashCode());
                var chunk = chunks[pos.X - generationRange.Position.X, pos.Y - generationRange.Position.Y];

                //var chunkBytes = new byte[AbstractChunk.ChunkBlocksByteLength];

                chunkWorldRange = new Range3I() { Position = new Vector3I(pos.X * AbstractChunk.ChunkSize.X, 0, pos.Y * AbstractChunk.ChunkSize.Z),
                                                  Size = AbstractChunk.ChunkSize
                                                };
                                                  //Max = new Vector3I((pos.X * AbstractChunk.ChunkSize.X) + AbstractChunk.ChunkSize.X, AbstractChunk.ChunkSize.Y, (pos.Y * AbstractChunk.ChunkSize.Z) + AbstractChunk.ChunkSize.Z) };

                TerraForming(chunk, ref chunkWorldRange, r);

                //chunk.BlockData.SetBlockBytes(chunkBytes);
                _chunksDone++;
            });
        }

        private void TerraForming(GeneratedChunk chunk, ref Range3I workingRange, FastRandom randomizer)
        {
            byte cubeId;
            int index;
            bool sandPlaced;
            int surfaceMud, surfaceMudLayer;
            int inWaterMaxLevel = 0;
            NoiseResult sandResult, gravelResult;
            int localX, localY, localZ;

            byte[] Cubes = chunk.BlockData.GetBlocksBytes();

            //Parcourir le _landscape pour changer les textures de surfaces
            for (int X = workingRange.Position.X; X < workingRange.Max.X; X++) //X
            {
                localX = X - workingRange.Position.X;
                for (int Z = workingRange.Position.Z; Z < workingRange.Max.Z; Z++) //Z
                {
                    surfaceMud = _rnd.Next(1, 4);
                    inWaterMaxLevel = 0;
                    surfaceMudLayer = 0;

                    //Check for Sand
                    sandResult = _sand.GetNoise2DValue(X, Z, 3, 0.50);
                    gravelResult = _gravel.GetNoise2DValue(X, Z, 3, 0.75);
                    sandPlaced = false;
                    index = -1;

                    localZ = Z - workingRange.Position.Z;
                    for (int Y = AbstractChunk.ChunkSize.Y - 1; Y >= 1; Y--) //Y
                    {
                        localY = Y - workingRange.Position.Y;

                        index = localX * AbstractChunk.ChunkSize.Y + localY + localZ * AbstractChunk.ChunkSize.X * AbstractChunk.ChunkSize.Y;
                        cubeId = Cubes[index];

                        if (surfaceMudLayer > 0 && cubeId == RealmConfiguration.CubeId.Air) surfaceMudLayer = 0;

                        //Be sure that the lowest Y level is "Solid"
                        if (Y <= _rnd.Next(5))
                        {
                            Cubes[index] = RealmConfiguration.CubeId.Rock;
                            continue;
                        }

                        if (cubeId == RealmConfiguration.CubeId.Stone)
                        {

                            if (Y > 64 - 3 && Y <= 64 + 1 && sandResult.Value > 0.7)
                            {
                                Cubes[index] = RealmConfiguration.CubeId.Sand;
                                sandPlaced = true;
                                continue;
                            }

                            if (Y < 64 && inWaterMaxLevel != 0)
                            {
                                if (cubeId == RealmConfiguration.CubeId.Stone)
                                {
                                    Cubes[index] = RealmConfiguration.CubeId.Dirt;

                                }
                                break;
                            }

                            inWaterMaxLevel = 0;

                            if (surfaceMud > surfaceMudLayer)
                            {
                                if (surfaceMudLayer == 0 && sandPlaced == false)
                                {
                                    Cubes[index] = RealmConfiguration.CubeId.Grass;

                                    //if (randomizer.NextDouble() < 0.005d)
                                    //{
                                    //    //AddTree(chunk, new Vector3I(X, Y + 1, Z));
                                    //}
                                    //else
                                    //    if (randomizer.NextDouble() < 0.03)
                                    //    {
                                    //        double result = randomizer.NextDouble();
                                    //        var globalPos = new Vector3D(X + 0.5, Y + 1, Z + 0.5);
                                    //        if (result <= 0.4)
                                    //        {
                                    //            var grass = _factory.CreateEntity<Grass>();

                                    //            grass.GrowPhase = (byte)randomizer.Next(0, 5);
                                    //            grass.Position = globalPos + new Vector3D(0.5, 1, 0.5);
                                    //            grass.LinkedCube = new Vector3I(globalPos.X, globalPos.Y, globalPos.Z);

                                    //            chunk.Entities.Add(grass);
                                    //        }
                                    //        else if (result <= 0.6)
                                    //        {
                                    //            var entity = _factory.CreateEntity<Flower1>();

                                    //            entity.Position = globalPos + new Vector3D(0.5, 1, 0.5);
                                    //            entity.LinkedCube = new Vector3I(globalPos.X, globalPos.Y, globalPos.Z);

                                    //            chunk.Entities.Add(entity);
                                    //        }
                                    //        else if (result <= 0.7)
                                    //        {
                                    //            var entity = _factory.CreateEntity<Flower2>();

                                    //            entity.Position = globalPos + new Vector3D(0.5, 1, 0.5);
                                    //            entity.LinkedCube = new Vector3I(globalPos.X, globalPos.Y, globalPos.Z);

                                    //            chunk.Entities.Add(entity);
                                    //        }
                                    //        else if (result <= 0.9)
                                    //        {
                                    //            var entity = _factory.CreateEntity<Mushr1>();

                                    //            entity.Position = globalPos + new Vector3D(0.5, 1, 0.5);
                                    //            entity.LinkedCube = new Vector3I(globalPos.X, globalPos.Y, globalPos.Z);

                                    //            chunk.Entities.Add(entity);
                                    //        }
                                    //        else if (result <= 1)
                                    //        {
                                    //            var entity = _factory.CreateEntity<Mushr2>();

                                    //            entity.Position = globalPos + new Vector3D(0.5, 1, 0.5);
                                    //            entity.LinkedCube = new Vector3I(globalPos.X, globalPos.Y, globalPos.Z);

                                    //            chunk.Entities.Add(entity);
                                    //        }
                                    //    }




                                }
                                else
                                {
                                    if (Y > 64 - 1 && Y <= 64 + 4 && gravelResult.Value > 1.8)
                                    {
                                        Cubes[index] = RealmConfiguration.CubeId.Gravel;
                                        continue;
                                    }
                                    else
                                    {
                                        Cubes[index] = RealmConfiguration.CubeId.Dirt;
                                    }
                                }
                                surfaceMudLayer++;
                            }
                        }
                        else
                        {
                            if (cubeId == RealmConfiguration.CubeId.StillWater)
                            {
                                inWaterMaxLevel = Y;
                            }
                            else
                            {
                                if (inWaterMaxLevel > 0 && cubeId == RealmConfiguration.CubeId.Air)
                                {
                                    Cubes[index] = RealmConfiguration.CubeId.StillWater;
                                }
                            }
                        }

                    }
                }
            }
        }

        private void AddTree(GeneratedChunk chunk, Vector3I vector3i)
        {
            // don't add tree at the edge of chunk
            if (vector3i.X == 0 || vector3i.X == AbstractChunk.ChunkSize.X - 1 || vector3i.Z == 0 || vector3i.Z == AbstractChunk.ChunkSize.Z - 1)
                return;

            for (int i = 0; i < 7; i++)
            {
                TryAddBlock(chunk, vector3i, RealmConfiguration.CubeId.Trunk);
                vector3i.Y++;
            }

            var radius = 2;
            for (int y = 0; y < 4; y++)
            {
                if (y == 0 || y == 3) radius = 1; else radius = 2;
                for (int x = -radius; x <= radius; x++)
                {
                    for (int z = -radius; z <= radius; z++)
                    {
                        TryAddBlock(chunk, new Vector3I(vector3i.X + x, vector3i.Y, vector3i.Z + z), RealmConfiguration.CubeId.Foliage);
                    }
                }
                vector3i.Y--;
            }
        }

        private void TryAddBlock(GeneratedChunk chunk, Vector3I pos, byte value)
        {
            if (pos.X >= 0 && pos.X < AbstractChunk.ChunkSize.X && pos.Y >= 0 && pos.Y < AbstractChunk.ChunkSize.Y && pos.Z >= 0 && pos.Z < AbstractChunk.ChunkSize.Z)
            {
                chunk.BlockData[pos] = value;
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
