using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.ClassExt;
using S33M3Resources.Structs.Vertex;
using S33M3DXEngine.Buffers;
using SharpDX.Direct3D11;
using Utopia.Resources.Effects.Entities;
using S33M3Resources.VertexFormats.Interfaces;
using S33M3DXEngine.VertexFormat;
using SharpDX.DXGI;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// Pure client class that holds voxelmodel instance data and wraps the common voxel model
    /// </summary>
    public class VisualVoxelModel
    {
        private readonly VoxelModel _model;
        private readonly VoxelMeshFactory _voxelMeshFactory;
        private VisualVoxelFrame[] _visualFrames;

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
        public VisualVoxelFrame[] VisualVoxelFrames
        {
            get { return _visualFrames; }
        }

        public VisualVoxelModel(VoxelModel model, VoxelMeshFactory voxelMeshFactory)
        {
            _model = model;
            _voxelMeshFactory = voxelMeshFactory;
        }

        /// <summary>
        /// Removes a frame from the model, free dx resources
        /// </summary>
        /// <param name="index"></param>
        public void RemoveFrameAt(int index)
        {
            var vf = _visualFrames[index];

            vf.IndexBuffer.Dispose();
            vf.VertexBuffer.Dispose();
            
            VoxelModel.RemoveFrameAt(index);

            ArrayHelper.RemoveAt(ref _visualFrames, index);
        }

        /// <summary>
        /// Re-generates vertices for a model part frame
        /// </summary>
        /// <param name="frameIndex"></param>
        public void RebuildFrame(int frameIndex)
        {
            List<VertexVoxelInstanced> vertices;
            List<ushort> indices;
            
            _voxelMeshFactory.GenerateVoxelFaces(_model.Frames[frameIndex], out vertices, out indices);
            
            _visualFrames[frameIndex].VertexBuffer.Dispose();
            _visualFrames[frameIndex].IndexBuffer.Dispose();

            _visualFrames[frameIndex].VertexBuffer = _voxelMeshFactory.InitBuffer(vertices);
            _visualFrames[frameIndex].IndexBuffer = _voxelMeshFactory.InitBuffer(indices);

            _visualFrames[frameIndex].BoundingBox = new BoundingBox(new Vector3(), _model.Frames[frameIndex].BlockData.ChunkSize);
        }

        /// <summary>
        /// Performs (re)creation of the vertices
        /// </summary>
        public void BuildMesh()
        {
            if (_visualFrames != null)
            {
                // dispose previous DX data
                foreach (var visualVoxelFrame in _visualFrames)
                {
                    visualVoxelFrame.IndexBuffer.Dispose();
                    visualVoxelFrame.VertexBuffer.Dispose();
                }
            }

            _visualFrames = new VisualVoxelFrame[_model.Frames.Count];

            for (var i = 0; i < _visualFrames.Length; i++)
            {
                var frame = new VisualVoxelFrame();
                
                List<VertexVoxelInstanced> vertices;
                List<ushort> indices;

                _voxelMeshFactory.GenerateVoxelFaces(_model.Frames[i], out vertices, out indices);

                frame.VertexBuffer = _voxelMeshFactory.InitBuffer(vertices);
                frame.IndexBuffer = _voxelMeshFactory.InitBuffer(indices);
                frame.BoundingBox = new BoundingBox(new Vector3(), _model.Frames[i].BlockData.ChunkSize);
                
                _visualFrames[i] = frame;
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
            effect.CBPerModel.Values.Alpha = instance.Alpha;
            effect.CBPerModel.IsDirty = true;

            // draw each part of the model
            for (int i = 0; i < state.PartsStates.Count; i++)
            {
                var voxelModelPartState = state.PartsStates[i];

                if (voxelModelPartState.ActiveFrame == byte.MaxValue)
                    continue;

                var vb = _visualFrames[voxelModelPartState.ActiveFrame].VertexBuffer;
                var ib = _visualFrames[voxelModelPartState.ActiveFrame].IndexBuffer;

                vb.SetToDevice(context, 0);
                ib.SetToDevice(context, 0);

                var frame = _model.Frames[voxelModelPartState.ActiveFrame];

                if (frame.ColorMapping != null)
                {
                    effect.CBPerModel.Values.ColorMapping = frame.ColorMapping.BlockColors;
                    effect.CBPerModel.IsDirty = true;
                }

                if (_model.Parts[i].IsHead)
                {
                    var bb = _visualFrames[voxelModelPartState.ActiveFrame].BoundingBox;
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
            move = Vector3.TransformCoordinate(move, partTransform * Matrix.RotationQuaternion(instance.Rotation));

            result = partTransform *
                     Matrix.RotationQuaternion(instance.Rotation) *
                     Matrix.Translation(-move) *
                     Matrix.RotationQuaternion(Quaternion.Invert(instance.Rotation)) *
                     Matrix.RotationQuaternion(instance.HeadRotation) *
                     Matrix.Translation(move);
        }

        /// <summary>
        /// Draws multiple instances and parts of the same frame in the single draw call
        /// </summary>
        /// <param name="activeFrame"></param>
        /// <param name="partIndex"></param>
        /// <param name="itemsList"></param>
        private void DrawGroup(byte activeFrame, byte partIndex, IList<VoxelModelInstance> itemsList)
        {
            if (activeFrame == byte.MaxValue)
                return;

            // create instance data block that will be passed to the shader
            var instanceData = new VoxelInstanceData[itemsList.Count];

            // update shader instance data by model instance variables
            for (var instanceIndex = 0; instanceIndex < itemsList.Count; instanceIndex++)
            {
                var instance = itemsList[instanceIndex];
                var state = instance.State;
                var voxelModelPartState = state.PartsStates[partIndex];

                instanceData[instanceIndex].LightColor = new Color4( instance.LightColor, instance.SunLightLevel);

                // apply rotations from the state and instance (if the head)
                if (_model.Parts[partIndex].IsHead)
                {
                    var bb = _visualFrames[activeFrame].BoundingBox;
                    RotateHead(voxelModelPartState, instance, bb, out instanceData[instanceIndex].Transform);
                }
                else
                {
                    instanceData[instanceIndex].Transform = voxelModelPartState.GetTransformation() * Matrix.RotationQuaternion(instance.Rotation);
                }

                instanceData[instanceIndex].Transform = Matrix.Transpose(instanceData[instanceIndex].Transform * instance.World);
            }

            var vb = _visualFrames[activeFrame].VertexBuffer;
            var ib = _visualFrames[activeFrame].IndexBuffer;

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
        public void DrawInstanced(DeviceContext context, HLSLVoxelModelInstanced effect, IList<VoxelModelInstance> instances)
        {
            if (!_initialized) 
                return;

            if (instances.Count == 0) 
                return;

            if (_model.ColorMapping != null)
            {
                effect.CBPerModel.Values.ColorMapping = _model.ColorMapping.BlockColors;
                effect.CBPerModel.IsDirty = true;
            }

            effect.Apply(context);

            if (VoxelModel.Frames.Count == 1 && VoxelModel.Parts.Count == 1)
            {
                // we have only one frame and part, so every model have the same VB
                DrawGroup(0, 0, instances);
            }
            else
            {
                for (byte partIndex = 0; partIndex < VoxelModel.Parts.Count; partIndex++)
                {
                    // group instances by an active frame because they have different VB
                    byte index = partIndex;
                    var groups = from inst in instances
                                 group inst by inst.State.PartsStates[index].ActiveFrame
                                 into g select new { FrameIndex = g.Key, Instances = g.AsEnumerable().ToArray() };

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

    public struct GroupItem
    {
        public byte PartIndex;
        public VoxelModelInstance Instance;
    }

    public class VisualVoxelFrame
    {
        public BoundingBox BoundingBox;
        public InstancedVertexBuffer<VertexVoxelInstanced, VoxelInstanceData> VertexBuffer;
        public IndexBuffer<ushort> IndexBuffer;
    }

    public struct VoxelInstanceData : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Matrix Transform;
        public Color4 LightColor;

        static VoxelInstanceData()
        {
            var elements = new[] 
            { 
                new InputElement("POSITION",  0, Format.R8G8B8A8_UInt,      0,                          0, InputClassification.PerVertexData,   0), 
                new InputElement("INFO",      0, Format.R8G8B8A8_UInt,      InputElement.AppendAligned, 0, InputClassification.PerVertexData,   0),
                new InputElement("TRANSFORM", 0, Format.R32G32B32A32_Float, 0,                          1, InputClassification.PerInstanceData, 1), //TRANSFORM Matrix Row0
                new InputElement("TRANSFORM", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1), //TRANSFORM Matrix Row1
                new InputElement("TRANSFORM", 2, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1), //TRANSFORM Matrix Row2
                new InputElement("TRANSFORM", 3, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1), //TRANSFORM Matrix Row3
                new InputElement("COLOR",     0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1)
            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

    }
}
