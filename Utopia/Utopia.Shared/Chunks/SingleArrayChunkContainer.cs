using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.World;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Class responsible to manage the acces to the circular array containing the Cubes
    /// </summary>
    public class SingleArrayChunkContainer
    {
        private int _bigArraySize;

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
        /// Visible World size in cubes Unit
        /// </summary>
        public Location3<int> _visibleWorldSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="worldParam">The world Parameters</param>e
        public SingleArrayChunkContainer(WorldParameters worldParam)
        {
            _visibleWorldSize = new Location3<int>()
            {
                X = AbstractChunk.ChunkSize.X * worldParam.WorldSize.X,
                Y = AbstractChunk.ChunkSize.Y,
                Z = AbstractChunk.ChunkSize.Z * worldParam.WorldSize.Z,
            };

            MoveX = _visibleWorldSize.Y;
            MoveZ = _visibleWorldSize.Y * _visibleWorldSize.X;
            MoveY = 1;

            //Initialize the Big Array
            Cubes = new TerraCube[_visibleWorldSize.X * _visibleWorldSize.Y * _visibleWorldSize.Z];
            _bigArraySize = Cubes.Length;
        }

        /// <summary>
        /// Get the Array Index of a cube
        /// </summary>
        /// <param name="X">world X position</param>
        /// <param name="Y">world Y position</param>
        /// <param name="Z">world Z position</param>
        /// <returns></returns>
        public int Index(ref Location3<int> cubePosition)
        {
            int x, z;
            x = cubePosition.X % _visibleWorldSize.X;
            if (x < 0) x += _visibleWorldSize.X;
            z = cubePosition.Z % _visibleWorldSize.Z;
            if (z < 0) z += _visibleWorldSize.Z;

            return x * MoveX + z * MoveZ + cubePosition.Y;
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
            x = X % _visibleWorldSize.X;
            if (x < 0) x += _visibleWorldSize.X;
            z = Z % _visibleWorldSize.Z;
            if (z < 0) z += _visibleWorldSize.Z;

            return x * MoveX + z * MoveZ + Y;
        }

        /// <summary>
        /// Get the Array Index of a cube
        /// </summary>
        /// <param name="X">world X position</param>
        /// <param name="Y">world Y position</param>
        /// <param name="Z">world Z position</param>
        /// <returns></returns>
        public int IndexMoveValidated(int index, int Modification1, int Modification2)
        {
            index = ValidateIndex(index + Modification1);
            index = ValidateIndex(index + Modification2);
            return index;
        }

        /// <summary>
        /// Get the Array Index of a cube
        /// </summary>
        /// <param name="X">world X position</param>
        /// <param name="Y">world Y position</param>
        /// <param name="Z">world Z position</param>
        /// <returns></returns>
        public int IndexMoveValidated(int index, int Modification1, int Modification2, int Modification3)
        {
            index = ValidateIndex(index + Modification1);
            index = ValidateIndex(index + Modification2);
            index = ValidateIndex(index + Modification3);
            return index;
        }

        public int ValidateIndex(int index)
        {
            //int i = neightborCubeIndex;
            //if (i >= _cubesHolder.Cubes.Length) i -= _cubesHolder.Cubes.Length;
            //if (i < 0) i += _cubesHolder.Cubes.Length;

            if (index < 0) index += _bigArraySize;
            else
            {
                if (index >= _bigArraySize) index -= _bigArraySize;
            }

            return index;
        }

        public void SetCube(ref Location3<int> cubeCoordinates, ref TerraCube cube)
        {
            int index = Index(cubeCoordinates.X, cubeCoordinates.Y, cubeCoordinates.Z);
            Cubes[index] = cube;

            if (BlockDataChanged != null) BlockDataChanged(this, new ChunkDataProviderDataChangedEventArgs { Count = 1, Locations = new[] { cubeCoordinates }, Bytes = new[] { cube.Id } });
        }

        public void SetCube(int X, int Y, int Z, ref TerraCube cube)
        {
            int index = Index(X, Y, Z);
            Cubes[index] = cube;

            if (BlockDataChanged != null) BlockDataChanged(this, new ChunkDataProviderDataChangedEventArgs { Count = 1, Locations = new[] { new Location3<int> { X = X, Y = Y, Z = Z } }, Bytes = new[] { cube.Id } });
        }

    }
}
