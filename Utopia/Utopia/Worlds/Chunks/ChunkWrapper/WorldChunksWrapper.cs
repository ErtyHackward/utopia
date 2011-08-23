using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;
using Utopia.Shared.Chunks;

namespace Utopia.Worlds.Chunks.ChunkWrapper
{
    public enum ChunkWrapType
    {
        X_Plus1,
        X_Minus1,
        Z_Plus1,
        Z_Minus1
    }

    public enum ChunkWrapperStatus
    {
        Idle,
        ProcessingNewWrap,
        WrapPostWrapWork
    }

    public class WorldChunksWrapper : IChunksWrapper
    {
        #region Private variables
        private ChunkWrapType _processingType;
        #endregion

        #region Public variables
        public IWorldChunks WorldChunks { get; set; }
        #endregion

        public WorldChunksWrapper()
        {
        }
        #region Private methods
        //Force some chunks to be refresh (not reconstructed)
        private void PostWrappingStep()
        {
            switch (_processingType)
            {
                case ChunkWrapType.X_Plus1:

                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedX(WorldChunks.WorldRange.Max.X - (AbstractChunk.ChunkSize.X * 2), WorldChunks.WorldRange.Min.Z))
                    {
                        //chunk.Iswrapping = true;
                        chunk.State = ChunkState.LandscapeLightsSourceCreated;
                        chunk.LightPropagateBorderOffset.X = -1;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                    }

                    break;
                case ChunkWrapType.X_Minus1:
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedX(WorldChunks.WorldRange.Min.X + AbstractChunk.ChunkSize.X, WorldChunks.WorldRange.Min.Z))
                    {
                        //chunk.Iswrapping = true;
                        chunk.State = ChunkState.LandscapeLightsSourceCreated;
                        chunk.LightPropagateBorderOffset.X = 1;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                    }

                    break;
                case ChunkWrapType.Z_Plus1:
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedZ(WorldChunks.WorldRange.Max.Z - (AbstractChunk.ChunkSize.Z * 2), WorldChunks.WorldRange.Min.X))
                    {
                        //chunk.Iswrapping = true;
                        chunk.State = ChunkState.LandscapeLightsSourceCreated;
                        chunk.LightPropagateBorderOffset.Z = -1;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                    }

