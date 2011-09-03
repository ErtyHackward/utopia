using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.Cubes;

namespace Utopia.Shared.World.Processors
{
    /// <summary>
    /// Sample world processor that creates simple flat world
    /// </summary>
    public class FlatWorldProcessor : IWorldProcessor
    {
        private int _totalChunks;
        private int _chunksDone;

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

        /// <summary>
        /// Starts generation process.
        /// </summary>
        public void Generate(Range2 generationRange, GeneratedChunk[,] chunks)
        {
            _totalChunks = generationRange.Count;
            _chunksDone = 0;
            generationRange.Foreach(pos =>
            {
                var chunk = chunks[pos.X - generationRange.Position.X, pos.Y - generationRange.Position.Y];

                var chunkBytes = new byte[AbstractChunk.ChunkBlocksByteLength];

                for (int y = 0; y < AbstractChunk.ChunkSize.Y; y++)
                {
                    for (int x = 0; x < AbstractChunk.ChunkSize.X; x++)
                    {
                        for (int z = 0; z < AbstractChunk.ChunkSize.Z; z++)
                        {
                            var index = x * AbstractChunk.ChunkSize.Y + y + z * AbstractChunk.ChunkSize.Y * AbstractChunk.ChunkSize.X;

                            if (y >= AbstractChunk.ChunkSize.Y / 2)
                                chunkBytes[index] = CubeId.Air;
                            else
                                chunkBytes[index] = CubeId.Stone;
                        }
                    }
                }

                chunk.BlockData.SetBlockBytes(chunkBytes);

                _chunksDone++;
            });
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
