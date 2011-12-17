using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Shared.Entities.Models;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// Pure client class that holds voxelmodel instance data and wraps the common voxel model
    /// </summary>
    public class VisualVoxelModel
    {
        private readonly VoxelModel _model;
        private readonly VoxelMeshFactory _voxelMeshFactory;

        public int ActiveState { get; set; }

        /// <summary>
        /// Gets current wrapped voxel model
        /// </summary>
        public VoxelModel VoxelModel
        {
            get { return _model; }
        }

        public VisualVoxelModel(VoxelModel model, VoxelMeshFactory voxelMeshFactory)
        {
            _model = model;
            _voxelMeshFactory = voxelMeshFactory;

            
        }


    }
}
