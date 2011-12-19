using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using Utopia.Shared.Entities.Models;
using UtopiaContent.Effects.Entities;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// Pure client class that holds voxelmodel instance data and wraps the common voxel model
    /// </summary>
    public class VisualVoxelModel
    {
        private readonly VoxelModel _model;
        private readonly VoxelMeshFactory _voxelMeshFactory;
        private VisualVoxelPart[] _visualParts;

        private bool _initialized;

        public bool Initialized
        {
            get { return _initialized; }
        }
        
        /// <summary>
        /// Gets current wrapped voxel model
        /// </summary>
        public VoxelModel VoxelModel
        {
            get { return _model; }
        }

        /// <summary>
        /// Gets array of visual data (verices and indices and bounding boxes)
        /// </summary>
        public VisualVoxelPart[] VisualVoxelParts
        {
            get { return _visualParts; }
        }
    

        public VisualVoxelModel(VoxelModel model, VoxelMeshFactory voxelMeshFactory)
        {
            _model = model;
            _voxelMeshFactory = voxelMeshFactory;
            
        }

        /// <summary>
        /// Creation of the vertices
        /// </summary>
        public void BuildMesh()
        {
            var minPoint = new Vector3();
            var maxPoint = new Vector3();

            _visualParts = new VisualVoxelPart[_model.Parts.Count];

            for (int i = 0; i < _visualParts.Length; i++)
            {
                var part = new VisualVoxelPart();
                
                part.VertexBuffers = new VertexBuffer<VertexVoxel>[_model.Parts[i].Frames.Count];
                part.IndexBuffers = new IndexBuffer<ushort>[_model.Parts[i].Frames.Count];
                part.BoundingBoxes = new BoundingBox[_model.Parts[i].Frames.Count];

                for (int j = 0; j < _model.Parts[i].Frames.Count; j++)
                {
                    List<VertexVoxel> vertices;
                    List<ushort> indices;

                    _voxelMeshFactory.GenerateVoxelFaces(_model.Parts[i].Frames[j].BlockData, out vertices, out indices);

                    part.VertexBuffers[j] = _voxelMeshFactory.InitBuffer(vertices);
                    part.IndexBuffers[j] = _voxelMeshFactory.InitBuffer(indices);
                    part.BoundingBoxes[j] = new BoundingBox(new Vector3(), _model.Parts[i].Frames[j].BlockData.ChunkSize);
                }
                
                _visualParts[i] = part;
            }

            for (int index = 0; index < _model.States.Count; index++)
            {
                var state = _model.States[index];

                // calculate bounding boxes for each part state
                for (int i = 0; i < state.PartsStates.Length; i++)
                {
                    var partState = state.PartsStates[i];
                    var bb = _visualParts[i].BoundingBoxes[partState.ActiveFrame];

                    bb.Minimum = Vector3.TransformCoordinate(bb.Minimum, partState.Transform);
                    bb.Maximum = Vector3.TransformCoordinate(bb.Maximum, partState.Transform);

                    partState.BoundingBox = bb;

                    minPoint = Vector3.Min(minPoint, bb.Minimum);
                    minPoint = Vector3.Min(minPoint, bb.Maximum);
                    maxPoint = Vector3.Max(maxPoint, bb.Maximum);
                    maxPoint = Vector3.Max(maxPoint, bb.Minimum);
                }
                
                state.BoundingBox = new BoundingBox(minPoint, maxPoint);
            }
            
            _initialized = true;
        }

        public void Draw(HLSLVoxelModel effect)
        {
            if (!_initialized) return;

            var state = _model.States[0];

            if (_model.ColorMapping != null)
            {
                effect.CBPerFrame.Values.ColorMapping = _model.ColorMapping.BlockColors;
                effect.CBPerFrame.IsDirty = true;
            }

            // draw each part of the model
            for (int i = 0; i < state.PartsStates.Length; i++)
            {
                var voxelModelPartState = state.PartsStates[i];

                var vb = _visualParts[i].VertexBuffers[voxelModelPartState.ActiveFrame];
                var ib = _visualParts[i].IndexBuffers[voxelModelPartState.ActiveFrame];

                vb.SetToDevice(0);
                ib.SetToDevice(0);

                if (_model.Parts[i].ColorMapping != null)
                {
                    effect.CBPerFrame.Values.ColorMapping = _model.Parts[i].ColorMapping.BlockColors;
                    effect.CBPerFrame.IsDirty = true;
                }

                effect.CBPerPart.Values.Transform = Matrix.Transpose(voxelModelPartState.Transform);
                effect.CBPerPart.IsDirty = true;
                effect.Apply();

                _voxelMeshFactory.Engine.Context.DrawIndexed(ib.IndicesCount, 0, 0);
            }
        }
    }

    public class VisualVoxelPart
    {
        public BoundingBox[] BoundingBoxes;
        public VertexBuffer<VertexVoxel>[] VertexBuffers;
        public IndexBuffer<ushort>[] IndexBuffers;
    }
}
