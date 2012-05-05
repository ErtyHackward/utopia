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

                chunk.BlockData.SetBlockBytes(chunkBytes);
            });
        }
        #endregion

        #region Private Methods
        INoise noise;
        private void Initialize()
        {
            _rnd = new Random(_worldParameters.Seed);

            INoise ground_gradient = new Gradient(0, 0, 8, 0);
            INoise ground_gradient_cache = new Cache(ground_gradient);
            INoise lowLandNoise = CreateLowLandNoise(ground_gradient_cache);

            INoise test = new FractalFbm(new Simplex(_rnd.Next()), 6, 4, enuBaseNoiseRange.ZeroToOne);

            noise = new Select(0, 1, lowLandNoise, 0.5);

            noise = test;

        }

        private void GenerateLandscape(byte[] ChunkCubes, ref Range3I chunkWorldRange)
        {
            //Create of a test Noise

            //Create value from Noise Fct sampling
            double[] noiseValue = NoiseSampler.NoiseSampling(noise, new Vector3I(AbstractChunk.ChunkSize.X / 4, AbstractChunk.ChunkSize.Y / 8, AbstractChunk.ChunkSize.Z / 4),
                                                            chunkWorldRange.Position.X / 320.0, (chunkWorldRange.Position.X / 320.0) + 0.05, AbstractChunk.ChunkSize.X,
                                                            chunkWorldRange.Position.Y / 2560.0, (chunkWorldRange.Position.Y / 2560.0) + 0.4, AbstractChunk.ChunkSize.Y,
                                                            chunkWorldRange.Position.Z / 320.0, (chunkWorldRange.Position.Z / 320.0) + 0.05, AbstractChunk.ChunkSize.Z);
            //Create the chunk Block byte from noiseResult

            int noiseValueIndex = 0;
            for (int X = 0; X < AbstractChunk.ChunkSize.X; X++)
            {
                for (int Z = 0; Z < AbstractChunk.ChunkSize.Z; Z++)
                {
                    for (int Y = 0; Y < AbstractChunk.ChunkSize.Y; Y++)
                    {
                        double value = noiseValue[noiseValueIndex];

                        if (value > 0.5)
                        {
                            ChunkCubes[((Z * AbstractChunk.ChunkSize.X) + X) * AbstractChunk.ChunkSize.Y + Y] = CubeId.Stone;
                        }
                        noiseValueIndex++;
                    }
                }
            }

        }

        private INoise CreateLowLandNoise(INoise ground_gradient)
        {
            //Create the Lowland base fractal with range from 0 to 1 values
            INoise lowland_shape_fractal = new FractalFbm(new Perlin(1234), 2, 1, enuBaseNoiseRange.ZeroToOne);
            //Rescale + offset the output result
            INoise lowland_scale = new ScaleOffset(lowland_shape_fractal, 0.2, 0.25);
            //Remove Y value from impacting the result (Fixed to 0) = removing one dimension to the generator noise
            INoise lowland_y_scale = new ScaleDomain(lowland_scale, 1, 0);
            //Offset the ground_gradient ( = create turbulance) to the Y scale of the gradient. input value 
            INoise lowland_terrain = new Turbulence(ground_gradient, 0, lowland_y_scale);

            return lowland_terrain;
        }
        #endregion
    }
}
