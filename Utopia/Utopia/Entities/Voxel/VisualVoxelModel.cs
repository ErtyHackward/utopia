using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using Utopia.Shared.Entities.Models;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// Pure client class that holds voxelmodel instance data and wraps the common voxel model
    /// </summary>
    public class VisualVoxelModel
    {
        private readonly VisualVoxelEntity _parent;
        private readonly VoxelModel _model;
        private readonly VoxelMeshFactory _voxelMeshFactory;

        private VisualVoxelPart[] _visualParts;

        public int ActiveState { get; set; }

        /// <summary>
        /// Gets current wrapped voxel model
        /// </summary>
        public VoxelModel VoxelModel
        {
            get { return _model; }
        }

        public VisualVoxelModel(VisualVoxelEntity parent, VoxelModel model, VoxelMeshFactory voxelMeshFactory)
        {
            _parent = parent;
            _model = model;
            _voxelMeshFactory = voxelMeshFactory;

            _visualParts = new VisualVoxelPart[model.Parts.Count];
        }

        /// <summary>
        /// Creation of the vertices
        /// </summary>
        public void LoadContent()
        {
            for (int i = 0; i < _visualParts.Length; i++)
            {
                var part = new VisualVoxelPart();
                
                part.VertexBuffers = new VertexBuffer<VertexVoxel>[_model.Parts[i].Frames.Count];
                part.IndexBuffers = new IndexBuffer<ushort>[_model.Parts[i].Frames.Count];

                for (int j = 0; j < _model.Parts[i].Frames.Count; j++)
                {
                    List<VertexVoxel> vertices;
                    List<ushort> indices;

                    _voxelMeshFactory.GenerateVoxelFaces(_model.Parts[i].Frames[j].BlockData, out vertices, out indices);

                    part.VertexBuffers[j] = _voxelMeshFactory.InitBuffer(vertices);
                    part.IndexBuffers[j] = _voxelMeshFactory.InitBuffer(indices);
                }
                
                _visualParts[i] = part;
            }
        }

        public void Draw()
        {

        }
    }

    public class VisualVoxelPart
    {
        public VertexBuffer<VertexVoxel>[] VertexBuffers;
        public IndexBuffer<ushort>[] IndexBuffers;
    }
}
