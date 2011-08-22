using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Threading;
using System.Threading.Tasks;
using Utopia.Shared.World;
using Utopia.Shared.Chunks;

namespace Utopia.Worlds.Chunks.ChunkLandscape
{
    public class LandscapeManager : ILandscapeManager
    {
        #region Private variable
        private CreateLandScapeDelegate _createLandScapeDelegate;
        private delegate void CreateLandScapeDelegate(VisualChunk chunk);
        private WorldGenerator _worldGenerator;
        #endregion

        #region Public variables/properties
        public WorldGenerator WorldGenerator
        {
            get { return _worldGenerator; }
            set { _worldGenerator = value; }
        }
        #endregion

        public LandscapeManager()
        {
            Intialize();
            System.Threading.ThreadPool.SetMaxThreads(1, 1);
        }

        public void Dispose()
        {
        }

        #region Public methods
        //Create the landscape for the chunk
        public void CreateLandScape(VisualChunk chunk, bool Async)
        {
            //1) Request Server the chunk
            //2) If chunk is a "pure" chunk on the server, then generate it localy.
            //2b) If chunk is not pure, we will have received the data inside a "GeneratedChunk" that we will copy inside the big buffe array.

            if (Async)
            {
                chunk.ThreadStatus = ThreadStatus.Locked;
                Task.Factory.StartNew(() => createLandScape_threaded(chunk));
            }
            else
            {
                _createLandScapeDelegate.Invoke(chunk);
            }

        }
        #endregion

        #region Private methods
        private void Intialize()
        {
            _createLandScapeDelegate = new CreateLandScapeDelegate(createLandScape_threaded);
        }

        //Create the landscape for the chunk
        private void createLandScape_threaded(VisualChunk chunk)
        {
            GeneratedChunk generatedChunk = _worldGenerator.GetChunks(chunk.ChunkPosition);
            chunk.BlockData.SetBlockBytes(generatedChunk.BlockData.GetBlocksBytes());
            chunk.State = ChunkState.LandscapeLightsPropagated;
            chunk.ThreadStatus = ThreadStatus.Idle;
        }

        #endregion
    }
}
