using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D.DebugTools;
using Utopia.Planets.Terran.World;
using SharpDX;
using S33M3Engines.Struct;
using Utopia.Shared.Structs;
using Utopia.Shared.Landscaping;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.Chunks.ChunkWrapper;

namespace Utopia.Planets.Terran.Chunk
{
    //Managed the wrapping by enqueuing the request, to ensure that only one wrap operation is done at a time
    public static class ChunkWrapper
    {
        private static ChunkWrapType _processingType;
        private static TerraWorld _world;

        //public static ChunkWrapperStatus Status = ChunkWrapperStatus.Idle;

        //Force some chunks to be refresh (not reconstructed)
        private static void PostWrappingStep()
        {
            switch (_processingType)
            {
                case ChunkWrapType.X_Plus1:

                    foreach (TerraChunk chunk in ChunkFinder.GetChunksWithFixedX(_world.WorldRange.Max.X - (LandscapeBuilder.Chunksize * 2), _world.WorldRange.Min.Z))
                    {
                        //chunk.Iswrapping = true;
                        chunk.State = ChunkState.LandscapeLightsSourceCreated;
                        chunk.LightPropagateBorderOffset.X = -1;
                        chunk.Priority = Amib.Threading.WorkItemPriority.Normal;
                    }

                    break;
                case ChunkWrapType.X_Minus1:
                    foreach (TerraChunk chunk in ChunkFinder.GetChunksWithFixedX(_world.WorldRange.Min.X + LandscapeBuilder.Chunksize, _world.WorldRange.Min.Z))
                    {
                        //chunk.Iswrapping = true;
                        chunk.State = ChunkState.LandscapeLightsSourceCreated;
                        chunk.LightPropagateBorderOffset.X = 1;
                        chunk.Priority = Amib.Threading.WorkItemPriority.Normal;
                    }

                    break;
                case ChunkWrapType.Z_Plus1:
                    foreach (TerraChunk chunk in ChunkFinder.GetChunksWithFixedZ(_world.WorldRange.Max.Z - (LandscapeBuilder.Chunksize * 2), _world.WorldRange.Min.X))
                    {
                        //chunk.Iswrapping = true;
                        chunk.State = ChunkState.LandscapeLightsSourceCreated;
                        chunk.LightPropagateBorderOffset.Z = -1;
                        chunk.Priority = Amib.Threading.WorkItemPriority.Normal;
                    }

                    break;
                case ChunkWrapType.Z_Minus1:
                    foreach (TerraChunk chunk in ChunkFinder.GetChunksWithFixedZ(_world.WorldRange.Min.Z + LandscapeBuilder.Chunksize, _world.WorldRange.Min.X))
                    {
                        //chunk.Iswrapping = true;
                        chunk.State = ChunkState.LandscapeLightsSourceCreated;
                        chunk.LightPropagateBorderOffset.Z = 1;
                        chunk.Priority = Amib.Threading.WorkItemPriority.Normal;
                    }
                    break;
            }
        }

        public static void Init(TerraWorld world)
        {
            _world = world;
        }

        public static void AddWrapOperation(ChunkWrapType operationType)
        {
            Wrap(operationType);
        }

