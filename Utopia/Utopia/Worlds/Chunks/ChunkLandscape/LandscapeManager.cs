using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Threading;

namespace Utopia.Worlds.Chunks.ChunkLandscape
{
    public class LandscapeManager : ILandscapeManager
    {
        #region Private variable
        private CreateLandScapeDelegate createLandScapeDelegate;
        #endregion

        #region Public variables/properties
        private delegate void CreateLandScapeDelegate(VisualChunk chunk);
        #endregion

        public LandscapeManager()
        {
            Intialize();
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

            //Run landscape Async mode
            if (Async)
            {
                chunk.ThreadStatus = ThreadStatus.Locked;
                createLandScapeDelegate.BeginInvoke(chunk, null, null);
            }
            else
            {
                createLandScapeDelegate.Invoke(chunk);
            }


            //if (Async)
            //{
            //    WorkQueue.DoWorkInThread(new Amib.Threading.WorkItemCallback(createLandScape_threaded2), chunk, chunk as IThreadStatus, chunk.ThreadPriority);
            //}
            //else
            //{
            //    createLandScapeDelegate.Invoke(chunk);
            //}

        }
        #endregion

        #region Private methods
        private void Intialize()
        {
            createLandScapeDelegate = new CreateLandScapeDelegate(createLandScape_threaded);
        }

        //Create the landscape for the chunk
        private object createLandScape_threaded2(object chunk)
        {
            System.Threading.Thread.Sleep(10);
            ((VisualChunk)chunk).ThreadStatus = ThreadStatus.Idle;

            return null;
        }

        //Create the landscape for the chunk
        private void createLandScape_threaded(VisualChunk chunk)
        {
            System.Threading.Thread.Sleep(10);
            chunk.ThreadStatus = ThreadStatus.Idle;
        }
        #endregion
    }
}
