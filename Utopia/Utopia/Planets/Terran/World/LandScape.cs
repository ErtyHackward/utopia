using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Planets.Terran.Cube;
using S33M3Engines.Struct;
using S33M3Engines.Maths;
using SharpDX;
using Utopia.Planets.Terran.Chunk;
using Utopia.USM;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Landscaping;
using S33M3Engines.Shared.Math;

namespace Utopia.Planets.Terran.World
{

    public enum IdxRelativeMove : byte
    {
        X_Minus1,
        X_Plus1,
        Z_Minus1,
        Z_Plus1,
        Y_Minus1,
        Y_Plus1,
        None
    }

    public struct SurroundingIndex
    {
        public int Index;
        public IdxRelativeMove IndexRelativePosition;
        public Location3<int> Position;
    }

    // Class representing the World Landscape !
    public class LandScape
    {
        public TerraCube[] Cubes;
        Location3<int> _landscapeBufferSize;
        TerraWorld _world;
        private int _landscapeBufferSizeXZ;

        public LandScape(Location3<int> LandscapeBufferSize, TerraWorld world)
        {
            _world = world;
            _landscapeBufferSize = LandscapeBufferSize;
            Cubes = new TerraCube[_landscapeBufferSize.X * _landscapeBufferSize.Z * _landscapeBufferSize.Y];

            _landscapeBufferSizeXZ = _landscapeBufferSize.X * _landscapeBufferSize.Z;
        }

        public int Index(int X, int Y, int Z)
        {
            return MathHelper.Mod(X, _landscapeBufferSize.X) +
                   MathHelper.Mod(Z, _landscapeBufferSize.Z) * _landscapeBufferSize.X +
                   Y * _landscapeBufferSize.X * _landscapeBufferSize.Z;
        }

        public int FastIndex(int baseIndex, int XPosition, int YPosition, int ZPosition, IdxRelativeMove idxmove, bool PositionFromBase)
        {
            int value = baseIndex;
            switch (idxmove)
            {
                case IdxRelativeMove.X_Minus1:
                    if (!PositionFromBase) XPosition++;
                    value = baseIndex - 1;
                    if (XPosition == _world.WrapEnd.X) value += _landscapeBufferSize.X;
                    break;
                case IdxRelativeMove.X_Plus1:
                    if (!PositionFromBase) XPosition--;
                    value = baseIndex + 1;
                    if (XPosition == _world.WrapEnd.X - 1) value -= _landscapeBufferSize.X;
                    break;
                case IdxRelativeMove.Z_Minus1:
                    if (!PositionFromBase) ZPosition++;
                    value = baseIndex - _landscapeBufferSize.X;
                    if (ZPosition == _world.WrapEnd.Z) value += _landscapeBufferSizeXZ;
                    break;
                case IdxRelativeMove.Z_Plus1:
                    if (!PositionFromBase) ZPosition--;
                    value = baseIndex + _landscapeBufferSize.X;
                    if (ZPosition == _world.WrapEnd.Z - 1) value -= _landscapeBufferSizeXZ;
                    break;
                case IdxRelativeMove.Y_Minus1:
                    if (!PositionFromBase) YPosition++;
                    value = baseIndex - _landscapeBufferSizeXZ;
                    break;
                case IdxRelativeMove.Y_Plus1:
                    if (!PositionFromBase) YPosition--;
                    value = baseIndex + _landscapeBufferSizeXZ;
                    break;
            }

            return value;
        }


