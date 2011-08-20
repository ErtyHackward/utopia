using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.Maths;
using S33M3Engines.Threading;
using Amib.Threading;

namespace Utopia.Worlds.Chunks
{
    public partial class WorldChunks : IWorldChunks
    {
        #region Private variables
        private bool processInsync;
        private int _chunkCreationTrigger;
        #endregion

        #region Public variables/properties
        #endregion

        #region public methods
        public void Update(ref GameTime TimeSpend)
        {
            if (_camManager.ActiveCamera.WorldPosition.Y < 400)
            {
                ChunkUpdateManager();
                if (!_gameStates.DebugActif) CheckWrapping();     // Handle Playerzz impact on Terra (Mainly the location will trigger chunk creation/destruction)
                SortChunks();
            }
        }

        public void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region private method

        private void IntilializeUpdateble()
        {
            _chunkCreationTrigger = (VisibleWorldSize.X / 2) - (1 * WorldParameters.ChunkSize.X);
        }

        private void ChunkUpdateManager()
        {
            ProcessChunks_Empty();
            ProcessChunks_LandscapeCreated();
            ProcessChunks_LandscapeLightsSourceCreated();
            ProcessChunks_LandscapeLightsPropagated();
            ProcessChunks_MeshesChanged();
        }

        private void ProcessChunks_Empty()
        {
            VisualChunk chunk;

            for (int chunkIndice = 0; chunkIndice < Chunks.Length; chunkIndice++)
            {
                chunk = Chunks[chunkIndice];
                if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                if (chunk.State == ChunkState.Empty)
                {
                    WorkQueue.DoWorkInThread(new WorkItemCallback(chunk.CreateLandScape_Threaded), null, chunk as IThreadStatus, chunk.ThreadPriority);
                }
            }
        }

        private void ProcessChunks_LandscapeCreated()
        {
            VisualChunk chunk;

            for (int chunkIndice = 0; chunkIndice < Chunks.Length; chunkIndice++)
            {
                chunk = Chunks[chunkIndice];
                if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                if (chunk.State == ChunkState.LandscapeCreated ||
                    chunk.State == ChunkState.UserChanged)
                {
                    WorkQueue.DoWorkInThread(new WorkItemCallback(chunk.CreateLightingSources_Threaded), null, chunk as IThreadStatus, chunk.ThreadPriority);
                }
            }
        }

        // Syncronisation STEP !!! ==> No previous state pending job possible !! Wait for them all to be finished !
        private void ProcessChunks_LandscapeLightsSourceCreated()
        {
            processInsync = isUpdateInSync(ChunksThreadSyncMode.UpdateReadyForLightPropagation);

            VisualChunk chunk;
            for (int chunkIndice = 0; chunkIndice < Chunks.Length; chunkIndice++)
            {
                chunk = Chunks[chunkIndice];
                if (processInsync || chunk.ThreadPriority == WorkItemPriority.Highest)
                {
                    if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                    if (chunk.State == ChunkState.LandscapeLightsSourceCreated)
                    {
                        WorkQueue.DoWorkInThread(new WorkItemCallback(chunk.PropagateLights_Threaded), null, chunk as IThreadStatus, chunk.ThreadPriority);
                    }
                }
            }
        }

        // Syncronisation STEP !!! ==> No previous state pending job possible !! Wait for them all to be finished !
        private void ProcessChunks_LandscapeLightsPropagated()
        {
            processInsync = isUpdateInSync(ChunksThreadSyncMode.UpdateReadyForMeshCreation);

            VisualChunk chunk;
            for (int chunkIndice = 0; chunkIndice < Chunks.Length; chunkIndice++)
            {
                chunk = Chunks[chunkIndice];
                if (processInsync || chunk.ThreadPriority == WorkItemPriority.Highest)
                {
                    if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                    if (chunk.State == ChunkState.LandscapeLightsPropagated)
                    {
                        WorkQueue.DoWorkInThread(new WorkItemCallback(chunk.CreateCubeMeshes_Threaded), null, chunk as IThreadStatus, chunk.ThreadPriority);
                    }
                }
            }
        }

        int userOrder = 0;
        private void ProcessChunks_MeshesChanged()
        {
            userOrder = CheckUserModifiedChunks();

            VisualChunk chunk;
            for (int chunkIndice = 0; chunkIndice < Chunks.Length; chunkIndice++)
            {
                chunk = Chunks[chunkIndice];
                if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                if (chunk.UserChangeOrder != userOrder && chunk.ThreadPriority == WorkItemPriority.Highest)
                { //If this thread is user changed, but 
                    continue;
                }

                if (chunk.State == ChunkState.MeshesChanged)
                {
                    //Console.WriteLine(chunk.UserChangeOrder);

                    chunk.UserChangeOrder = 0;
                    chunk.ThreadPriority = WorkItemPriority.Normal;
                    //Si exécuté dans un thread => Doit fonctionner avec des device context deffered, avec un system de replay (Pas encore testé !!)
                    chunk.SendCubeMeshesToBuffers();

                    //if (WorkQueue.ThreadingActif) WorkQueue.DoWorkInThread(new WorkItemCallback(chunk.SendCubeMeshesToBuffers_Threaded), new Amib.Threading.Internal.WorkItemParam(), chunk as IThreadStatus, chunk.Priority, false);
                    //else chunk.SendCubeMeshesToBuffers(0);
                }
            }
        }


