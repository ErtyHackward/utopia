using System;
using Utopia.Shared.Chunks;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Ninject;
using System.Collections.Generic;

namespace Utopia.Shared.World
{
    /// <summary>
    /// Represents a highest level world generator.
    /// This class organize work of any world-generation related classes
    /// </summary>
    public class WorldGenerator : IDisposable
    {
        private bool _abortOperation;
        private delegate void GenerateDelegate(Range2 range);
        private int _activeStage;
        private Dictionary<IntVector2, GeneratedChunk> Chunks { get; private set; }

        /// <summary>
        /// Gets or sets current world designer
        /// </summary>
        public WorldParameters WorldParametes { get; set; }

        /// <summary>
        /// Gets a generation stages manager
        /// </summary>
        public WorldGenerationStagesManager Stages { get; private set; }
        
        /// <summary>
        /// Gets overall operations progress percent [0; 100]
        /// </summary>
        public int OverallProgress
        {
            get {
                // todo: need to check the formula
                var percentPerStage = 100m / Stages.Count;
                return (int) (_activeStage * percentPerStage + (Decimal)Stages[_activeStage].PercentCompleted / 100 * percentPerStage); 
            }
        }

        /// <summary>
        /// Initializes instance of world generator
        /// </summary>
        /// <param name="worldParameters">World parameters object</param>
        /// <param name="processors">Arbitrary amount of world processors</param>
        public WorldGenerator(WorldParameters worldParameters, params IWorldProcessor[] processors)
        {
            if (worldParameters == null) throw new ArgumentNullException("worldParameters");
            WorldParametes = worldParameters;

            Chunks = new Dictionary<IntVector2, GeneratedChunk>();

            Stages = new WorldGenerationStagesManager();
            Stages.AddStages(processors);
        }

        /// <summary>
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
            if(Stages.Count == 0)
                throw new InvalidOperationException("Add at least one genereation process (stage) before starting");

            _abortOperation = false;
            _activeStage = 0;
            return new GenerateDelegate(Generate).BeginInvoke(range, callback, state);
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
            Generate(range);

            entities = new EntityCollection[range.Count];

            int chunkIndex = 0;

            for (int x = range.Min.X; x < range.Max.X; x++)
            {
                for (int z = range.Min.Y; z < range.Max.Y; z++)
                {
                    var chunk = _chunks[x - range.Min.X, z - range.Min.Y];
                    var chunkData = chunk.BlockData.GetBlocksBytes();
                    var chunkEntities = chunk.Entities;
                    
                    Array.Copy(chunkData, 0, dataArray, arrayOffset + chunkIndex * AbstractChunk.ChunkBlocksByteLength, AbstractChunk.ChunkBlocksByteLength);
                    entities[chunkIndex] = chunkEntities;

                    chunkIndex++;
                }
            }
        }

        private void Generate(Range2 range)
        {
            _generatedRange = range;
            _chunks = new GeneratedChunk[range.Size.X, range.Size.Z];
            
            foreach (var stage in Stages)
            {
                stage.Generate(range, _chunks);
                if(_abortOperation)
                    return;
                _activeStage++;
            }
        }

        /// <summary>
        /// Stops generation
        /// </summary>
        public void Abort()
        {
            _abortOperation = true;
        }

        /// <summary>
        /// Gets a generated chunk. (Generates it if needed)
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public GeneratedChunk GetChunk(IntVector2 position)
        {
            if (_chunks != null && _generatedRange.Contains(position))
            {
                return _chunks[position.X - _generatedRange.Min.X, position.Y - _generatedRange.Min.Y];
            }

            // todo: need to generate a bigger range
            Generate(new Range2 { Min = position, Max = position + 1 });
            return GetChunk(position);
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
            _chunks = null;
        }
    }
}
