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

                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedX(WorldChunks.VisualWorldParameters.WorldRange.Max.X - (AbstractChunk.ChunkSize.X * 2), WorldChunks.VisualWorldParameters.WorldRange.Min.Z))
                    {
                        //chunk.Iswrapping = true;
                        chunk.State = ChunkState.LandscapeLightsSourceCreated;
                        chunk.LightPropagateBorderOffset.X = -1;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                    }

                    break;
                case ChunkWrapType.X_Minus1:
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedX(WorldChunks.VisualWorldParameters.WorldRange.Min.X + AbstractChunk.ChunkSize.X, WorldChunks.VisualWorldParameters.WorldRange.Min.Z))
                    {
                        //chunk.Iswrapping = true;
                        chunk.State = ChunkState.LandscapeLightsSourceCreated;
                        chunk.LightPropagateBorderOffset.X = 1;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                    }

                    break;
                case ChunkWrapType.Z_Plus1:
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedZ(WorldChunks.VisualWorldParameters.WorldRange.Max.Z - (AbstractChunk.ChunkSize.Z * 2), WorldChunks.VisualWorldParameters.WorldRange.Min.X))
                    {
                        //chunk.Iswrapping = true;
                        chunk.State = ChunkState.LandscapeLightsSourceCreated;
                        chunk.LightPropagateBorderOffset.Z = -1;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                    }

                    break;
                case ChunkWrapType.Z_Minus1:
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedZ(WorldChunks.VisualWorldParameters.WorldRange.Min.Z + AbstractChunk.ChunkSize.Z, WorldChunks.VisualWorldParameters.WorldRange.Min.X))
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

            NewWorldRange = WorldChunks.VisualWorldParameters.WorldRange;
            NewWrapEnd = WorldChunks.VisualWorldParameters.WrapEnd;

            switch (operationType)
            {
                case ChunkWrapType.X_Plus1:
                    //GameConsole.Write("Row of chunks generated : Xmax");
                    //Find the Xmin chunks ! (They will be recycled with newwww cube Range)
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedX(WorldChunks.VisualWorldParameters.WorldRange.Min.X, WorldChunks.VisualWorldParameters.WorldRange.Min.Z))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                        chunk.Ready2Draw = false;

                        NewMinWorldValue = WorldChunks.VisualWorldParameters.WorldRange.Max.X;
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

                    if (NewWorldRange.Min.X > NewWrapEnd.X) NewWrapEnd.X += WorldChunks.VisualWorldParameters.WorldVisibleSize.X;

                    break;
                case ChunkWrapType.X_Minus1:
                    //GameConsole.Write("Row of chunks generated : Xmin");
                    //Find the Xmin chunks ! (They will be recycled with newwww cube Range)
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedX(WorldChunks.VisualWorldParameters.WorldRange.Max.X - AbstractChunk.ChunkSize.X, WorldChunks.VisualWorldParameters.WorldRange.Min.Z))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                        chunk.Ready2Draw = false;

                        NewMinWorldValue = WorldChunks.VisualWorldParameters.WorldRange.Min.X;
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

                    if (NewWorldRange.Min.X <= NewWrapEnd.X - (WorldChunks.VisualWorldParameters.WorldVisibleSize.X)) NewWrapEnd.X -= WorldChunks.VisualWorldParameters.WorldVisibleSize.X;

                    break;
                case ChunkWrapType.Z_Plus1:
                    //GameConsole.Write("Row of chunks generated : ZMax");
                    //Find the Xmin chunks ! (They will be recycled with new cube Range)
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedZ(WorldChunks.VisualWorldParameters.WorldRange.Min.Z, WorldChunks.VisualWorldParameters.WorldRange.Min.X))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                        chunk.Ready2Draw = false;

                        NewMinWorldValue = WorldChunks.VisualWorldParameters.WorldRange.Max.Z;
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

                    if (NewWorldRange.Min.Z > NewWrapEnd.Z) NewWrapEnd.Z += WorldChunks.VisualWorldParameters.WorldVisibleSize.Z;

                    break;
                case ChunkWrapType.Z_Minus1:
                    //GameConsole.Write("Row of chunks generated : Zmin");
                    foreach (VisualChunk chunk in WorldChunks.GetChunksWithFixedZ(WorldChunks.VisualWorldParameters.WorldRange.Max.Z - AbstractChunk.ChunkSize.Z, WorldChunks.VisualWorldParameters.WorldRange.Min.X))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.ThreadPriority = Amib.Threading.WorkItemPriority.Normal;
                        chunk.Ready2Draw = false;

                        NewMinWorldValue = WorldChunks.VisualWorldParameters.WorldRange.Min.Z;
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

                    if (NewWorldRange.Min.Z <= NewWrapEnd.Z - (WorldChunks.VisualWorldParameters.WorldVisibleSize.Z)) NewWrapEnd.Z -= WorldChunks.VisualWorldParameters.WorldVisibleSize.Z;

                    break;
                default:
                    break;
            }

            //Save the new World Range after Wrapping
            WorldChunks.VisualWorldParameters.WorldRange = NewWorldRange;
            WorldChunks.VisualWorldParameters.WrapEnd = NewWrapEnd;

            PostWrappingStep();

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