        private int CheckUserModifiedChunks()
        {
            VisualChunk chunk;
            int nbrUserThreads = 0;
            int lowestOrder = int.MaxValue;
            for (int chunkIndice = 0; chunkIndice < Chunks.Length; chunkIndice++)
            {
                chunk = Chunks[chunkIndice];
                if (chunk.ThreadPriority == WorkItemPriority.Highest && chunk.State != ChunkState.DisplayInSyncWithMeshes)
                    nbrUserThreads++;
                if (chunk.State == ChunkState.MeshesChanged && chunk.ThreadPriority == WorkItemPriority.Highest)
                {
                    if (chunk.UserChangeOrder < lowestOrder && chunk.UserChangeOrder > 0) lowestOrder = chunk.UserChangeOrder;
                    nbrUserThreads--;
                }
            }

            if (nbrUserThreads == 0) //All my threads are ready to render !
            {
                return lowestOrder;
            }
            else return 0;
        }

        private bool isUpdateInSync(ChunksThreadSyncMode syncMode)
        {
            VisualChunk chunk;
            bool inSync = true;
            int nbrThread;
            switch (syncMode)
            {
                case ChunksThreadSyncMode.UpdateReadyForLightPropagation:
                    for (int chunkIndice = 0; chunkIndice < Chunks.Length; chunkIndice++)
                    {
                        chunk = Chunks[chunkIndice];
                        if (chunk.State == ChunkState.Empty ||
                            chunk.State == ChunkState.LandscapeCreated || chunk.State == ChunkState.UserChanged)
                        {
                            inSync = false;
                            break;
                        }
                    }
                    break;
                case ChunksThreadSyncMode.UpdateReadyForMeshCreation:
                    for (int chunkIndice = 0; chunkIndice < Chunks.Length; chunkIndice++)
                    {
                        chunk = Chunks[chunkIndice];
                        if (chunk.State == ChunkState.Empty ||
                            chunk.State == ChunkState.LandscapeCreated ||
                            chunk.State == ChunkState.LandscapeLightsSourceCreated || chunk.State == ChunkState.UserChanged)
                        {
                            inSync = false;
                            break;
                        }
                    }
                    break;
                case ChunksThreadSyncMode.HighPriorityReadyToBeSendToGraphicalCard:
                    nbrThread = 0;
                    for (int chunkIndice = 0; chunkIndice < Chunks.Length; chunkIndice++)
                    {
                        chunk = Chunks[chunkIndice];
                        if (chunk.ThreadPriority == WorkItemPriority.Highest) nbrThread++;
                        if (chunk.State == ChunkState.MeshesChanged && chunk.ThreadPriority == WorkItemPriority.Highest)
                        {
                            nbrThread--;
                            break;
                        }
                        inSync = nbrThread == 0;
                    }
                    break;
                case ChunksThreadSyncMode.ReadyForWrapping:
                    for (int chunkIndice = 0; chunkIndice < Chunks.Length; chunkIndice++)
                    {
                        chunk = Chunks[chunkIndice];
                        if (chunk.State != ChunkState.DisplayInSyncWithMeshes)
                        {
                            inSync = false;
                            break;
                        }
                    }
                    break;
            }

            return inSync;
        }

        /// <summary>
        /// Sort the chunks array if needed
        /// </summary>
        private void SortChunks()
        {
            if (!_chunkNeed2BeSorted || _camManager.ActiveCamera == null) return;
            int index = 0;

            foreach (var chunk in Chunks.OrderBy(x => MVector3.Distance(x.CubeRange.Min, _camManager.ActiveCamera.WorldPosition)))
            {
                SortedChunks[index] = chunk;
                index++;
            }

            _chunkNeed2BeSorted = false;
        }

        #region Update WRAPPING
        private void CheckWrapping()
        {
            
            if (!isUpdateInSync(ChunksThreadSyncMode.ReadyForWrapping)) return;

            // Get World Border line ! => Highest and lowest X et Z chunk components
            //Compute Player position against WorldRange
            var resultmin = new DVector3(_player.WorldPosition.Value.X - WorldRange.Min.X,
                                        _player.WorldPosition.Value.Y - WorldRange.Min.Y,
                                        _player.WorldPosition.Value.Z - WorldRange.Min.Z);

            var resultmax = new DVector3(WorldRange.Max.X - _player.WorldPosition.Value.X,
                                        WorldRange.Max.Y - _player.WorldPosition.Value.Y,
                                        WorldRange.Max.Z - _player.WorldPosition.Value.Z);

            float wrapOrder = float.MaxValue;
            ChunkWrapType operation = ChunkWrapType.Z_Plus1;
            //FindLowest value !

            if (_chunkCreationTrigger > resultmin.X || _chunkCreationTrigger > resultmin.Z ||
                _chunkCreationTrigger > resultmax.X || _chunkCreationTrigger > resultmax.Z)
            {

                if (resultmin.X < wrapOrder)
                {
                    wrapOrder = (float)resultmin.X; operation = ChunkWrapType.X_Minus1;
                }

                if (resultmin.Z < wrapOrder)
                {
                    wrapOrder = (float)resultmin.Z; operation = ChunkWrapType.Z_Minus1;
                }

                if (resultmax.X < wrapOrder)
                {
                    wrapOrder = (float)resultmax.X; operation = ChunkWrapType.X_Plus1;
                }

                if (resultmax.Z < wrapOrder)
                {
                    wrapOrder = (float)resultmax.Z; operation = ChunkWrapType.Z_Plus1;
                }

                ChunkWrapper.AddWrapOperation(operation);

            }
        }
        #endregion

        #endregion
    }
}