        public int FastIndex(int baseIndex, int Position, IdxRelativeMove idxmove)
        {
            int value;
            switch (idxmove)
            {
                case IdxRelativeMove.None:
                    return baseIndex;
                case IdxRelativeMove.X_Minus1:
                    value = baseIndex - 1;
                    if (Position == _world.WrapEnd.X) value += _landscapeBufferSize.X;
                    break;
                case IdxRelativeMove.X_Plus1:
                    value = baseIndex + 1;
                    if (Position == _world.WrapEnd.X - 1) value -= _landscapeBufferSize.X;
                    break;
                case IdxRelativeMove.Z_Minus1:
                    value = baseIndex - _landscapeBufferSize.X;
                    if (Position == _world.WrapEnd.Z) value += _landscapeBufferSizeXZ;
                    break;
                case IdxRelativeMove.Z_Plus1:
                    value = baseIndex + _landscapeBufferSize.X;
                    if (Position == _world.WrapEnd.Z - 1) value -= _landscapeBufferSizeXZ;
                    break;
                case IdxRelativeMove.Y_Minus1:
                    value = baseIndex - _landscapeBufferSizeXZ;
                    break;
                case IdxRelativeMove.Y_Plus1:
                    value = baseIndex + _landscapeBufferSizeXZ;
                    break;
                default:
                    value = int.MaxValue;
                    break;
            }

            return value;

        }

        public bool Index(int X, int Y, int Z, bool isSafe, out int index)
        {
            if (isSafe)
            {
                if (X < _world.WorldRange.Min.X || X >= _world.WorldRange.Max.X || Z < _world.WorldRange.Min.Z || Z >= _world.WorldRange.Max.Z || Y < 0 || Y >= _world.WorldRange.Max.Y)
                {
                    index = int.MaxValue;
                    return false;
                }
            }

            index = MathHelper.Mod(X, _landscapeBufferSize.X) +
                    MathHelper.Mod(Z, _landscapeBufferSize.Z) * _landscapeBufferSize.X +
                    Y * _landscapeBufferSize.X * _landscapeBufferSize.Z;

            return true;
        }

        public bool isIndexInError(int index)
        {
            if (index == int.MaxValue) return true;
            return false;
        }

        public bool SafeIndexY(int X, int Y, int Z, out int index)
        {
            if (Y < 0 || Y >= _world.WorldRange.Max.Y)
            {
                index = int.MaxValue;
                return false;
            }

            index = MathHelper.Mod(X, _landscapeBufferSize.X) +
                   MathHelper.Mod(Z, _landscapeBufferSize.Z) * _landscapeBufferSize.X +
                   Y * _landscapeBufferSize.X * _landscapeBufferSize.Z;
            return true;
        }

        public TerraCube GetCube(int X, int Y, int Z)
        {
            return Cubes[Index(X, Y, Z)];
        }

        public SurroundingIndex[] GetSurroundingBlocksIndex(int baseIndex, int CubeXCoord, int CubeYCoord, int CubeZCoord)
        {
            int cubeIndex = baseIndex;
            SurroundingIndex[] surroundingIndexes = new SurroundingIndex[6];

            surroundingIndexes[0] = new SurroundingIndex() { Index = FastIndex(cubeIndex, CubeXCoord, IdxRelativeMove.X_Plus1), IndexRelativePosition = IdxRelativeMove.X_Plus1, Position = new Location3<int>(CubeXCoord + 1, CubeYCoord, CubeZCoord) };
            surroundingIndexes[1] = new SurroundingIndex() { Index = FastIndex(cubeIndex, CubeXCoord, IdxRelativeMove.X_Minus1), IndexRelativePosition = IdxRelativeMove.X_Minus1, Position = new Location3<int>(CubeXCoord - 1, CubeYCoord, CubeZCoord) };
            surroundingIndexes[2] = new SurroundingIndex() { Index = FastIndex(cubeIndex, CubeZCoord, IdxRelativeMove.Z_Plus1), IndexRelativePosition = IdxRelativeMove.Z_Plus1, Position = new Location3<int>(CubeXCoord, CubeYCoord, CubeZCoord + 1) };
            surroundingIndexes[3] = new SurroundingIndex() { Index = FastIndex(cubeIndex, CubeZCoord, IdxRelativeMove.Z_Minus1), IndexRelativePosition = IdxRelativeMove.Z_Minus1, Position = new Location3<int>(CubeXCoord, CubeYCoord, CubeZCoord - 1) };
            surroundingIndexes[4] = new SurroundingIndex() { Index = FastIndex(cubeIndex, CubeYCoord, IdxRelativeMove.Y_Minus1), IndexRelativePosition = IdxRelativeMove.Y_Minus1, Position = new Location3<int>(CubeXCoord, CubeYCoord - 1, CubeZCoord) };
            surroundingIndexes[5] = new SurroundingIndex() { Index = FastIndex(cubeIndex, CubeYCoord, IdxRelativeMove.Y_Plus1), IndexRelativePosition = IdxRelativeMove.Y_Plus1, Position = new Location3<int>(CubeXCoord, CubeYCoord + 1, CubeZCoord) };
            return surroundingIndexes;
        }

