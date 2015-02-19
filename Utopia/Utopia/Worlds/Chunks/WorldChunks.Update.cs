using System.Linq;
using Utopia.Entities.Managers;
using Utopia.Worlds.Chunks.ChunkWrapper;
using Utopia.Shared.Chunks;
using S33M3DXEngine.Threading;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using System;
using System.Diagnostics;
using Utopia.Shared.LandscapeEntities;
using Utopia.Action;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Worlds.Chunks
{

#if PERFTEST
    public class Perf
    {
        public long cubeChunkID;
        public bool Actif;
        public Stopwatch sw = new Stopwatch();
        public System.Collections.Generic.List<string> CollectedData = new System.Collections.Generic.List<string>();
        public void AddData(string data)
        {
            if (Actif) CollectedData.Add(data + " " + sw.ElapsedMilliseconds);
        }
    }
#endif

    public partial class WorldChunks : IWorldChunks2D
    {
#if PERFTEST
        public static Perf perf = new Perf();

#endif
        private int _chunkCreationTrigger;
        private Vector3D _lastPlayerTriggeredPosition;
        private Range3I _eventNotificationArea; //Area where the server is sending events, everything outside this Area won't received events
        private int _sliceValue = -1;

        #region public methods
        public override void FTSUpdate(GameTime timeSpend)
        {
            PlayerDisplacementChunkEvents();

            // make chunks pop Up
            for (int i = _transparentChunks.Count - 1; i >= 0; i--)
            {
                var transparentChunk = _transparentChunks[i];
                transparentChunk.PopUpValue.BackUpValue();
                transparentChunk.PopUpValue.Value -= 0.02f;
            }

            // slicing view of the chunk only if player is in God mode !
            if (PlayerManager is GodEntityManager)
            {
                SlicingUpdate(timeSpend);
            }
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            ChunkVisibilityTest();

            if (_camManager.ActiveCamera.WorldPosition.Value.Y < 400)
            {
                ChunkUpdateManager();
                CheckWrapping();
                SortChunks();
            }

            // Manage chunk PopUp !
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

            //Do interpolation on static entities from chunk (Case of when the entity does have an animation)
            for (int chunkIndice = 0; chunkIndice < SortedChunks.Length; chunkIndice++)
            {
                var chunk = SortedChunks[chunkIndice];
                if (chunk.Graphics.NeedToRender)
                {
                    if (chunk.DistanceFromPlayer > StaticEntityViewRange) 
                        continue;

                    foreach (var staticEntity in chunk.AllEntities())
                    {
                        staticEntity.VoxelEntity.ModelInstance.Interpolation(elapsedTime);
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
            int maximumUpdateOrderPossible = SortedChunks.Max(x => x.UpdateOrder);
            UpdateLeveled();
            ChunkResyncing();
            CreateNewChunk();
            PropagateOuterChunkLights();
            CreateChunkMeshes(maximumUpdateOrderPossible);
            SendMeshesToGC(maximumUpdateOrderPossible);
        }

        private void UpdateLeveled()
        {
            foreach (var chunk in ChunksToDraw(false))
            {
                if (chunk.Graphics.SliceValue != _sliceValue)
                {
                    chunk.Graphics.SliceValue = _sliceValue;
                    chunk.State = ChunkState.OuterLightSourcesProcessed;
                }
            }
        }

        private void ChunkResyncing()
        {
            //Process each chunk that are in Empty state, and not currently processed
            foreach (VisualChunk2D chunk in SortedChunks.Where(x => x.IsServerResyncMode && x.ThreadStatus == ThreadsManager.ThreadStatus.Idle))
            {
                VisualChunk2D localChunk = chunk;

                //Start chunk creation process in a threaded way !
                localChunk.ThreadStatus = ThreadsManager.ThreadStatus.Locked;           //Lock the thread before entering async process.
#if DEBUG
                localChunk.ThreadLockedBy = "ChunkResyncing";
#endif
                //SmartThread.ThreadPool.QueueWorkItem(ChunkCreationThreadedSteps_Threaded, chunk, WorkItemPriority.Normal);
                S33M3DXEngine.Threading.ThreadsManager.RunAsync(() => ChunkResyncing_Threaded(localChunk));
            }
        }

        private void ChunkResyncing_Threaded(VisualChunk2D chunk)
        {
            if (chunk.IsServerResyncMode) _landscapeManager.CreateLandScape(chunk);
            chunk.ThreadStatus = ThreadsManager.ThreadStatus.Idle;
        }

        //Will create new chunks based on chunks with state = Empty
        private void CreateNewChunk()
        {
            //Process each chunk that are in Empty state, and not currently processed
            foreach (VisualChunk2D chunk in SortedChunks.Where(x => (x.State == ChunkState.Empty || x.State == ChunkState.LandscapeCreated) && x.ThreadStatus == ThreadsManager.ThreadStatus.Idle))
            {
                VisualChunk2D localChunk = chunk;

                //Start chunk creation process in a threaded way !
                localChunk.ThreadStatus = ThreadsManager.ThreadStatus.Locked;           //Lock the thread before entering async process.
#if DEBUG
                localChunk.ThreadLockedBy = "CreateNewChunk";
#endif
                //SmartThread.ThreadPool.QueueWorkItem(ChunkCreationThreadedSteps_Threaded, chunk, WorkItemPriority.Normal);
                S33M3DXEngine.Threading.ThreadsManager.RunAsync(() => ChunkCreationThreadedSteps_Threaded(localChunk));
            }
        }

        //Chunk creation steps
        //The process done isnight this step is impacting only the chunk that need to be creation
        private void ChunkCreationThreadedSteps_Threaded(VisualChunk2D chunk)
        {

#if PERFTEST
            Utopia.Worlds.Chunks.WorldChunks.perf.AddData("ChunkCreationThreadedSteps_Threaded Started " + chunk.ChunkID);
#endif

            //Create the landscape, by updating the "Big Array" area under the chunk
            if (chunk.State == ChunkState.Empty) _landscapeManager.CreateLandScape(chunk);

            //Was my landscape Creation, if not it means that the chunk has been requested to the server, waiting for server answer
            if (chunk.State == ChunkState.LandscapeCreated)
            {
                //Create Inner chunk Light sources
                _lightingManager.CreateChunkLightSources(chunk);
                //Propagate Inner chunk LightSources
                _lightingManager.PropagateInnerChunkLightSources(chunk);
                //The Thread status will be ChunkState.InnerLightsSourcePropagated if going out of this
            }

            chunk.ThreadStatus = ThreadsManager.ThreadStatus.Idle;
        }

        //Will take the newly created chunks || the chunk that didn't had the outside light propagate (Border chunk), and propagate the light from the surroundings chunks
        private void PropagateOuterChunkLights()
        {
            //Process each chunk that are in InnerLightsSourcePropagated state, and not being currently processed
            foreach (VisualChunk2D chunk in SortedChunks.Where(x =>
                                                       (x.State == ChunkState.InnerLightsSourcePropagated || (x.IsOutsideLightSourcePropagated == false && x.IsBorderChunk == false && x.State >= ChunkState.InnerLightsSourcePropagated)) &&
                                                       x.ThreadStatus == ThreadsManager.ThreadStatus.Idle))
            {
                VisualChunk2D localChunk = chunk;

                //all the surrounding chunks must have had their LightSources Processed at minimum.
                if (localChunk.IsBorderChunk == true || localChunk.SurroundingChunksMinimumState(ChunkState.InnerLightsSourcePropagated))
                {
                    //Check if the surrounding chunk from this chunk are in the correct state = ChunkState.InnerLightsSourcePropagated
                    localChunk.ThreadStatus = ThreadsManager.ThreadStatus.Locked;           //Lock the thread before entering async process.
#if DEBUG
                    localChunk.ThreadLockedBy = "PropagateOuterChunkLights";
#endif
                    S33M3DXEngine.Threading.ThreadsManager.RunAsync(() => ChunkOuterLightPropagation_Threaded(localChunk));
                }
            }
        }

        //Will propagate light from chunk surrounding the current chunk
        private void ChunkOuterLightPropagation_Threaded(VisualChunk2D chunk)
        {
#if PERFTEST
            Utopia.Worlds.Chunks.WorldChunks.perf.AddData("ChunkOuterLightPropagation_Threaded Started " + chunk.ChunkID);
#endif

            _lightingManager.PropagateOutsideChunkLightSources(chunk);
            chunk.ThreadStatus = ThreadsManager.ThreadStatus.Idle;
            //The state will always be OuterLightSourcesProcessed, but the logic could not be implementated if the chunk was a Border chunk. In this case its 
            //IsOutsideLightSourcePropagated will be False, it will be process as soon as the chunk isBorderChunk change to false !
        }

        private void CreateChunkMeshes(int maximumUpdateOrderPossible)
        {
            //Process each chunk that are in IsOutsideLightSourcePropagated state, and not currently processed
            foreach (VisualChunk2D chunk in SortedChunks.Where(x => x.State == ChunkState.OuterLightSourcesProcessed && x.ThreadStatus == ThreadsManager.ThreadStatus.Idle))
            {
                VisualChunk2D localChunk = chunk;
                if (maximumUpdateOrderPossible > 0 && localChunk.UpdateOrder == 0) continue;

                //all the surrounding chunks must have had their LightSources Processed at minimum.
                if (localChunk.SurroundingChunksMinimumState(ChunkState.OuterLightSourcesProcessed))
                {
                    localChunk.ThreadStatus = ThreadsManager.ThreadStatus.Locked;
#if DEBUG
                    localChunk.ThreadLockedBy = "CreateChunkMeshes";
#endif
                    S33M3DXEngine.Threading.ThreadsManager.RunAsync(() => CreateChunkMeshes_Threaded(localChunk), localChunk.UpdateOrder > 0 ? ThreadsManager.ThreadTaskPriority.High : ThreadsManager.ThreadTaskPriority.Normal);
                }
            }
        }

        //Will create the chunk buffer, ready to be sent to the GC
        private void CreateChunkMeshes_Threaded(VisualChunk2D chunk)
        {
#if PERFTEST
            Utopia.Worlds.Chunks.WorldChunks.perf.AddData("CreateChunkMeshes_Threaded Started  " + chunk.ChunkID);
#endif

            //The chunk surrounding me must all have their landscape created !
            _chunkMeshManager.CreateChunkMesh(chunk);
            chunk.ThreadStatus = ThreadsManager.ThreadStatus.Idle;
        }

        //Sending newly created Mesh to the GC, making the change visible, this is NOT threadsafe, the chunk must be done one by one.
        //This could lead to "lag" if too many chunks are sending their new mesh to the GC at the same time (during the same frame)
        //Maybe it will be worth to check is a limit of chunk by update must be placed (But this will slow down chunk creation time)
        private void SendMeshesToGC(int maximumUpdateOrderPossible)
        {
            int nbrchunksSend2GC = 0;            

            //Process each chunk that are in IsOutsideLightSourcePropagated state, and not currently processed
            foreach (VisualChunk2D chunk in SortedChunks.Where(x => x.State == ChunkState.MeshesChanged &&
                                                             x.ThreadStatus == ThreadsManager.ThreadStatus.Idle &&
                                                             x.UpdateOrder == maximumUpdateOrderPossible))
            {
                chunk.UpdateOrder = 0;
                chunk.Graphics.SendCubeMeshesToBuffers();
                nbrchunksSend2GC++;
#if PERFTEST
                if (chunk.ChunkID == Utopia.Worlds.Chunks.WorldChunks.perf.cubeChunkID)
                {
                    Utopia.Worlds.Chunks.WorldChunks.perf.AddData("FINISHED");
                    Utopia.Worlds.Chunks.WorldChunks.perf.Actif = false;

                    foreach (var data in Utopia.Worlds.Chunks.WorldChunks.perf.CollectedData)
                    {
                        Console.WriteLine(data);
                    }
                }
                else Utopia.Worlds.Chunks.WorldChunks.perf.AddData("SendMeshesToGC Started  " + chunk.ChunkID);
#endif

                if (nbrchunksSend2GC > VisualWorldParameters.VisibleChunkInWorld.X) break;
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
                chunk.DistanceFromPlayer = MVector3.Distance2D(chunk.ChunkCenter, PlayerManager.CameraWorldPosition);
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
            double distance = MVector3.Distance2D(_lastPlayerTriggeredPosition, PlayerManager.Player.Position);
            //Triggered when player has move a distance of 8 blocks (half chunk distance)
            if (distance > (AbstractChunk.ChunkSize.X / 2d))
            {
                var newEventNotificationArea = new Range3I
                {
                    Position = BlockHelper.EntityToChunkPosition(PlayerManager.Player.Position) - _eventNotificationArea.Size / 2,
                    Size = _eventNotificationArea.Size
                };

                var chunks2Syncro = newEventNotificationArea.AllExclude(_eventNotificationArea);
                if (chunks2Syncro != null)
                {
                    bool synchroFullyRequested = true;
                    //Get all new chunk in the area that are in a state ready to be requested !
                    //Check that the concerned chunks are in a correct state to be requested.

                    foreach (var chunkPosition in chunks2Syncro)
                    {
                        if (ResyncChunk(chunkPosition, false) == false)
                        {
                            synchroFullyRequested = false;
                            break;
                        }
                    }
                    if (synchroFullyRequested) _eventNotificationArea = newEventNotificationArea;
                }

                _lastPlayerTriggeredPosition = PlayerManager.Player.Position;
                ChunkNeed2BeSorted = true;
            }
        }

        #region Update WRAPPING
        private void CheckWrapping()
        {
            if (SortedChunks.Count(x => x.State != ChunkState.DisplayInSyncWithMeshes) > 0) return;

            // Get World Border line ! => Highest and lowest X et Z chunk components
            //Compute Player position against WorldRange
            var resultmin = new Vector3D(PlayerManager.Player.Position.X - VisualWorldParameters.WorldRange.Position.X,
                                         PlayerManager.Player.Position.Y - VisualWorldParameters.WorldRange.Position.Y,
                                         PlayerManager.Player.Position.Z - VisualWorldParameters.WorldRange.Position.Z);

            var resultmax = new Vector3D(VisualWorldParameters.WorldRange.Max.X - PlayerManager.Player.Position.X,
                                         VisualWorldParameters.WorldRange.Max.Y - PlayerManager.Player.Position.Y,
                                         VisualWorldParameters.WorldRange.Max.Z - PlayerManager.Player.Position.Z);

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


        //Landscape chunk Slicing visualization Handling
        private void SlicingUpdate(GameTime timeSpend)
        {
            if (SortedChunks.Count(x => x.State != ChunkState.DisplayInSyncWithMeshes) > 0) return;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.LandscapeSlicerUp))
            {
                if (_sliceValue == -1) _sliceValue = (int)_camManager.ActiveCamera.WorldPosition.ValueInterp.Y;
                _sliceValue++;
            }

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.LandscapeSlicerDown))
            {
                if (_sliceValue == -1) _sliceValue = (int)_camManager.ActiveCamera.WorldPosition.ValueInterp.Y;
                _sliceValue--;
            }

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.LandscapeSlicerOff))
            {
                _sliceValue = -1;
            }
        }
        
        #endregion
    }
}
