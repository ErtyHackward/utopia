using System;
using System.Collections.Generic;
using System.Linq;
using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.World
{
    /// <summary>
    /// Represents a highest level world generator.
    /// This class organize work of any world-generation related classes
    /// </summary>
    public class WorldGenerator
    {
        private bool _abortOperation;
        private delegate void GenerateDelegate(Range2 range);
        private int _activeStage;

        /// <summary>
        /// Gets or sets current world designer
        /// </summary>
        public WorldParameters WorldParametes { get; set; }

        /// <summary>
        /// Gets a generation stages manager
        /// </summary>
        public WorldGenerationStagesManager Stages { get; private set; }

        /// <summary>
        /// Gets generators chunks
        /// </summary>
        public Dictionary<IntVector2, GeneratedChunk> Chunks { get; private set; }

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
        
        private void Generate(Range2 range)
        {
            // first lets check do we have all requestd chunks or not
            if (range.All(pos => Chunks.ContainsKey(pos)))
            {
                // no need to perform any further actions
                return;
            }

            // todo: add initialization code of the chunks here after designing of the 

            foreach (var stage in Stages)
            {
                stage.Generate(this, range);
                if(_abortOperation)
                    return;
                _activeStage++;
            }
        }

        /// <summary>
        /// Stops generation as fast as possible
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
            if (Chunks.ContainsKey(position))
            {
                return Chunks[position];
            }

            // todo: need to generate bigger range
            Generate(new Range2 { Min = position, Max = position + 1 });
            return Chunks[position];
            

        }
    }
}
