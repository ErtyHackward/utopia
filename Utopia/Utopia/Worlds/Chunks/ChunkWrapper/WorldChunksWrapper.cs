using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;
using Utopia.Shared.Chunks;
using S33M3Resources.Structs;

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
        public IWorldChunks2D WorldChunks { get; set; }
        #endregion

        public WorldChunksWrapper()
        {
        }

        #region Private methods
        //Force some chunks to be refresh (not reconstructed)
        //All chunks are in "DisplayInSyncWithMeshes"
        private void PostWrappingStep()
        {
            switch (_processingType)
            {
                case ChunkWrapType.X_Plus1:

                    foreach (VisualChunk2D chunk in WorldChunks.GetChunksWithFixedX(WorldChunks.VisualWorldParameters.WorldRange.Max.X - (AbstractChunk.ChunkSize.X * 2), WorldChunks.VisualWorldParameters.WorldRange.Position.Z))
                    {
                        chunk.RefreshBorderChunk();
                    }

                    break;
                case ChunkWrapType.X_Minus1:
                    foreach (VisualChunk2D chunk in WorldChunks.GetChunksWithFixedX(WorldChunks.VisualWorldParameters.WorldRange.Position.X + AbstractChunk.ChunkSize.X, WorldChunks.VisualWorldParameters.WorldRange.Position.Z))
                    {
                        chunk.RefreshBorderChunk();
                    }

                    break;
                case ChunkWrapType.Z_Plus1:
                    foreach (VisualChunk2D chunk in WorldChunks.GetChunksWithFixedZ(WorldChunks.VisualWorldParameters.WorldRange.Max.Z - (AbstractChunk.ChunkSize.Z * 2), WorldChunks.VisualWorldParameters.WorldRange.Position.X))
                    {
                        chunk.RefreshBorderChunk();
                    }

                    break;
                case ChunkWrapType.Z_Minus1:
                    foreach (VisualChunk2D chunk in WorldChunks.GetChunksWithFixedZ(WorldChunks.VisualWorldParameters.WorldRange.Position.Z + AbstractChunk.ChunkSize.Z, WorldChunks.VisualWorldParameters.WorldRange.Position.X))
                    {
                        chunk.RefreshBorderChunk();
                    }
                    break;
            }
        }

        private void Wrap(ChunkWrapType operationType)
        {
            _processingType = operationType;
            Range3I NewCubeRange, ActualCubeRange, NewWorldRange;
            Vector2I NewWrapEnd;
            int NewMinWorldValue;

            NewWorldRange = WorldChunks.VisualWorldParameters.WorldRange;
            NewWrapEnd = WorldChunks.VisualWorldParameters.WrapEnd;

            switch (operationType)
            {
                case ChunkWrapType.X_Plus1:
                    //GameConsole.Write("Row of chunks generated : Xmax");
                    //Find the Xmin chunks ! (They will be recycled with newwww cube Range)
                    foreach (VisualChunk2D chunk in WorldChunks.GetChunksWithFixedX(WorldChunks.VisualWorldParameters.WorldRange.Position.X, WorldChunks.VisualWorldParameters.WorldRange.Position.Z))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.Graphics.IsExistingMesh4Drawing = false;

                        NewMinWorldValue = WorldChunks.VisualWorldParameters.WorldRange.Max.X;
                        ActualCubeRange = chunk.CubeRange;
                        NewCubeRange = new Range3I()
                        {
                            Position = new Vector3I(NewMinWorldValue, ActualCubeRange.Position.Y, ActualCubeRange.Position.Z),
                            Size = AbstractChunk.ChunkSize
                        };
                        chunk.CubeRange = NewCubeRange;
                    }

                    //Update World Range
                    NewWorldRange.Position.X += AbstractChunk.ChunkSize.X;
                    if (NewWorldRange.Position.X > NewWrapEnd.X) NewWrapEnd.X += WorldChunks.VisualWorldParameters.WorldVisibleSize.X;

                    break;
                case ChunkWrapType.X_Minus1:
                    //GameConsole.Write("Row of chunks generated : Xmin");
                    //Find the Xmin chunks ! (They will be recycled with newwww cube Range)
                    foreach (VisualChunk2D chunk in WorldChunks.GetChunksWithFixedX(WorldChunks.VisualWorldParameters.WorldRange.Max.X - AbstractChunk.ChunkSize.X, WorldChunks.VisualWorldParameters.WorldRange.Position.Z))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.Graphics.IsExistingMesh4Drawing = false;

                        NewMinWorldValue = WorldChunks.VisualWorldParameters.WorldRange.Position.X;
                        ActualCubeRange = chunk.CubeRange;
                        NewCubeRange = new Range3I()
                        {
                            Position = new Vector3I(NewMinWorldValue - AbstractChunk.ChunkSize.X, ActualCubeRange.Position.Y, ActualCubeRange.Position.Z),
                            Size = AbstractChunk.ChunkSize
                        };
                        chunk.CubeRange = NewCubeRange;
                    }

                    //Update World Range
                    NewWorldRange.Position.X -= AbstractChunk.ChunkSize.X;
                    if (NewWorldRange.Position.X <= NewWrapEnd.X - (WorldChunks.VisualWorldParameters.WorldVisibleSize.X)) NewWrapEnd.X -= WorldChunks.VisualWorldParameters.WorldVisibleSize.X;

                    break;
                case ChunkWrapType.Z_Plus1:
                    //GameConsole.Write("Row of chunks generated : ZMax");
                    //Find the Xmin chunks ! (They will be recycled with new cube Range)
                    foreach (VisualChunk2D chunk in WorldChunks.GetChunksWithFixedZ(WorldChunks.VisualWorldParameters.WorldRange.Position.Z, WorldChunks.VisualWorldParameters.WorldRange.Position.X))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.Graphics.IsExistingMesh4Drawing = false;

                        NewMinWorldValue = WorldChunks.VisualWorldParameters.WorldRange.Max.Z;
                        ActualCubeRange = chunk.CubeRange;
                        NewCubeRange = new Range3I()
                        {
                            Position = new Vector3I(ActualCubeRange.Position.X, ActualCubeRange.Position.Y, NewMinWorldValue),
                            Size = AbstractChunk.ChunkSize
                        };
                        chunk.CubeRange = NewCubeRange;
                    }

                    //Update World Range
                    NewWorldRange.Position.Z += AbstractChunk.ChunkSize.Z;
                    if (NewWorldRange.Position.Z > NewWrapEnd.Y) NewWrapEnd.Y += WorldChunks.VisualWorldParameters.WorldVisibleSize.Z;

                    break;
                case ChunkWrapType.Z_Minus1:
                    //GameConsole.Write("Row of chunks generated : Zmin");
                    foreach (VisualChunk2D chunk in WorldChunks.GetChunksWithFixedZ(WorldChunks.VisualWorldParameters.WorldRange.Max.Z - AbstractChunk.ChunkSize.Z, WorldChunks.VisualWorldParameters.WorldRange.Position.X))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.Graphics.IsExistingMesh4Drawing = false;

                        NewMinWorldValue = WorldChunks.VisualWorldParameters.WorldRange.Position.Z;
                        ActualCubeRange = chunk.CubeRange;
                        NewCubeRange = new Range3I()
                        {
                            Position = new Vector3I(ActualCubeRange.Position.X, ActualCubeRange.Position.Y, NewMinWorldValue - AbstractChunk.ChunkSize.Z),
                            Size = AbstractChunk.ChunkSize
                        };
                        chunk.CubeRange = NewCubeRange;
                    }

                    //Update World Range
                    NewWorldRange.Position.Z -= AbstractChunk.ChunkSize.Z;
                    if (NewWorldRange.Position.Z <= NewWrapEnd.Y - (WorldChunks.VisualWorldParameters.WorldVisibleSize.Z)) NewWrapEnd.Y -= WorldChunks.VisualWorldParameters.WorldVisibleSize.Z;

                    break;
                default:
                    break;
            }

            //Save the new World Range after Wrapping
            WorldChunks.VisualWorldParameters.WorldRange = NewWorldRange;
            WorldChunks.VisualWorldParameters.WrapEnd = NewWrapEnd;

            PostWrappingStep();
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