        public SurroundingIndex[] GetSurroundingBlocksIndex(int CubeXCoord, int CubeYCoord, int CubeZCoord)
        {
            int cubeIndex = Index(CubeXCoord, CubeYCoord, CubeZCoord);
            return GetSurroundingBlocksIndex(cubeIndex, CubeXCoord, CubeYCoord, CubeZCoord);
        }

        public SurroundingIndex[] GetSurroundingBlocksIndex(ref Location3<int> CubeCoordinates)
        {
            return GetSurroundingBlocksIndex(CubeCoordinates.X, CubeCoordinates.Y, CubeCoordinates.Z);
        }

        public void SetCube(ref Location3<int> cubeCoordinates, ref TerraCube cube, bool isUSMCalled = true)
        {
            Cubes[Index(cubeCoordinates.X, cubeCoordinates.Y, cubeCoordinates.Z)] = cube;
            // Save the cube that has been changed !

            //if (isUSMCalled)
            //{
            //    _chunk = ChunkFinder.GetChunk(cubeCoordinates.X, cubeCoordinates.Z);
            //    CubeData data2Set = new CubeData() { ChunkID = _chunk.ChunkID, Cube = cube, CubeChunkLocation = new Location3<int>(cubeCoordinates.X, cubeCoordinates.Y, cubeCoordinates.Z) };
            //    UtopiaSaveManager.Setdata.Enqueue(ref data2Set);
            //}
        }

        public void SetCube(int x, int y, int z, ref TerraCube cube, bool isUSMCalled = true)
        {
            Cubes[Index(x, y, z)] = cube;
            // Save the cube that has been changed !
            //if (isUSMCalled)
            //{
            //    _chunk = ChunkFinder.GetChunk(x, z);
            //    CubeData data2Set = new CubeData() { ChunkID = _chunk.ChunkID, Cube = cube, CubeChunkLocation = new Location3<int>(x, y, z) };
            //    UtopiaSaveManager.Setdata.Enqueue(ref data2Set);
            //}
        }

