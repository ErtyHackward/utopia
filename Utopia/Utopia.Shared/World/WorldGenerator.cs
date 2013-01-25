using System;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Ninject;
using S33M3Resources.Structs;
using Utopia.Shared.LandscapeEntities;

namespace Utopia.Shared.World
{
    /// <summary>
    /// Represents a highest level world generator.
    /// This class organize work of any world-generation related classes
    /// </summary>
    public class WorldGenerator : IDisposable
    {
        private delegate GeneratedChunk[,] GenerateDelegate(Range2I range);

        /// <summary>
        /// Gets or sets current world designer
        /// </summary>
        public WorldParameters WorldParameters { get; set; }

        /// <summary>
        /// Gets a generation stages manager
        /// </summary>
        public WorldGenerationStagesManager Stages { get; private set; }


        public LandscapeManager<GeneratedChunk> LandscapeManager { get; private set; }

        /// <summary>
        /// Initializes instance of world generator
        /// </summary>
        /// <param name="worldParameters">World parameters object</param>
        /// <param name="processors">Arbitrary amount of world processors</param>
        public WorldGenerator(WorldParameters worldParameters, params IWorldProcessor[] processors)
        {
            if (worldParameters == null) throw new ArgumentNullException("worldParameters");
            WorldParameters = worldParameters;

            Stages = new WorldGenerationStagesManager();
            Stages.AddStages(processors);
        }

        /// <summary>
        /// Initializes instance of world generator
        /// </summary>
        /// <param name="worldParameters">World parameters object</param>
        /// <param name="processorsConfig">Arbitrary amount of world processors</param>
        [Inject()]
        public WorldGenerator(WorldParameters worldParameters, IWorldProcessorConfig processorsConfig)
            : this(worldParameters, processorsConfig.WorldProcessors)
        {

        }

        /// <summary>
        /// Performs world generation asynchronously
        /// </summary>
        /// <param name="range">chunks to generate</param>
        /// <param name="callback">this callback will be called when world will be generated</param>
        /// <param name="state"></param>
        public IAsyncResult GenerateAsync(Range2I range, AsyncCallback callback, object state)
        {
            var del = new GenerateDelegate(Generate);
            return del.BeginInvoke(range, callback, state);
        }

        private GeneratedChunk[,] Generate(Range2I range)
        {
            if (Stages.Count == 0)
                throw new InvalidOperationException("Add at least one genereation process (stage) before starting");

            var chunks = new GeneratedChunk[range.Size.X, range.Size.Y];
            
            for (int x = 0; x < range.Size.X; x++)
            {
                for (int z = 0; z < range.Size.Y; z++)
                {
                    chunks[x, z] = new GeneratedChunk() { Position = new Vector2I(x + range.Position.X, z + range.Position.Y) };
                }
            }

            foreach (var stage in Stages)
            {
                stage.Generate(range, chunks);
            }

            return chunks;
        }

        /// <summary>
        /// Gets a generated chunk. Generates 9 chunks (target + 8 surrounding). Consider using GenerateAsync method
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public GeneratedChunk GetChunk(Vector2I position)
        {
            var chunks = Generate(new Range2I { Position = position, Size = new Vector2I(1,1) });

            return chunks[0, 0];
        }

        /// <summary>
        /// Releases all resources
        /// </summary>
        public void Dispose()
        {
            foreach (var stage in Stages)
            {
                stage.Dispose();
            }
        }
    }
}
