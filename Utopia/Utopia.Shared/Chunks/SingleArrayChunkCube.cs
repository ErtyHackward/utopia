using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Math;
using Utopia.Shared.World;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Class responsible to manage the acces to the circular array containing the Cubes
    /// </summary>
    public class SingleArrayChunkCube
    {
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
        private Location3<int> _visibleWorldSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="worldParam">The world Parameters</param>
        public SingleArrayChunkCube(WorldParameters worldParam)
        {
            _visibleWorldSize = new Location3<int>()
            {
                X = worldParam.ChunkSize.X * worldParam.WorldSize.X,
                Y = worldParam.ChunkSize.Y,
                Z = worldParam.ChunkSize.Z * worldParam.WorldSize.Z,
            };

            MoveX = _visibleWorldSize.Y;
            MoveZ = _visibleWorldSize.Y * _visibleWorldSize.X;
            MoveY = 1;
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

    }
}
