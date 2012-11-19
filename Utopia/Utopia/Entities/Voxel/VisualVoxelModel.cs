using System.Collections.Generic;
using System.Linq;
using SharpDX;
using Utopia.Shared.Entities.Models;
using UtopiaContent.Effects.Entities;
using Utopia.Shared.ClassExt;
using S33M3Resources.Structs.Vertex;
using S33M3DXEngine.Buffers;
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

            foreach (var voxelModelState in VoxelModel.States)
            {
                voxelModelState.PartsStates.RemoveAt(index);
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

            List<VertexVoxelInstanced> vertices;
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
                // dispose prevoious DX data
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
                
                part.VertexBuffers = new InstancedVertexBuffer<VertexVoxelInstanced, VoxelInstanceData>[_model.Parts[i].Frames.Count];
                part.IndexBuffers = new IndexBuffer<ushort>[_model.Parts[i].Frames.Count];
                part.BoundingBoxes = new BoundingBox[_model.Parts[i].Frames.Count];

                for (int j = 0; j < _model.Parts[i].Frames.Count; j++)
                {
                    List<VertexVoxelInstanced> vertices;
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
        /// Draws one model using its instance data
        /// </summary>
        /// <param name="context"></param>
        /// <param name="effect"></param>
        /// <param name="instance"></param>
        public void Draw(DeviceContext context, HLSLVoxelModel effect, VoxelModelInstance instance)
        {
            if (!_initialized) return;

            var state = instance.State;

            if (_model.ColorMapping != null)
            {
                effect.CBPerModel.Values.ColorMapping = _model.ColorMapping.BlockColors;
            }

            effect.CBPerModel.Values.World = Matrix.Transpose(instance.World);
            effect.CBPerModel.Values.LightColor = instance.LightColor;
            effect.CBPerModel.IsDirty = true;

            // draw each part of the model
            for (int i = 0; i < state.PartsStates.Count; i++)
            {
                var voxelModelPartState = state.PartsStates[i];

                if (_visualParts[i] == null) continue;

                if (voxelModelPartState.ActiveFrame == byte.MaxValue)
                    continue;

                var vb = _visualParts[i].VertexBuffers[voxelModelPartState.ActiveFrame];
                var ib = _visualParts[i].IndexBuffers[voxelModelPartState.ActiveFrame];

                vb.SetToDevice(context, 0);
                ib.SetToDevice(context, 0);

                if (_model.Parts[i].ColorMapping != null)
                {
                    effect.CBPerModel.Values.ColorMapping = _model.Parts[i].ColorMapping.BlockColors;
                    effect.CBPerModel.IsDirty = true;
                }

                if (_model.Parts[i].IsHead)
                {
                    var bb = _visualParts[i].BoundingBoxes[voxelModelPartState.ActiveFrame];
                    RotateHead(voxelModelPartState, instance, bb, out effect.CBPerPart.Values.Transform);
                    effect.CBPerPart.Values.Transform = Matrix.Transpose(effect.CBPerPart.Values.Transform);
                }
                else
                {
                    effect.CBPerPart.Values.Transform = Matrix.Transpose(voxelModelPartState.GetTransformation() * Matrix.RotationQuaternion(instance.Rotation));
                }

                effect.CBPerPart.IsDirty = true;
                effect.Apply(context);

                context.DrawIndexed(ib.IndicesCount, 0, 0);
            }
        }

        private void RotateHead(VoxelModelPartState partState, VoxelModelInstance instance, BoundingBox bb, out Matrix result)
        {
            var move = (bb.Maximum - bb.Minimum) / 2;

            var partTransform = partState.GetTransformation();
            
            // get the point of the head center
            move = Vector3.TransformCoordinate(move, partTransform);
            
            // 1. apply model part transform
            // 2. move to the head center
            // 3. rotate the head
            // 4. apply the translation again.

            result = partTransform *
                     Matrix.Translation(-move) *
                     Matrix.RotationQuaternion(instance.HeadRotation) *
                     Matrix.Translation(move);
        }

        private void DrawGroup(byte activeFrame, int partIndex, IList<VoxelModelInstance> instances)
        {
            // create instance data block that will be passed to the shader
            //var instanceData = instances.Select(ins => new VoxelInstanceData { Transform = ins.World, LightColor = ins.LightColor }).ToArray();
            VoxelInstanceData[] instanceData = new VoxelInstanceData[instances.Count];

            // update shader instance data by model instance variables
            for (int instanceIndex = 0; instanceIndex < instances.Count; instanceIndex++)
            {
                var instance = instances[instanceIndex];
                var state = instance.State;
                var voxelModelPartState = state.PartsStates[partIndex];

                instanceData[instanceIndex].LightColor = instance.LightColor;

                // apply rotations from the state and instance (if the head)
                if (_model.Parts[partIndex].IsHead)
                {
                    var bb = _visualParts[partIndex].BoundingBoxes[activeFrame];
                    RotateHead(voxelModelPartState, instance, bb, out instanceData[instanceIndex].Transform);
                }
                else
                {
                    instanceData[instanceIndex].Transform = voxelModelPartState.GetTransformation() * Matrix.RotationQuaternion(instance.Rotation);
                }

                instanceData[instanceIndex].Transform = Matrix.Transpose(instanceData[instanceIndex].Transform * instance.World);
            }

            var vb = _visualParts[partIndex].VertexBuffers[activeFrame];
            var ib = _visualParts[partIndex].IndexBuffers[activeFrame];

            vb.SetInstancedData(_voxelMeshFactory.Engine.ImmediateContext, instanceData);

            vb.SetToDevice(_voxelMeshFactory.Engine.ImmediateContext, 0);
            ib.SetToDevice(_voxelMeshFactory.Engine.ImmediateContext, 0);

            _voxelMeshFactory.Engine.ImmediateContext.DrawIndexedInstanced(ib.IndicesCount, instanceData.Length, 0, 0, 0);
        }

        /// <summary>
        /// Performs instanced drawing of the group of models (should be the instances of the same model)
        /// </summary>
        /// <param name="context"> </param>
        /// <param name="effect"></param>
        /// <param name="instances"></param>
        public void DrawInstanced(DeviceContext context, HLSLVoxelModelInstanced effect, IEnumerable<VoxelModelInstance> instances)
        {
            if (!_initialized) return;

            var instancesList = instances.ToArray();
            if (instancesList.Length == 0) return;

            if (_model.ColorMapping != null)
            {
                effect.CBPerModel.Values.ColorMapping = _model.ColorMapping.BlockColors;
                effect.CBPerModel.IsDirty = true;
            }

            effect.Apply(context);

            // we need to draw each part separately
            for (int partIndex = 0; partIndex < VoxelModel.Parts.Count; partIndex++)
            {
                if (VoxelModel.Parts[partIndex].Frames.Count == 1 || VoxelModel.States.Count == 1)
                {
                    // we have only one frame or state, so every model have the same VB
                    DrawGroup(instancesList[0].State.PartsStates[partIndex].ActiveFrame, partIndex, instancesList);
                }
                else
                {
                    // group instances by active frame because they have different VB
                    var groups = from inst in instancesList
                                 group inst by inst.State.PartsStates[partIndex].ActiveFrame
                                 into g select new {FrameIndex = g.Key, Instances = g.AsEnumerable().ToArray()};

                    foreach (var g in groups)
                    {
                        DrawGroup(g.FrameIndex, partIndex, g.Instances);
                    }
                }
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
        public InstancedVertexBuffer<VertexVoxelInstanced, VoxelInstanceData>[] VertexBuffers;
        public IndexBuffer<ushort>[] IndexBuffers;
    }

    public struct VoxelInstanceData
    {
        public Matrix Transform;
        public Color3 LightColor;
    }
}
