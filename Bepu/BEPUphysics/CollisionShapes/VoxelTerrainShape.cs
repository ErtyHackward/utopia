using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BEPUphysics.CollisionShapes
{
    /// <summary>
    /// Represents a voxel terrain
    /// </summary>
    public class VoxelTerrainShape : CollisionShape
    {
        private byte[, ,] _voxels;

        public byte[, ,] Voxels 
        { 
            get 
            { 
                return _voxels; 
            }
            set { _voxels = value; }
        }
    }
}