        public void SetCubeWithNotification(ref Location3<int> cubeCoordinates, ref TerraCube cube, bool includeSurroundingChunks, Amib.Threading.WorkItemPriority priority = Amib.Threading.WorkItemPriority.Highest, Amib.Threading.WorkItemPriority priorityNeighb = Amib.Threading.WorkItemPriority.AboveNormal)
        {
            TerraChunk chunk, neightboorChunk;
            RenderCubeProfile profile = RenderCubeProfile.CubesProfile[cube.Id];

            SetCube(ref cubeCoordinates, ref cube);

            //The Modified bloc
            chunk = ChunkFinder.GetChunk(cubeCoordinates.X, cubeCoordinates.Z);

#if DEBUG
            //if (chunk.State != ChunkState.DisplayInSyncWithMeshes && chunk.State != ChunkState.UserChanged)
            //{
            //    Console.WriteLine("WARNING SetCubeWithNotification chunk state changed, but shouldn't !! : OLD " + chunk.State + " new : " + ChunkState.UserChanged);
            //}
#endif
            chunk.State = ChunkState.UserChanged;
            chunk.Priority = priority;
            chunk.UserChangeOrder = !profile.IsBlockingLight ? 2 : 1;

            if (includeSurroundingChunks)
            {

                neightboorChunk = ChunkFinder.GetChunk(cubeCoordinates.X + 16, cubeCoordinates.Z);
                neightboorChunk.State = ChunkState.UserChanged;
                neightboorChunk.Priority = priority;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;

                neightboorChunk = ChunkFinder.GetChunk(cubeCoordinates.X - 16, cubeCoordinates.Z);
                neightboorChunk.State = ChunkState.UserChanged;
                neightboorChunk.Priority = priority;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;

                neightboorChunk = ChunkFinder.GetChunk(cubeCoordinates.X, cubeCoordinates.Z + 16);
                neightboorChunk.State = ChunkState.UserChanged;
                neightboorChunk.Priority = priority;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;

                neightboorChunk = ChunkFinder.GetChunk(cubeCoordinates.X, cubeCoordinates.Z - 16);
                neightboorChunk.State = ChunkState.UserChanged;
                neightboorChunk.Priority = priority;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;

                neightboorChunk = ChunkFinder.GetChunk(cubeCoordinates.X + 16, cubeCoordinates.Z + 16);
                neightboorChunk.State = ChunkState.UserChanged;
                neightboorChunk.Priority = priority;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;

                neightboorChunk = ChunkFinder.GetChunk(cubeCoordinates.X - 16, cubeCoordinates.Z + 16);
                neightboorChunk.State = ChunkState.UserChanged;
                neightboorChunk.Priority = priority;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;

                neightboorChunk = ChunkFinder.GetChunk(cubeCoordinates.X + 16, cubeCoordinates.Z - 16);
                neightboorChunk.State = ChunkState.UserChanged;
                neightboorChunk.Priority = priority;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;

                neightboorChunk = ChunkFinder.GetChunk(cubeCoordinates.X - 16, cubeCoordinates.Z - 16);
                neightboorChunk.State = ChunkState.UserChanged;
                neightboorChunk.Priority = priority;
                neightboorChunk.UserChangeOrder = !profile.IsBlockingLight ? 1 : 2;
            }
        }

        public bool isPickable(ref Vector3 position, out TerraCube cube)
        {
            int cubeIndex;

            if (Index(MathHelper.Fastfloor(position.X), MathHelper.Fastfloor(position.Y), MathHelper.Fastfloor(position.Z), true, out cubeIndex))
            {
                cube = Cubes[cubeIndex];
                if (Cubes[cubeIndex].Id == CubeId.Air) cube = new TerraCube(CubeId.Error);
                return RenderCubeProfile.CubesProfile[cube.Id].IsPickable;
            }

            cube = new TerraCube(CubeId.Error);
            return false;
        }

        public bool isPickable(ref Vector3 position)
        {
            int cubeIndex;
            if (Index(MathHelper.Fastfloor(position.X), MathHelper.Fastfloor(position.Y), MathHelper.Fastfloor(position.Z), true, out cubeIndex))
            {
                return RenderCubeProfile.CubesProfile[Cubes[cubeIndex].Id].IsPickable;
            }

            return false;
        }

        public bool IsSolidToPlayer(ref BoundingBox bb)
        {
            int index;

            //Get ground surface 4 blocks below the Bounding box
            int Xmin = MathHelper.Fastfloor(bb.Minimum.X);
            int Zmin = MathHelper.Fastfloor(bb.Minimum.Z);
            int Ymin = MathHelper.Fastfloor(bb.Minimum.Y);
            int Xmax = MathHelper.Fastfloor(bb.Maximum.X);
            int Zmax = MathHelper.Fastfloor(bb.Maximum.Z);
            int Ymax = MathHelper.Fastfloor(bb.Maximum.Y);

            for (int x = Xmin; x <= Xmax; x++)
            {
                for (int z = Zmin; z <= Zmax; z++)
                {
                    for (int y = Ymin; y <= Ymax; y++)
                    {
                        if (SafeIndexY(x, y, z, out index))
                        {
                            if (RenderCubeProfile.CubesProfile[Cubes[index].Id].IsSolidToEntity)
                            {
                                return true;
                            }
                        }

                    }
                }
            }
            return false;
        }

