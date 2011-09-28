using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.World;
using Utopia.Shared.Structs;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Cubes;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Class responsible to manage the acces to the circular array containing the Cubes
    /// </summary>
    public class SingleArrayChunkContainer: IDisposable
    {
        public struct SurroundingIndex
        {
            public int Index;
            public IdxRelativeMove IndexRelativePosition;
            public Vector3I Position;
        }

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

        public enum Axis
        {
            X,
            Y,
            Z
        }

        public static int UpIndex = 0;
        public static int DownIndex = 1;
        public static int RightIndex = 2;
        public static int LeftIndex = 3;
        public static int UpRightIndex = 4;
        public static int UpLeftIndex = 5;
        public static int DownRightIndex = 6;
        public static int DownLeftIndex = 7;
        public static int BaseIndex = 8;

        private int _bigArraySize;
        private VisualWorldParameters _visualWorldParam;

        /// <summary>
        /// Occurs when block data was changed
        /// </summary>
        public event EventHandler<ChunkDataProviderDataChangedEventArgs> BlockDataChanged;

        /// <summary>
        /// Contains the array of visible cubes
        /// </summary>
        public TerraCube[] Cubes { get; set;}

        /// <summary>
        /// Contains the value used to advance inside the array from a specific Index.
        /// </summary>
        public int MoveX { get; private set; } // + = Move Est, - = Move West
        public int MoveZ { get; private set; } // + = Move North, - = Move South
        public int MoveY { get; private set; } // + = Move Up, - = Move Bellow

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="worldParam">The world Parameters</param>e
        public SingleArrayChunkContainer(VisualWorldParameters visualWorldParam)
        {
            _visualWorldParam = visualWorldParam;

            MoveX = _visualWorldParam.WorldVisibleSize.Y;
            MoveZ = _visualWorldParam.WorldVisibleSize.Y * _visualWorldParam.WorldVisibleSize.X;
            MoveY = 1;

            //Initialize the Big Array
            Cubes = new TerraCube[_visualWorldParam.WorldVisibleSize.X * _visualWorldParam.WorldVisibleSize.Y * _visualWorldParam.WorldVisibleSize.Z];
            _bigArraySize = Cubes.Length;
        }

        /// <summary>
        /// Not mandatory, but will help the Gac to handle the BigArray faster !
        /// </summary>
        public void Dispose()
        {
            Cubes = null;
        }

        /// <summary>
        /// Get the Array Index of a cube
        /// </summary>
        /// <param name="X">world X position</param>
        /// <param name="Y">world Y position</param>
        /// <param name="Z">world Z position</param>
        /// <returns></returns>
        public int Index(ref Vector3I cubePosition)
        {
            int x, z;
            x = cubePosition.X % _visualWorldParam.WorldVisibleSize.X;
            if (x < 0) x += _visualWorldParam.WorldVisibleSize.X;
            z = cubePosition.Z % _visualWorldParam.WorldVisibleSize.Z;
            if (z < 0) z += _visualWorldParam.WorldVisibleSize.Z;

            return x * MoveX + z * MoveZ + cubePosition.Y;
        }

        /// <summary>
        /// Get the Array Index of a cube
        /// </summary>
        /// <param name="X">world X position</param>
        /// <param name="Y">world Y position</param>
        /// <param name="Z">world Z position</param>
        /// <returns></returns>
        public bool Index(ref Vector3I cubePosition, bool isSafe, out int index)
        {
            if (isSafe)
            {
                if (cubePosition.X < _visualWorldParam.WorldRange.Min.X || cubePosition.X >= _visualWorldParam.WorldRange.Max.X || cubePosition.Z < _visualWorldParam.WorldRange.Min.Z || cubePosition.Z >= _visualWorldParam.WorldRange.Max.Z || cubePosition.Y < 0 || cubePosition.Y >= _visualWorldParam.WorldRange.Max.Y)
                {
                    index = int.MaxValue;
                    return false;
                }
            }

            int x, z;
            x = cubePosition.X % _visualWorldParam.WorldVisibleSize.X;
            if (x < 0) x += _visualWorldParam.WorldVisibleSize.X;
            z = cubePosition.Z % _visualWorldParam.WorldVisibleSize.Z;
            if (z < 0) z += _visualWorldParam.WorldVisibleSize.Z;

            index = x * MoveX + z * MoveZ + cubePosition.Y;
            return true;
        }

        /// <summary>
        /// Get the Array Index of a cube
        /// </summary>
        /// <param name="X">world X position</param>
        /// <param name="Y">world Y position</param>
        /// <param name="Z">world Z position</param>
        /// <returns></returns>
        public int Index(int X, int Y, int Z)
        {
            int x, z;
            x = X % _visualWorldParam.WorldVisibleSize.X;
            if (x < 0) x += _visualWorldParam.WorldVisibleSize.X;
            z = Z % _visualWorldParam.WorldVisibleSize.Z;
            if (z < 0) z += _visualWorldParam.WorldVisibleSize.Z;

            return x * MoveX + z * MoveZ + Y;
        }

        /// <summary>
        /// Get big array Index
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <param name="isSafe">Full checks will be applied on the X, Y, and Z values</param>
        /// <param name="index">Result</param>
        /// <returns></returns>
        public bool Index(int X, int Y, int Z, bool isSafe, out int index)
        {
            if (isSafe)
            {
                if (X < _visualWorldParam.WorldRange.Min.X || X >= _visualWorldParam.WorldRange.Max.X || Z < _visualWorldParam.WorldRange.Min.Z || Z >= _visualWorldParam.WorldRange.Max.Z || Y < 0 || Y >= _visualWorldParam.WorldRange.Max.Y)
                {
                    index = int.MaxValue;
                    return false;
                }
            }

            index = Index(X, Y, Z);

            return true;
        }

        /// <summary>
        /// Get the Safe Index of a cube. By Safe it means that the Y value will be check against at the maximum possible to avoid wrapping
        /// </summary>
        /// <param name="X">world X position</param>
        /// <param name="Y">world Y position</param>
        /// <param name="Z">world Z position</param>
        /// <returns></returns>
        public bool IndexSafe(int X, int Y, int Z, out int index)
        {
            if (Y >= AbstractChunk.ChunkSize.Y || Y < 0)
            {
                index = int.MaxValue;
                return false;
            }

            index = Index(X, Y, Z);

            return true;
        }

        /// <summary>
        /// Get the Array Index of a cube
        /// </summary>
        /// <param name="X">world X position</param>
        /// <param name="Y">world Y position</param>
        /// <param name="Z">world Z position</param>
        /// <returns></returns>
        public int IndexMoves(int index, int Modification1, int Modification2)
        {
            index = index + Modification1;
            index = index + Modification2;
            return MakeIndexSafe(index);
        }


        public void SurroundingAxisIndex(int baseIndex, int X, int Y, int Z, Axis workingAxis, int[] Indices, bool ForceIndexSafe = false)
        {
            Indices[BaseIndex] = baseIndex;

            switch (workingAxis)
            {
                case Axis.X:
                    //Fixed X
                    Indices[UpIndex] = FastIndex(baseIndex, Y, IdxRelativeMove.Y_Plus1);
                    Indices[DownIndex] = FastIndex(baseIndex, Y, IdxRelativeMove.Y_Minus1);
                    Indices[LeftIndex] = FastIndex(baseIndex, Z, IdxRelativeMove.Z_Plus1);
                    Indices[RightIndex] = FastIndex(baseIndex, Z, IdxRelativeMove.Z_Minus1);
                    Indices[UpLeftIndex] = FastIndex(Indices[UpIndex], Z + 1, IdxRelativeMove.Z_Plus1);
                    Indices[UpRightIndex] = FastIndex(Indices[UpIndex], Z - 1, IdxRelativeMove.Z_Minus1);
                    Indices[DownLeftIndex] = FastIndex(Indices[DownIndex], Z + 1, IdxRelativeMove.Z_Plus1);
                    Indices[DownRightIndex] = FastIndex(Indices[DownIndex], Z - 1, IdxRelativeMove.Z_Minus1);
                    break;
                case Axis.Y:
                    //Fixed Y
                    Indices[UpIndex] = FastIndex(baseIndex, Z, IdxRelativeMove.Z_Minus1);
                    Indices[DownIndex] = FastIndex(baseIndex, Z, IdxRelativeMove.Z_Plus1);
                    Indices[RightIndex] = FastIndex(baseIndex, X, IdxRelativeMove.X_Plus1);
                    Indices[LeftIndex] = FastIndex(baseIndex, X, IdxRelativeMove.X_Minus1);
                    Indices[UpRightIndex] = FastIndex(Indices[UpIndex], X + 1, IdxRelativeMove.X_Plus1);
                    Indices[UpLeftIndex] = FastIndex(Indices[UpIndex], X - 1, IdxRelativeMove.X_Minus1);
                    Indices[DownRightIndex] = FastIndex(Indices[DownIndex], X + 1, IdxRelativeMove.X_Plus1);
                    Indices[DownLeftIndex] = FastIndex(Indices[DownIndex], X - 1, IdxRelativeMove.X_Minus1);
                    break;
                case Axis.Z:
                    //Fixed Z
                    Indices[UpIndex] = FastIndex(baseIndex, Y, IdxRelativeMove.Y_Plus1);
                    Indices[DownIndex] = FastIndex(baseIndex, Y, IdxRelativeMove.Y_Minus1);
                    Indices[RightIndex] = FastIndex(baseIndex, X, IdxRelativeMove.X_Plus1);
                    Indices[LeftIndex] = FastIndex(baseIndex, X, IdxRelativeMove.X_Minus1);
                    Indices[UpRightIndex] = FastIndex(Indices[UpIndex], X + 1, IdxRelativeMove.X_Plus1);
                    Indices[UpLeftIndex] = FastIndex(Indices[UpIndex], X - 1, IdxRelativeMove.X_Minus1);
                    Indices[DownRightIndex] = FastIndex(Indices[DownIndex], X + 1, IdxRelativeMove.X_Plus1);
                    Indices[DownLeftIndex] = FastIndex(Indices[DownIndex], X - 1, IdxRelativeMove.X_Minus1);
                    break;
                default:
                    break;
            }

            //Validate the index
            for (int i = 0; i < 9; i++)
            {
                if (ForceIndexSafe)
                {
                    Indices[i] = MakeIndexSafe(Indices[i]);
                }
                else
                {
                    if (Indices[i] < 0 || Indices[i] >= _bigArraySize) Indices[i] = int.MaxValue;
                }
            }

        }

        /// <summary>
        /// Get the Array Index of a cube
        /// </summary>
        /// <param name="X">world X position</param>
        /// <param name="Y">world Y position</param>
        /// <param name="Z">world Z position</param>
        /// <returns></returns>
        public int IndexMoves(int index, int Modification1, int Modification2, int Modification3)
        {
            index = index + Modification1;
            index = index + Modification2;
            index = index + Modification3;
            return MakeIndexSafe(index);
        }

        public bool isIndexInError(int index)
        {
            if (index == int.MaxValue) return true;
            return false;
        }

        /// <summary>
        /// The index will be forced to be inside the big array
        /// !!!! WARNING : This could send back "wrong" index !!!!
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int MakeIndexSafe(int index)
        {
            index = index % _bigArraySize;
            if (index < 0) index += _bigArraySize;
            return index;
        }

        public bool MakeIndexSafe(int index, out int indexResult)
        {
            indexResult = index % _bigArraySize;
            if (indexResult < 0) indexResult += _bigArraySize;
            if (indexResult != index) return true;
            return false;
        }

        public int FastIndex(int baseIndex, int Position, IdxRelativeMove idxmove, bool validated = false)
        {
            int value;
            switch (idxmove)
            {
                case IdxRelativeMove.None:
                    return baseIndex;
                case IdxRelativeMove.X_Minus1:
                    value = baseIndex - _visualWorldParam.WorldVisibleSize.Y;
                    if (Position == _visualWorldParam.WrapEnd.X) value += _visualWorldParam.WorldVisibleSizeXY; //_visualWorldParam.WorldVisibleSize.X * _visualWorldParam.WorldVisibleSize.Y;
                    break;
                case IdxRelativeMove.X_Plus1:
                    value = baseIndex + _visualWorldParam.WorldVisibleSize.Y;
                    if (Position == _visualWorldParam.WrapEnd.X - 1) value -= _visualWorldParam.WorldVisibleSizeXY; //_visualWorldParam.WorldVisibleSize.X * _visualWorldParam.WorldVisibleSize.Y;
                    break;
                case IdxRelativeMove.Z_Minus1:
                    value = baseIndex - _visualWorldParam.WorldVisibleSize.X * _visualWorldParam.WorldVisibleSize.Y;
                    if (Position == _visualWorldParam.WrapEnd.Y) value += _visualWorldParam.WorldVisibleSizeXYZ; //_visualWorldParam.WorldVisibleSize.X * _visualWorldParam.WorldVisibleSize.Y * _visualWorldParam.WorldVisibleSize.Z;
                    break;
                case IdxRelativeMove.Z_Plus1:
                    value = baseIndex + _visualWorldParam.WorldVisibleSize.X * _visualWorldParam.WorldVisibleSize.Y;
                    if (Position == _visualWorldParam.WrapEnd.Y - 1) value -= _visualWorldParam.WorldVisibleSizeXYZ; // _visualWorldParam.WorldVisibleSize.X * _visualWorldParam.WorldVisibleSize.Y * _visualWorldParam.WorldVisibleSize.Z;
                    break;
                case IdxRelativeMove.Y_Minus1:
                    value = baseIndex - 1;
                    break;
                case IdxRelativeMove.Y_Plus1:
                    value = baseIndex + 1;
                    break;
                default:
                    value = int.MaxValue;
                    break;
            }

            if (validated)
            {
                value = value % _bigArraySize;
                if (value < 0) value += _bigArraySize;
            }

            return value;
        }

        public SurroundingIndex[] GetSurroundingBlocksIndex(int baseIndex, int CubeXCoord, int CubeYCoord, int CubeZCoord)
        {
            int cubeIndex = baseIndex;
            SurroundingIndex[] surroundingIndexes = new SurroundingIndex[6];

            surroundingIndexes[0] = new SurroundingIndex() { Index = FastIndex(cubeIndex, CubeXCoord, IdxRelativeMove.X_Plus1), IndexRelativePosition = IdxRelativeMove.X_Plus1, Position = new Vector3I(CubeXCoord + 1, CubeYCoord, CubeZCoord) };
            surroundingIndexes[1] = new SurroundingIndex() { Index = FastIndex(cubeIndex, CubeXCoord, IdxRelativeMove.X_Minus1), IndexRelativePosition = IdxRelativeMove.X_Minus1, Position = new Vector3I(CubeXCoord - 1, CubeYCoord, CubeZCoord) };
            surroundingIndexes[2] = new SurroundingIndex() { Index = FastIndex(cubeIndex, CubeZCoord, IdxRelativeMove.Z_Plus1), IndexRelativePosition = IdxRelativeMove.Z_Plus1, Position = new Vector3I(CubeXCoord, CubeYCoord, CubeZCoord + 1) };
            surroundingIndexes[3] = new SurroundingIndex() { Index = FastIndex(cubeIndex, CubeZCoord, IdxRelativeMove.Z_Minus1), IndexRelativePosition = IdxRelativeMove.Z_Minus1, Position = new Vector3I(CubeXCoord, CubeYCoord, CubeZCoord - 1) };
            surroundingIndexes[4] = new SurroundingIndex() { Index = cubeIndex + MoveY , IndexRelativePosition = IdxRelativeMove.Y_Plus1, Position = new Vector3I(CubeXCoord, CubeYCoord + 1, CubeZCoord) };
            surroundingIndexes[5] = new SurroundingIndex() { Index = cubeIndex - MoveY , IndexRelativePosition = IdxRelativeMove.Y_Minus1, Position = new Vector3I(CubeXCoord, CubeYCoord - 1, CubeZCoord) };
            return surroundingIndexes;
        }

        public SurroundingIndex[] GetSurroundingBlocksIndex(int CubeXCoord, int CubeYCoord, int CubeZCoord)
        {
            int cubeIndex = Index(CubeXCoord, CubeYCoord, CubeZCoord);
            return GetSurroundingBlocksIndex(cubeIndex, CubeXCoord, CubeYCoord, CubeZCoord);
        }

        public SurroundingIndex[] GetSurroundingBlocksIndex(ref Vector3I CubeCoordinates)
        {
            return GetSurroundingBlocksIndex(CubeCoordinates.X, CubeCoordinates.Y, CubeCoordinates.Z);
        }

        public bool isPickable(ref Vector3 position, out TerraCube cube)
        {
            int cubeIndex;

            if (Index(MathHelper.Fastfloor(position.X), MathHelper.Fastfloor(position.Y), MathHelper.Fastfloor(position.Z), true, out cubeIndex))
            {
                cube = Cubes[cubeIndex];
                // Simon disabled this, i dont want it and method was not in use :  if (Cubes[cubeIndex].Id == CubeId.Air) cube = new TerraCube(CubeId.Error);
                return CubeProfile.CubesProfile[cube.Id].IsPickable;
            }

            cube = new TerraCube();
            return false;
        }

        public bool isPickable(ref Vector3D position, out TerraCube cube)
        {
            int cubeIndex;

            if (Index(MathHelper.Fastfloor(position.X), MathHelper.Fastfloor(position.Y), MathHelper.Fastfloor(position.Z), true, out cubeIndex))
            {
                cube = Cubes[cubeIndex];
                return CubeProfile.CubesProfile[cube.Id].IsPickable;
            }

            cube = new TerraCube();
            return false;
        }

        public bool isPickable(ref Vector3D position, out TerraCubeWithPosition cubewithPosition)
        {
            int cubeIndex;

            Vector3I cubePosition = new Vector3I(MathHelper.Fastfloor(position.X), MathHelper.Fastfloor(position.Y), MathHelper.Fastfloor(position.Z));

            if (Index(ref cubePosition, true, out cubeIndex))
            {
                cubewithPosition = new TerraCubeWithPosition(cubePosition, Cubes[cubeIndex]);
                return CubeProfile.CubesProfile[cubewithPosition.Cube.Id].IsPickable;
            }

            cubewithPosition = new TerraCubeWithPosition();
            return false;
        }

        public bool isPickable(ref Vector3 position, Tool withTool, out TerraCube cube)
        {
            int cubeIndex;

            if (Index(MathHelper.Fastfloor(position.X), MathHelper.Fastfloor(position.Y), MathHelper.Fastfloor(position.Z), true, out cubeIndex))
            {
                cube = Cubes[cubeIndex];
                return CubeProfile.CubesProfile[cube.Id].IsPickable;
            }

            cube = new TerraCube();
            return false;
        }


        public bool isPickable(ref Vector3 position)
        {
            int cubeIndex;
            if (Index(MathHelper.Fastfloor(position.X), MathHelper.Fastfloor(position.Y), MathHelper.Fastfloor(position.Z), true, out cubeIndex))
            {
                return CubeProfile.CubesProfile[Cubes[cubeIndex].Id].IsPickable;
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
                        if (IndexSafe(x, y, z, out index))
                        {
                            if (CubeProfile.CubesProfile[Cubes[index].Id].IsSolidToEntity)
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
                        if (IndexSafe(x, y, z, out index))
                        {
                            if (CubeProfile.CubesProfile[Cubes[index].Id].IsSolidToEntity)
                            {
                                collidingcube.Cube = Cubes[index];
                                collidingcube.Position = new Vector3I(x, y, z);
                                return true;
                            }
                        }
                    }
                }
            }

            collidingcube = new TerraCubeWithPosition();
            return false;
        }

        public void GetNextSolidBlockToPlayer(ref Vector3 FromPosition, ref Vector3I Direction, out TerraCubeWithPosition cubeWithPosition)
        {
            int index = 0;
            cubeWithPosition.Cube = new TerraCube(CubeId.Air);

            int X = MathHelper.Fastfloor(FromPosition.X);
            int Z = MathHelper.Fastfloor(FromPosition.Z);
            int Y = MathHelper.Fastfloor(FromPosition.Y);

            if (Y >= _visualWorldParam.WorldVisibleSize.Y) Y = _visualWorldParam.WorldVisibleSize.Y - 1;

            while (!CubeProfile.CubesProfile[cubeWithPosition.Cube.Id].IsSolidToEntity && !isIndexInError(index))
            {
                if (IndexSafe(X, Y, Z, out index))
                {
                    if (CubeProfile.CubesProfile[Cubes[index].Id].IsSolidToEntity)
                    {
                        cubeWithPosition.Cube = Cubes[index];
                        break;
                    }
                    X += Direction.X;
                    Y += Direction.Y;
                    Z += Direction.Z;
                }
            }


            cubeWithPosition.Position = new Vector3I(X, Y, Z);
        }

        public void GetNextSolidBlockToPlayer(ref BoundingBox FromBBPosition, ref Vector3I Direction, out TerraCubeWithPosition cubeWithPosition)
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


        public void SetCube(ref Vector3I cubeCoordinates, ref TerraCube cube)
        {
            int index = Index(cubeCoordinates.X, cubeCoordinates.Y, cubeCoordinates.Z);
            Cubes[index] = cube;

            if (BlockDataChanged != null) BlockDataChanged(this, new ChunkDataProviderDataChangedEventArgs { Count = 1, Locations = new[] { cubeCoordinates }, Bytes = new[] { cube.Id } });
        }

        public void SetCube(int X, int Y, int Z, ref TerraCube cube)
        {
            int index = Index(X, Y, Z);
            Cubes[index] = cube;

            if (BlockDataChanged != null) BlockDataChanged(this, new ChunkDataProviderDataChangedEventArgs { Count = 1, Locations = new[] { new Vector3I { X = X, Y = Y, Z = Z } }, Bytes = new[] { cube.Id } });
        }
    }
}
