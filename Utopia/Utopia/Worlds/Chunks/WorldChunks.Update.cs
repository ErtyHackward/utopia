using System;
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
        private int _chunkCreationTrigger;
        private Vector3D _lastPlayerTriggeredPosition;
        #endregion

        #region Public variables/properties
        #endregion

        #region public methods
        public override void Update(GameTime timeSpend)
        {
            PlayerDisplacementChunkEvents();

            // make chunks pop Up
            for (int i = _transparentChunks.Count - 1; i >= 0; i--)
            {
                var transparentChunk = _transparentChunks[i];
                transparentChunk.PopUpValue.BackUpValue();
                transparentChunk.PopUpValue.Value -= 0.02f;
            }
        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            if (_camManager.ActiveCamera.WorldPosition.Value.Y < 400)
            {
                ChunkUpdateManager();
                CheckWrapping();
                SortChunks();
            }

            // make chunks appear slowly and not hurt the eyes
            for (int i = _transparentChunks.Count - 1; i >= 0; i--)
            {
                var transparentChunk = _transparentChunks[i];

                transparentChunk.PopUpValue.ValueInterp = MathHelper.Lerp(transparentChunk.PopUpValue.ValuePrev, transparentChunk.PopUpValue.Value, interpolationLd);

                if (transparentChunk.PopUpValue.ValueInterp <= 0)
                {
                    transparentChunk.PopUpValue.ValueInterp = 0;
                    _transparentChunks.RemoveAt(i);
                }
            }

            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                var chunk = SortedChunks[chunkIndice];
                if (chunk.isExistingMesh4Drawing)
                {
                    foreach (var pair in chunk.VisualVoxelEntities)
                    {
                        foreach (var staticEntity in pair.Value)
                        {
                            staticEntity.VoxelEntity.ModelInstance.Interpolation(elapsedTime);
                        }
                    }
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
            CreateNewChunk();
            PropagateOuterChunkLights();
            CreateChunkMeshes();
            SendMeshesToGC();
        }

        //Will create new chunks based on chunks with state = Empty
        private void CreateNewChunk()
        {
            //Process each chunk that are in Empty state, and not currently processed
            foreach (VisualChunk chunk in SortedChunks.Where(x => (x.State == ChunkState.Empty ||x.State == ChunkState.LandscapeCreated) && x.ThreadStatus == ThreadStatus.Idle))
            {
                //Start chunk creation process in a threaded way !
                chunk.ThreadStatus = ThreadStatus.Locked;           //Lock the thread before entering async process.
                SmartThread.ThreadPool.QueueWorkItem(ChunkCreationThreadedSteps_Threaded, chunk, WorkItemPriority.Normal);
            }
        }

        //Chunk creation steps
        //The process done isnight this step is impacting only the chunk that need to be creation
        private void ChunkCreationThreadedSteps_Threaded(VisualChunk chunk)
        {
            //Create the landscape, by updating the "Big Array" area under the chunk
            if(chunk.State == ChunkState.Empty) _landscapeManager.CreateLandScape(chunk);

            //Was my landscape Creation, if not it means that the chunk has been requested to the server, waiting for server answer
            if (chunk.State == ChunkState.LandscapeCreated)
            {
                //Create Inner chunk Light sources
                _lightingManager.CreateChunkLightSources(chunk);
                //Propagate Inner chunk LightSources
                _lightingManager.PropagateInnerChunkLightSources(chunk);
                //The Thread status will be ChunkState.InnerLightsSourcePropagated if going out of this
            }

            chunk.ThreadStatus = ThreadStatus.Idle;
        }

        //Will take the newly created chunks || the chunk that didn't had the outside light propagate (Border chunk), and propagate the light from the surroundings chunks
        private void PropagateOuterChunkLights()
        {
            //Process each chunk that are in InnerLightsSourcePropagated state, and not being currently processed
            foreach (VisualChunk chunk in SortedChunks.Where(x =>
                                                       (x.State == ChunkState.InnerLightsSourcePropagated || (x.IsOutsideLightSourcePropagated == false && x.IsBorderChunk == false && x.State >= ChunkState.InnerLightsSourcePropagated)) && 
                                                       x.ThreadStatus == ThreadStatus.Idle))
            {
                //all the surrounding chunks must have had their LightSources Processed at minimum.
                if (chunk.IsBorderChunk == true || chunk.SurroundingChunksMinimumState(ChunkState.InnerLightsSourcePropagated))
                {
                    //Check if the surrounding chunk from this chunk are in the correct state = ChunkState.InnerLightsSourcePropagated
                    chunk.ThreadStatus = ThreadStatus.Locked;           //Lock the thread before entering async process.
                    SmartThread.ThreadPool.QueueWorkItem(ChunkOuterLightPropagation_Threaded, chunk, WorkItemPriority.Normal);
                }
            }
        }

        //Will propagate light from chunk surrounding the current chunk
        private void ChunkOuterLightPropagation_Threaded(VisualChunk chunk)
        {
            _lightingManager.PropagateOutsideChunkLightSources(chunk);
            chunk.ThreadStatus = ThreadStatus.Idle;
            //The state will always be OuterLightSourcesProcessed, but the logic could not be implementated if the chunk was a Border chunk. In this case its 
            //IsOutsideLightSourcePropagated will be False, it will be process as soon as the chunk isBorderChunk change to false !
        }

        private void CreateChunkMeshes()
        {
            //Process each chunk that are in IsOutsideLightSourcePropagated state, and not currently processed
            foreach (VisualChunk chunk in SortedChunks.Where(x => x.State == ChunkState.OuterLightSourcesProcessed && x.ThreadStatus == ThreadStatus.Idle))
            {
                //all the surrounding chunks must have had their LightSources Processed at minimum.
                if (chunk.SurroundingChunksMinimumState(ChunkState.OuterLightSourcesProcessed))
                {
                    chunk.ThreadStatus = ThreadStatus.Locked; 
                    SmartThread.ThreadPool.QueueWorkItem(CreateChunkMeshes_Threaded, chunk, WorkItemPriority.Normal);
                }
            }
        }

        //Will create the chunk buffer, ready to be sent to the GC
        private void CreateChunkMeshes_Threaded(VisualChunk chunk)
        {
            //The chunk surrounding me must all have their landscape created !
            _chunkMeshManager.CreateChunkMesh(chunk);
            chunk.ThreadStatus = ThreadStatus.Idle;
        }

        //Sending newly created Mesh to the GC, making the change visible, this is NOT threadsafe, the chunk must be done one by one.
        //This could lead to "lag" if too many chunks are sending their new mesh to the GC at the same time (during the same frame)
        //Maybe it will be worth to check is a limit of chunk by update must be placed (But this will slow down chunk creation time)
        private void SendMeshesToGC()
        {
            int maximumUpdateOrderPossible = SortedChunks.Max(x => x.UpdateOrder);
            //Process each chunk that are in IsOutsideLightSourcePropagated state, and not currently processed
            foreach (VisualChunk chunk in SortedChunks.Where(x => x.State == ChunkState.MeshesChanged && 
                                                             x.ThreadStatus == ThreadStatus.Idle &&
                                                             x.UpdateOrder == maximumUpdateOrderPossible))
            {
                chunk.UpdateOrder = 0;
                chunk.SendCubeMeshesToBuffers();
            }
        }

        /// <summary>
        /// Sort the chunks array if needed
        /// </summary>
        private void SortChunks()
        {
            if (!ChunkNeed2BeSorted || _camManager.ActiveCamera == null) return;

            //Compute Distance Squared from Chunk Center to Camera
            foreach (var chunk in Chunks)
            {
                chunk.DistanceFromPlayer = MVector3.Distance2D(chunk.ChunkCenter, _playerManager.CameraWorldPosition);
            }

            //Sort by this distance
            int index = 0;
            foreach (var chunk in Chunks.OrderBy(x => x.DistanceFromPlayer))
            {
                SortedChunks[index] = chunk;
                index++;
            }
            ChunkNeed2BeSorted = false;
        }

        private void PlayerDisplacementChunkEvents()
        {
            double distance = MVector3.Distance2D(_lastPlayerTriggeredPosition, _playerManager.Player.Position);
            if(distance > 8){
                _lastPlayerTriggeredPosition = _playerManager.Player.Position;
                ChunkNeed2BeSorted = true;
            }
        }

        #region Update WRAPPING
        private void CheckWrapping()
        {
            if(SortedChunks.Count(x => x.State != ChunkState.DisplayInSyncWithMeshes) > 0) return;

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