                    break;
                case ChunkWrapType.Z_Minus1:
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedZ(WorldChunks.WorldRange.Min.Z + AbstractChunk.ChunkSize.Z, WorldChunks.WorldRange.Min.X))
                    {
                        //chunk.Iswrapping = true;
                        chunk.State = ChunkState.LandscapeLightsSourceCreated;
                        chunk.LightPropagateBorderOffset.Z = 1;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                    }
                    break;
            }
        }

        private void Wrap(ChunkWrapType operationType)
        {
            _processingType = operationType;
            Range<int> NewCubeRange, ActualCubeRange, NewWorldRange;
            Location2<int> NewWrapEnd;
            int NewMinWorldValue;

            NewWorldRange = WorldChunks.WorldRange;
            NewWrapEnd = WorldChunks.WrapEnd;

            switch (operationType)
            {
                case ChunkWrapType.X_Plus1:
                    //GameConsole.Write("Row of chunks generated : Xmax");
                    //Find the Xmin chunks ! (They will be recycled with newwww cube Range)
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedX(WorldChunks.WorldRange.Min.X, WorldChunks.WorldRange.Min.Z))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                        chunk.Ready2Draw = false;

                        NewMinWorldValue = WorldChunks.WorldRange.Max.X;
                        ActualCubeRange = chunk.CubeRange;
                        NewCubeRange = new Range<int>()
                        {
                            Min = new Location3<int>(NewMinWorldValue, ActualCubeRange.Min.Y, ActualCubeRange.Min.Z),
                            Max = new Location3<int>(NewMinWorldValue + AbstractChunk.ChunkSize.X, ActualCubeRange.Max.Y, ActualCubeRange.Max.Z)
                        };
                        chunk.CubeRange = NewCubeRange;
                    }

                    //Update World Range
                    NewWorldRange.Min.X += AbstractChunk.ChunkSize.X;
                    NewWorldRange.Max.X += AbstractChunk.ChunkSize.X;

                    if (WorldChunks.WorldRange.Min.X > WorldChunks.WrapEnd.X) NewWrapEnd.X += WorldChunks.VisibleWorldSize.X;

                    break;
                case ChunkWrapType.X_Minus1:
                    //GameConsole.Write("Row of chunks generated : Xmin");
                    //Find the Xmin chunks ! (They will be recycled with newwww cube Range)
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedX(WorldChunks.WorldRange.Max.X - AbstractChunk.ChunkSize.X, WorldChunks.WorldRange.Min.Z))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                        chunk.Ready2Draw = false;

                        NewMinWorldValue = WorldChunks.WorldRange.Min.X;
                        ActualCubeRange = chunk.CubeRange;
                        NewCubeRange = new Range<int>()
                        {
                            Min = new Location3<int>(NewMinWorldValue - AbstractChunk.ChunkSize.X, ActualCubeRange.Min.Y, ActualCubeRange.Min.Z),
                            Max = new Location3<int>(NewMinWorldValue, ActualCubeRange.Max.Y, ActualCubeRange.Max.Z)
                        };
                        chunk.CubeRange = NewCubeRange;
                    }

                    //Update World Range
                    NewWorldRange.Min.X -= AbstractChunk.ChunkSize.X;
                    NewWorldRange.Max.X -= AbstractChunk.ChunkSize.X;

                    if (WorldChunks.WorldRange.Min.X <= WorldChunks.WrapEnd.X - (WorldChunks.VisibleWorldSize.X)) NewWrapEnd.X -= WorldChunks.VisibleWorldSize.X;

                    break;
                case ChunkWrapType.Z_Plus1:
                    //GameConsole.Write("Row of chunks generated : ZMax");
                    //Find the Xmin chunks ! (They will be recycled with new cube Range)
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedZ(WorldChunks.WorldRange.Min.Z, WorldChunks.WorldRange.Min.X))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                        chunk.Ready2Draw = false;

                        NewMinWorldValue = WorldChunks.WorldRange.Max.Z;
                        ActualCubeRange = chunk.CubeRange;
                        NewCubeRange = new Range<int>()
                        {
                            Min = new Location3<int>(ActualCubeRange.Min.X, ActualCubeRange.Min.Y, NewMinWorldValue),
                            Max = new Location3<int>(ActualCubeRange.Max.X, ActualCubeRange.Max.Y, NewMinWorldValue + AbstractChunk.ChunkSize.Z)
                        };
                        chunk.CubeRange = NewCubeRange;
                    }

                    //Update World Range
                    NewWorldRange.Min.Z += AbstractChunk.ChunkSize.Z;
                    NewWorldRange.Max.Z += AbstractChunk.ChunkSize.Z;

                    if (WorldChunks.WorldRange.Min.Z > WorldChunks.WrapEnd.Z) NewWrapEnd.Z += WorldChunks.VisibleWorldSize.Z;

                    break;
                case ChunkWrapType.Z_Minus1:
                    //GameConsole.Write("Row of chunks generated : Zmin");
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedZ(WorldChunks.WorldRange.Max.Z - AbstractChunk.ChunkSize.Z, WorldChunks.WorldRange.Min.X))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                        chunk.Ready2Draw = false;

                        NewMinWorldValue = WorldChunks.WorldRange.Min.Z;
                        ActualCubeRange = chunk.CubeRange;
                        NewCubeRange = new Range<int>()
                        {
                            Min = new Location3<int>(ActualCubeRange.Min.X, ActualCubeRange.Min.Y, NewMinWorldValue - AbstractChunk.ChunkSize.Z),
                            Max = new Location3<int>(ActualCubeRange.Max.X, ActualCubeRange.Max.Y, NewMinWorldValue)
                        };
                        chunk.CubeRange = NewCubeRange;
                    }

                    //Update World Range
                    NewWorldRange.Min.Z -= AbstractChunk.ChunkSize.Z;
                    NewWorldRange.Max.Z -= AbstractChunk.ChunkSize.Z;

                    if (WorldChunks.WorldRange.Min.Z <= WorldChunks.WrapEnd.Z - (WorldChunks.VisibleWorldSize.Z)) NewWrapEnd.Z -= WorldChunks.VisibleWorldSize.Z;

                    break;
                default:
                    break;
            }

            //Save the new World Range after Wrapping
            WorldChunks.WorldRange = NewWorldRange;
            WorldChunks.WrapEnd = NewWrapEnd;

            //PostWrappingStep();

            WorldChunks.ChunkNeed2BeSorted = true;

        }
        #endregion

        #region Public methods

        public void AddWrapOperation(ChunkWrapType operationType)
        {
            Wrap(operationType);
        }
        #endregion


    }
}
