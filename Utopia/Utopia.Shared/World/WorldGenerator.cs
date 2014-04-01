using System;
using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Ninject;
using S33M3Resources.Structs;
using System.Collections.Generic;

namespace Utopia.Shared.World
{
    /// <summary>
    /// Represents a highest level world generator.
    /// This class organize work of any world-generation related classes
    /// </summary>
    public class WorldGenerator : IDisposable
    {
        private delegate GeneratedChunk[,,] GenerateDelegate(Range3I range);
        private delegate void BufferFlushDelegate(Vector3I[] chunksPosition);
        private readonly List<IWorldProcessor> _processors = new List<IWorldProcessor>();
        private readonly List<IWorldProcessorBuffered> _bufferedProcessors = new List<IWorldProcessorBuffered>();

        /// <summary>
        /// Gets or sets current world designer
        /// </summary>
        public WorldParameters WorldParameters { get; set; }

        public IEntitySpawningControler EntitySpawningControler { get; set; }

        /// <summary>
        /// Initializes instance of world generator
        /// </summary>
        /// <param name="worldParameters">World parameters object</param>
        /// <param name="processors">Arbitrary amount of world processors</param>
        public WorldGenerator(WorldParameters worldParameters, params IWorldProcessor[] processors)
        {
            if (worldParameters == null) throw new ArgumentNullException("worldParameters");
            WorldParameters = worldParameters;

            foreach (var processor in processors)
            {
                _processors.Add(processor);
                if (processor is IWorldProcessorBuffered) _bufferedProcessors.Add(processor as IWorldProcessorBuffered);
            }
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

        public void FlushBuffers(params Vector3I[] chunksPosition)
        {
            foreach (var bufferedProcessor in _bufferedProcessors)
            {
                var del = new BufferFlushDelegate(bufferedProcessor.FlushBufferedChunks);
                del.BeginInvoke(chunksPosition, null, null);
                bufferedProcessor.FlushBufferedChunks(chunksPosition);
            }
        }

        /// <summary>
        /// Performs world generation asynchronously
        /// </summary>
        /// <param name="range">chunks to generate</param>
        /// <param name="callback">this callback will be called when world will be generated</param>
        /// <param name="state"></param>
        public IAsyncResult GenerateAsync(Range3I range, AsyncCallback callback, object state)
        {
            var del = new GenerateDelegate(Generate);
            return del.BeginInvoke(range, callback, state);
        }

        private GeneratedChunk[,,] Generate(Range3I range)
        {
            if (_processors.Count == 0)
                throw new InvalidOperationException("Add at least one genereation process (stage) before starting");

            var chunks = new GeneratedChunk[range.Size.X, range.Size.Y, range.Size.Z];
            
            for (int x = 0; x < range.Size.X; x++)
            {
                for (int y = 0; y < range.Size.Y; y++)
                {
                    for (int z = 0; z < range.Size.Z; z++)
                    {
                        chunks[x, y, z] = new GeneratedChunk { Position = new Vector3I(x + range.Position.X, y + range.Position.Y, z + range.Position.Z) };
                    }
                }
            }

            foreach (var processor in _processors)
            {
                processor.Generate(range, chunks);
            }

            return chunks;
        }

        /// <summary>
        /// Gets a generated chunk.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public GeneratedChunk GetChunk(Vector3I position)
        {
            var chunks = Generate(new Range3I { Position = position, Size = new Vector3I(1,1,1) });

            return chunks[0, 0, 0];
        }

        /// <summary>
        /// Releases all resources
        /// </summary>
        public void Dispose()
        {
            foreach (var processor in _processors)
            {
                processor.Dispose();
            }
        }
    }
}
