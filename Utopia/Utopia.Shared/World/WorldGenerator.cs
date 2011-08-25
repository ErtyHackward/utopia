using System;
using Utopia.Shared.Chunks;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Ninject;

namespace Utopia.Shared.World
{
    /// <summary>
    /// Represents a highest level world generator.
    /// This class organize work of any world-generation related classes
    /// </summary>
    public class WorldGenerator : IDisposable
    {
        private delegate GeneratedChunk[,] GenerateDelegate(Range2 range);

        /// <summary>
        /// Gets or sets current world designer
        /// </summary>
        public WorldParameters WorldParametes { get; set; }

        /// <summary>
        /// Gets a generation stages manager
        /// </summary>
        public WorldGenerationStagesManager Stages { get; private set; }

        /// <summary>
        /// Initializes instance of world generator
        /// </summary>
        /// <param name="worldParameters">World parameters object</param>
        /// <param name="processors">Arbitrary amount of world processors</param>
        public WorldGenerator(WorldParameters worldParameters, params IWorldProcessor[] processors)
        {
            if (worldParameters == null) throw new ArgumentNullException("worldParameters");
            WorldParametes = worldParameters;

            Stages = new WorldGenerationStagesManager();
            Stages.AddStages(processors);
        }


        /// Initializes instance of world generator
        /// </summary>
        /// <param name="worldParameters">World parameters object</param>
        /// <param name="processors">Arbitrary amount of world processors</param>
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
        public IAsyncResult GenerateAsync(Range2 range, AsyncCallback callback, object state)
        {
            if (Stages.Count == 0)
                throw new InvalidOperationException("Add at least one genereation process (stage) before starting");
            


            var del = new GenerateDelegate(Generate);
            return del.BeginInvoke(range, callback, state);
        }

        /// <summary>
        /// Performs generation of specified range of chunks
        /// </summary>
        /// <param name="range">Range of chunks to generate</param>
        /// <param name="dataArray">Array to write data to</param>
        /// <param name="arrayOffset">dataArray shift</param>
        /// <param name="entities">Generated entities collection</param>
        public void Generate(Range2 range, byte[] dataArray, int arrayOffset, out EntityCollection[] entities)
        {
            var chunks = Generate(range);

            entities = new EntityCollection[range.Count];

            int chunkIndex = 0;

            for (int x = range.Min.X; x < range.Max.X; x++)
            {
                for (int z = range.Min.Y; z < range.Max.Y; z++)
                {
                    var chunk = chunks[x - range.Min.X, z - range.Min.Y];
                    var chunkData = chunk.BlockData.GetBlocksBytes();
                    var chunkEntities = chunk.Entities;

                    Array.Copy(chunkData, 0, dataArray, arrayOffset + chunkIndex * AbstractChunk.ChunkBlocksByteLength, AbstractChunk.ChunkBlocksByteLength);
                    entities[chunkIndex] = chunkEntities;

                    chunkIndex++;
                }
            }
        }

        private GeneratedChunk[,] Generate(Range2 range)
        {
            var chunks = new GeneratedChunk[range.Size.X, range.Size.Z];

            for (int x = range.Min.X; x < range.Max.X; x++)
            {
                for (int z = range.Min.Y; z < range.Max.Y; z++)
                {
                    chunks[x - range.Min.X, z - range.Min.Y] = new GeneratedChunk();
                }
            }

            foreach (var stage in Stages)
            {
                stage.Generate(range, chunks);
            }

            return chunks;
        }

        /// <summary>
        /// Gets a generated chunk. (Generates it if needed)
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public GeneratedChunk GetChunk(IntVector2 position)
        {
            var chunks = Generate(new Range2 { Min = position, Max = position + 1 });

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
