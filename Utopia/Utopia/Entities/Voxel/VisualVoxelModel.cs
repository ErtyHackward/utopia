using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Shared.Entities.Models;
using UtopiaContent.Effects.Entities;
using Utopia.Shared.ClassExt;
using S33M3_Resources.Struct.Vertex;
using S33M3_DXEngine.Buffers;
using SharpDX.Direct3D11;

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

        /// <summary>
        /// Gets a value indicating if the model have a dx mesh and can be displayed. Call BuildMesh to get model initialized
        /// </summary>
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
        /// Gets array of visual data (verices and indices and transformed bounding boxes)
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
        /// Removes a part from the model, free dx resources
        /// </summary>
        /// <param name="index"></param>
        public void RemovePartAt(int index)
        {
            var vp = _visualParts[index];

            // free all frames buffers
            for (int i = 0; i < vp.VertexBuffers.Length; i++)
            {
                vp.VertexBuffers[i].Dispose();
                vp.IndexBuffers[i].Dispose();
            }

            ArrayHelper.RemoveAt(ref _visualParts, index);
        }

        /// <summary>
        /// Re-generates vertices for a model part frame
        /// </summary>
        /// <param name="partIndex"></param>
        /// <param name="frameIndex"></param>
        public void RebuildFrame(int partIndex, int frameIndex)
        {
            var part = _visualParts[partIndex];

            List<VertexVoxel> vertices;
            List<ushort> indices;

            _voxelMeshFactory.GenerateVoxelFaces(_model.Parts[partIndex].Frames[frameIndex].BlockData, out vertices, out indices);

            part.VertexBuffers[frameIndex].Dispose();
            part.IndexBuffers[frameIndex].Dispose();

            part.VertexBuffers[frameIndex] = _voxelMeshFactory.InitBuffer(vertices);
            part.IndexBuffers[frameIndex] = _voxelMeshFactory.InitBuffer(indices);

            part.BoundingBoxes[frameIndex] = new BoundingBox(new Vector3(), _model.Parts[partIndex].Frames[frameIndex].BlockData.ChunkSize);

        }

        /// <summary>
        /// Performs (re)creation of the vertices
        /// </summary>
        public void BuildMesh()
        {
            if (_visualParts != null)
            {
                // dispose prevoious dx data
                foreach (var visualVoxelPart in _visualParts)
                {
                    for (int i = 0; i < visualVoxelPart.VertexBuffers.Length; i++)
                    {
                        visualVoxelPart.VertexBuffers[i].Dispose();
                        visualVoxelPart.IndexBuffers[i].Dispose();
                    }
                }

            }


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

            foreach (var state in _model.States)
            {
                state.UpdateBoundingBox();
            }
            
            _initialized = true;
        }

        /// <summary>
        /// Draws a model with default state, to perform real drawing use VoxelModelInstance class
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="state"></param>
        public void Draw(DeviceContext context, HLSLVoxelModel effect, VoxelModelState state = null)
        {
            if (!_initialized) return;

            if(state == null)
                state = _model.States[0];

            if (_model.ColorMapping != null)
            {
                effect.CBPerFrame.Values.ColorMapping = _model.ColorMapping.BlockColors;
                effect.CBPerFrame.IsDirty = true;
            }

            // draw each part of the model
            for (int i = 0; i < state.PartsStates.Count; i++)
            {
                var voxelModelPartState = state.PartsStates[i];

                var vb = _visualParts[i].VertexBuffers[voxelModelPartState.ActiveFrame];
                var ib = _visualParts[i].IndexBuffers[voxelModelPartState.ActiveFrame];

                vb.SetToDevice(context, 0);
                ib.SetToDevice(context, 0);

                if (_model.Parts[i].ColorMapping != null)
                {
                    effect.CBPerFrame.Values.ColorMapping = _model.Parts[i].ColorMapping.BlockColors;
                    effect.CBPerFrame.IsDirty = true;
                }

                effect.CBPerPart.Values.Transform = Matrix.Transpose(voxelModelPartState.Transform);
                effect.CBPerPart.IsDirty = true;
                effect.Apply(context);

                _voxelMeshFactory.Engine.ImmediateContext.DrawIndexed(ib.IndicesCount, 0, 0);
            }
        }

        public override string ToString()
        {
            return _model.Name;
        }
    }

    public class VisualVoxelPart
    {
        public BoundingBox[] BoundingBoxes;
        public VertexBuffer<VertexVoxel>[] VertexBuffers;
        public IndexBuffer<ushort>[] IndexBuffers;
    }
}