        public bool IsSolidToPlayer(ref BoundingBox bb, out TerraCubeWithPosition collidingcube)
        {
            int index;

            //Get ground surface 4 blocks below the Bounding box
            int Xmin = MathHelper.Fastfloor(bb.Minimum.X);
            int Zmin = MathHelper.Fastfloor(bb.Minimum.Z);
            int Ymin = MathHelper.Fastfloor(bb.Minimum.Y);
            int Xmax = MathHelper.Fastfloor(bb.Maximum.X);
            int Zmax = MathHelper.Fastfloor(bb.Maximum.Z);
            int Ymax = MathHelper.Fastfloor(bb.Maximum.Y);

            for (int x = Xmin; x <= Xmax; x++)
            {
                for (int z = Zmin; z <= Zmax; z++)
                {
                    for (int y = Ymin; y <= Ymax; y++)
                    {
                        if (SafeIndexY(x, y, z, out index))
                        {
                            if (RenderCubeProfile.CubesProfile[Cubes[index].Id].IsSolidToEntity)
                            {
                                collidingcube.Cube = Cubes[index];
                                collidingcube.Position = new Location3<int>(x, y, z);
                                return true;
                            }
                        }
                    }
                }
            }

            collidingcube = new TerraCubeWithPosition();
            return false;
        }

        public void GetNextSolidBlockToPlayer(ref Vector3 FromPosition, ref Location3<int> Direction, out TerraCubeWithPosition cubeWithPosition)
        {
            int index = 0;
            cubeWithPosition.Cube = new TerraCube(CubeId.Air);

            int X = MathHelper.Fastfloor(FromPosition.X);
            int Z = MathHelper.Fastfloor(FromPosition.Z);
            int Y = MathHelper.Fastfloor(FromPosition.Y);

            if (Y >= LandscapeBuilder.Worldsize.Y) Y = LandscapeBuilder.Worldsize.Y - 1;

            while (!RenderCubeProfile.CubesProfile[cubeWithPosition.Cube.Id].IsSolidToEntity && !isIndexInError(index))
            {
                if (SafeIndexY(X, Y, Z, out index))
                {
                    if (RenderCubeProfile.CubesProfile[Cubes[index].Id].IsSolidToEntity)
                    {
                        cubeWithPosition.Cube = Cubes[index];
                        break;
                    }
                    X += Direction.X;
                    Y += Direction.Y;
                    Z += Direction.Z;
                }
            }


            cubeWithPosition.Position = new Location3<int>(X, Y, Z);
        }

        public void GetNextSolidBlockToPlayer(ref BoundingBox FromBBPosition, ref Location3<int> Direction, out TerraCubeWithPosition cubeWithPosition)
        {
            TerraCubeWithPosition testCube;
            Vector3 testPoint;

            testPoint = new Vector3(FromBBPosition.Minimum.X, FromBBPosition.Minimum.Y, FromBBPosition.Minimum.Z);
            GetNextSolidBlockToPlayer(ref testPoint, ref Direction, out testCube);
            cubeWithPosition = testCube;

            testPoint = new Vector3(FromBBPosition.Maximum.X, FromBBPosition.Minimum.Y, FromBBPosition.Minimum.Z);
            GetNextSolidBlockToPlayer(ref testPoint, ref Direction, out testCube);
            if (testCube.Position.Y > cubeWithPosition.Position.Y) cubeWithPosition = testCube;

            testPoint = new Vector3(FromBBPosition.Minimum.X, FromBBPosition.Minimum.Y, FromBBPosition.Maximum.Z);
            GetNextSolidBlockToPlayer(ref testPoint, ref Direction, out testCube);
            if (testCube.Position.Y > cubeWithPosition.Position.Y) cubeWithPosition = testCube;

            testPoint = new Vector3(FromBBPosition.Maximum.X, FromBBPosition.Minimum.Y, FromBBPosition.Maximum.Z);
            GetNextSolidBlockToPlayer(ref testPoint, ref Direction, out testCube);
            if (testCube.Position.Y > cubeWithPosition.Position.Y) cubeWithPosition = testCube;
        }


    }

}