        public static void Wrap(ChunkWrapType operationType)
        {
            _processingType = operationType;
            Range<int> NewCubeRange, ActualCubeRange;
            int NewMinWorldValue;
            
            switch (operationType)
            {
                case ChunkWrapType.X_Plus1:
                    //GameConsole.Write("Row of chunks generated : Xmax");
                    //Find the Xmin chunks ! (They will be recycled with newwww cube Range)
                    foreach (TerraChunk chunk in ChunkFinder.GetChunksWithFixedX(_world.WorldRange.Min.X, _world.WorldRange.Min.Z))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.Priority = Amib.Threading.WorkItemPriority.Normal;
                        chunk.Ready2Draw = false;

                        NewMinWorldValue = _world.WorldRange.Max.X;
                        ActualCubeRange = chunk.CubeRange;
                        NewCubeRange = new Range<int>()
                        {
                            Min = new Location3<int>(NewMinWorldValue, ActualCubeRange.Min.Y, ActualCubeRange.Min.Z),
                            Max = new Location3<int>(NewMinWorldValue + LandscapeBuilder.Chunksize, ActualCubeRange.Max.Y, ActualCubeRange.Max.Z)
                        };
                        chunk.CubeRange = NewCubeRange;
                    }

                    //Update World Range
                    _world.WorldRange.Min.X += LandscapeBuilder.Chunksize;
                    _world.WorldRange.Max.X += LandscapeBuilder.Chunksize;

                    if (_world.WorldRange.Min.X > _world.WrapEnd.X) _world.WrapEnd.X += LandscapeBuilder.ChunkGridSize * LandscapeBuilder.Chunksize; 

                    break;
                case ChunkWrapType.X_Minus1:
                    //GameConsole.Write("Row of chunks generated : Xmin");
                    //Find the Xmin chunks ! (They will be recycled with newwww cube Range)
                    foreach (TerraChunk chunk in ChunkFinder.GetChunksWithFixedX(_world.WorldRange.Max.X - LandscapeBuilder.Chunksize, _world.WorldRange.Min.Z))
                    {
                        chunk.State = ChunkState.Empty;
                        chunk.Priority = Amib.Threading.WorkItemPriority.Normal;
                        chunk.Ready2Draw = false;

                        NewMinWorldValue = _world.WorldRange.Min.X;
                        ActualCubeRange = chunk.CubeRange;
                        NewCubeRange = new Range<int>()
                        {
                            Min = new Location3<int>(NewMinWorldValue - LandscapeBuilder.Chunksize, ActualCubeRange.Min.Y, ActualCubeRange.Min.Z),
                            Max = new Location3<int>(NewMinWorldValue, ActualCubeRange.Max.Y, ActualCubeRange.Max.Z)
                        };
                        chunk.CubeRange = NewCubeRange;
                    }

                    //Update World Range
                    _world.WorldRange.Min.X -= LandscapeBuilder.Chunksize;
                    _world.WorldRange.Max.X -= LandscapeBuilder.Chunksize;

                    if (_world.WorldRange.Min.X <= _world.WrapEnd.X - (LandscapeBuilder.ChunkGridSize * LandscapeBuilder.Chunksize)) _world.WrapEnd.X -= LandscapeBuilder.ChunkGridSize * LandscapeBuilder.Chunksize;

                    break;
                case ChunkWrapType.Z_Plus1:
                        //GameConsole.Write("Row of chunks generated : ZMax");
                        //Find the Xmin chunks ! (They will be recycled with new cube Range)
                    foreach (TerraChunk chunk in ChunkFinder.GetChunksWithFixedZ(_world.WorldRange.Min.Z, _world.WorldRange.Min.X))
                        {
                            chunk.State = ChunkState.Empty;
                            chunk.Priority = Amib.Threading.WorkItemPriority.Normal;
                            chunk.Ready2Draw = false;

                            NewMinWorldValue = _world.WorldRange.Max.Z;
                            ActualCubeRange = chunk.CubeRange;
                            NewCubeRange = new Range<int>()
                            {
                                Min = new Location3<int>(ActualCubeRange.Min.X, ActualCubeRange.Min.Y, NewMinWorldValue),
                                Max = new Location3<int>(ActualCubeRange.Max.X, ActualCubeRange.Max.Y, NewMinWorldValue + LandscapeBuilder.Chunksize)
                            };
                            chunk.CubeRange = NewCubeRange;
                        }

                        //Update World Range
                    _world.WorldRange.Min.Z += LandscapeBuilder.Chunksize;
                    _world.WorldRange.Max.Z += LandscapeBuilder.Chunksize;

                    if (_world.WorldRange.Min.Z > _world.WrapEnd.Z) _world.WrapEnd.Z += LandscapeBuilder.ChunkGridSize * LandscapeBuilder.Chunksize; 

                    break;
                case ChunkWrapType.Z_Minus1:
                        //GameConsole.Write("Row of chunks generated : Zmin");
                    foreach (TerraChunk chunk in ChunkFinder.GetChunksWithFixedZ(_world.WorldRange.Max.Z - LandscapeBuilder.Chunksize, _world.WorldRange.Min.X))
                        {
                            chunk.State = ChunkState.Empty;
                            chunk.Priority = Amib.Threading.WorkItemPriority.Normal;
                            chunk.Ready2Draw = false;

                            NewMinWorldValue = _world.WorldRange.Min.Z;
                            ActualCubeRange = chunk.CubeRange;
                            NewCubeRange = new Range<int>()
                            {
                                Min = new Location3<int>(ActualCubeRange.Min.X, ActualCubeRange.Min.Y, NewMinWorldValue - LandscapeBuilder.Chunksize),
                                Max = new Location3<int>(ActualCubeRange.Max.X, ActualCubeRange.Max.Y, NewMinWorldValue)
                            };
                            chunk.CubeRange = NewCubeRange;
                        }

                        //Update World Range
                    _world.WorldRange.Min.Z -= LandscapeBuilder.Chunksize;
                    _world.WorldRange.Max.Z -= LandscapeBuilder.Chunksize;

                    if (_world.WorldRange.Min.Z <= _world.WrapEnd.Z - (LandscapeBuilder.ChunkGridSize * LandscapeBuilder.Chunksize)) _world.WrapEnd.Z -= LandscapeBuilder.ChunkGridSize * LandscapeBuilder.Chunksize;

                    break;
                default:
                    break;
            }

            PostWrappingStep();

            _world.ChunkNeed2BeSorted = true;

        }
    }
}
