﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.Chunks.ChunkWrapper;
using System.Threading;
using Utopia.Shared.Chunks;
using Amib.Threading;
using S33M3DXEngine.Threading;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;

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
        public override void Update(GameTime timeSpend)
        {
            if (_camManager.ActiveCamera.WorldPosition.Y < 400)
            {
                ChunkUpdateManager();
                CheckWrapping();     // Handle Playerzz impact on Terra (Mainly the location will trigger chunk creation/destruction)
                SortChunks();
            }

            // make chunks appear slowly and not hurt the eyes
            for (int i = _transparentChunks.Count - 1; i >= 0; i--)
            {
                var transparentChunk = _transparentChunks[i];
                transparentChunk.Opaque += 2f * timeSpend.ElapsedGameTimeInS_LD;
                if (transparentChunk.Opaque >= 1)
                {
                    transparentChunk.Opaque = 1;
                    _transparentChunks.RemoveAt(i);
                }
            }
        }

        #endregion

        #region private method

        private void IntilializeUpdateble()
        {
            _chunkCreationTrigger = (VisualWorldParameters.WorldVisibleSize.X / 2) - (1 * AbstractChunk.ChunkSize.X);
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

            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];

                if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                if (chunk.State == ChunkState.Empty)
                {
                    _landscapeManager.CreateLandScape(chunk, SmartThread.ThreadingActif);
                }
            }
        }

        private void ProcessChunks_LandscapeCreated()
        {
            VisualChunk chunk;

            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];
                if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                if (chunk.State == ChunkState.LandscapeCreated ||
                    chunk.State == ChunkState.UserChanged)
                {
                    _lightingManager.CreateChunkLightSources(chunk, SmartThread.ThreadingActif);
                }
            }
        }

        // Syncronisation STEP !!! ==> No previous state pending job possible !! Wait for them all to be finished !
        private void ProcessChunks_LandscapeLightsSourceCreated()
        {
            processInsync = isUpdateInSync(ChunksThreadSyncMode.UpdateReadyForLightPropagation);

            VisualChunk chunk;
            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];
                if (processInsync || chunk.ThreadPriority == WorkItemPriority.Highest)
                {
                    if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                    if (chunk.State == ChunkState.LandscapeLightsSourceCreated)
                    {
                        _lightingManager.PropagateChunkLightSources(chunk, SmartThread.ThreadingActif);
                    }
                }
            }
        }

        // Syncronisation STEP !!! ==> No previous state pending job possible !! Wait for them all to be finished !
        private void ProcessChunks_LandscapeLightsPropagated()
        {
            processInsync = isUpdateInSync(ChunksThreadSyncMode.UpdateReadyForMeshCreation);

            VisualChunk chunk;
            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];
                if (processInsync || chunk.ThreadPriority == WorkItemPriority.Highest)
                {
                    if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                    if (chunk.State == ChunkState.LandscapeLightsPropagated)
                    {
                        _chunkMeshManager.CreateChunkMesh(chunk, SmartThread.ThreadingActif);
                    }
                }
            }
        }

        int userOrder = 0;
        private void ProcessChunks_MeshesChanged()
        {
            userOrder = CheckUserModifiedChunks();
            bool lowpriorityThreadChunkAlreadyProcessed = false;
            VisualChunk chunk;
            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];
                if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                if (chunk.UserChangeOrder != userOrder && chunk.ThreadPriority == WorkItemPriority.Highest)
                { //If this thread is user changed
                    continue;
                }

                if (lowpriorityThreadChunkAlreadyProcessed && chunk.ThreadPriority != WorkItemPriority.Highest) continue;

                if (chunk.State == ChunkState.MeshesChanged)
                {
                    if (chunk.ThreadPriority != WorkItemPriority.Highest)
                    {
                        lowpriorityThreadChunkAlreadyProcessed = true;
                    }
                    chunk.UserChangeOrder = 0;
                    chunk.ThreadPriority = WorkItemPriority.Normal;
                    //If Executed in a thread ==> Must be executed on a deferred context, with a Replay system in place.
                    chunk.SendCubeMeshesToBuffers();
                }
            }
        }

        private int CheckUserModifiedChunks()
        {
            VisualChunk chunk;
            int nbrUserThreads = 0;
            int lowestOrder = int.MaxValue;
            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                chunk = SortedChunks[chunkIndice];
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
                    for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
                    {
                        chunk = SortedChunks[chunkIndice];
                        if (chunk.State == ChunkState.Empty ||
                            chunk.State == ChunkState.LandscapeCreated || 
                            chunk.State == ChunkState.UserChanged)
                        {
                            inSync = false;
                            break;
                        }
                    }
                    break;
                case ChunksThreadSyncMode.UpdateReadyForMeshCreation:
                    for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
                    {
                        chunk = SortedChunks[chunkIndice];
                        if (chunk.State == ChunkState.Empty ||
                            chunk.State == ChunkState.LandscapeCreated ||
                            chunk.State == ChunkState.LandscapeLightsSourceCreated || 
                            chunk.State == ChunkState.UserChanged)
                        {
                            inSync = false;
                            break;
                        }
                    }
                    break;
                case ChunksThreadSyncMode.HighPriorityReadyToBeSendToGraphicalCard:
                    nbrThread = 0;
                    for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
                    {
                        chunk = SortedChunks[chunkIndice];
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
                    for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
                    {
                        chunk = SortedChunks[chunkIndice];
                        if (chunk.State != ChunkState.DisplayInSyncWithMeshes || chunk.ThreadStatus == ThreadStatus.Locked)
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
            if (!ChunkNeed2BeSorted || _camManager.ActiveCamera == null) return;
            int index = 0;

            foreach (var chunk in Chunks.OrderBy(x => MVector3.DistanceSquared(x.ChunkCenter, _playerManager.CameraWorldPosition)))
            {
                SortedChunks[index] = chunk;
                index++;
            }
            ChunkNeed2BeSorted = false;
        }

        #region Update WRAPPING
        private void CheckWrapping()
        {
            if (!isUpdateInSync(ChunksThreadSyncMode.ReadyForWrapping)) return;

            // Get World Border line ! => Highest and lowest X et Z chunk components
            //Compute Player position against WorldRange
            var resultmin = new Vector3D(_playerManager.Player.Position.X - VisualWorldParameters.WorldRange.Position.X,
                                        _playerManager.Player.Position.Y - VisualWorldParameters.WorldRange.Position.Y,
                                        _playerManager.Player.Position.Z - VisualWorldParameters.WorldRange.Position.Z);

            var resultmax = new Vector3D(VisualWorldParameters.WorldRange.Max.X - _playerManager.Player.Position.X,
                                        VisualWorldParameters.WorldRange.Max.Y - _playerManager.Player.Position.Y,
                                        VisualWorldParameters.WorldRange.Max.Z - _playerManager.Player.Position.Z);

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

                _chunkWrapper.AddWrapOperation(operation);

            }
        }
        #endregion

        #endregion
    }
}
